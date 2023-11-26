using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using UnitystationLauncher.Exceptions;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;

// psst
// You know ECMA-335 right? The specification for the CLI that .NET runs on?
// Yeah, you need it to understand a lot of this code. So get a copy.
// You know the cool thing?
// ISO has a version that has correct PDF metadata so there's an actual table of contents.
// Right here: https://standards.iso.org/ittf/PubliclyAvailableStandards/c058046_ISO_IEC_23271_2012(E).zip

namespace UnitystationLauncher.ContentScanning;

/// <summary>
///     Manages the type white/black list of types and namespaces, and verifies assemblies against them.
/// </summary>
internal static class AssemblyTypeCheckerHelpers
{
    private static readonly bool _parallelReferencedMembersScanning = true;

    internal static Resolver CreateResolver(DirectoryInfo managedPath)
    {
        return new(managedPath);
    }

    internal static string FormatMethodName(MetadataReader reader, MethodDefinition method)
    {
        MethodSignature<MType> methodSig = method.DecodeSignature(new TypeProvider(), 0);
        MTypeDefined type = GetTypeFromDefinition(reader, method.GetDeclaringType());

        return
            $"{type}.{reader.GetString(method.Name)}({string.Join(", ", methodSig.ParameterTypes)}) Returns {methodSig.ReturnType} ";
    }

    internal static void CheckNoUnmanagedMethodDefs(MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        foreach (MethodDefinitionHandle methodDefHandle in reader.MethodDefinitions)
        {
            MethodDefinition methodDef = reader.GetMethodDefinition(methodDefHandle);
            MethodImplAttributes implAttr = methodDef.ImplAttributes;
            MethodAttributes attr = methodDef.Attributes;

            if ((implAttr & MethodImplAttributes.Unmanaged) != 0 ||
                (implAttr & MethodImplAttributes.CodeTypeMask) is not (MethodImplAttributes.IL
                or MethodImplAttributes.Runtime))
            {
                string err = $"Method has illegal MethodImplAttributes: {FormatMethodName(reader, methodDef)}";
                errors.Add(new(err));
            }

            if ((attr & (MethodAttributes.PinvokeImpl | MethodAttributes.UnmanagedExport)) != 0)
            {
                string err = $"Method has illegal MethodAttributes: {FormatMethodName(reader, methodDef)}";
                errors.Add(new(err));
            }
        }
    }

    internal static void CheckNoTypeAbuse(MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        foreach (TypeDefinitionHandle typeDefHandle in reader.TypeDefinitions)
        {
            TypeDefinition typeDef = reader.GetTypeDefinition(typeDefHandle);
            if ((typeDef.Attributes & TypeAttributes.ExplicitLayout) != 0)
            {
                // The C# compiler emits explicit layout types for some array init logic. These have no fields.
                // Only ban explicit layout if it has fields.

                MTypeDefined type = GetTypeFromDefinition(reader, typeDefHandle);

                if (typeDef.GetFields().Count > 0)
                {
                    string err = $"Explicit layout type {type} may not have fields.";
                    errors.Add(new(err));
                }
            }
        }
    }

    internal static List<MTypeReferenced> GetReferencedTypes(MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        return reader.TypeReferences.Select(typeRefHandle =>
            {
                try
                {
                    return ParseTypeReference(reader, typeRefHandle);
                }
                catch (UnsupportedMetadataException e)
                {
                    errors.Add(new(e));
                    return null;
                }
            })
            .Where(p => p != null)
            .ToList()!;
    }

    internal static List<MMemberRef> GetReferencedMembers(MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        if (_parallelReferencedMembersScanning)
        {
            return reader.MemberReferences.AsParallel()
                .Select(memRefHandle =>
                {
                    MemberReference memRef = reader.GetMemberReference(memRefHandle);
                    string memName = reader.GetString(memRef.Name);
                    MType parent;
                    switch (memRef.Parent.Kind)
                    {
                        // See II.22.25 in ECMA-335.
                        case HandleKind.TypeReference:
                            {
                                // Regular type reference.
                                try
                                {
                                    parent = ParseTypeReference(reader, (TypeReferenceHandle)memRef.Parent);
                                }
                                catch (UnsupportedMetadataException u)
                                {
                                    errors.Add(new(u));
                                    return null;
                                }

                                break;
                            }
                        case HandleKind.TypeDefinition:
                            {
                                try
                                {
                                    parent = GetTypeFromDefinition(reader, (TypeDefinitionHandle)memRef.Parent);
                                }
                                catch (UnsupportedMetadataException u)
                                {
                                    errors.Add(new(u));
                                    return null;
                                }

                                break;
                            }
                        case HandleKind.TypeSpecification:
                            {
                                TypeSpecification typeSpec = reader.GetTypeSpecification((TypeSpecificationHandle)memRef.Parent);
                                // Generic type reference.
                                TypeProvider provider = new();
                                parent = typeSpec.DecodeSignature(provider, 0);

                                if (parent.IsCoreTypeDefined())
                                {
                                    // Ensure this isn't a self-defined type.
                                    // This can happen due to generics since MethodSpec needs to point to MemberRef.
                                    return null;
                                }

                                break;
                            }
                        case HandleKind.ModuleReference:
                            {
                                errors.Add(new(
                                    $"Module global variables and methods are unsupported. Name: {memName}"));
                                return null;
                            }
                        case HandleKind.MethodDefinition:
                            {
                                errors.Add(new($"Vararg calls are unsupported. Name: {memName}"));
                                return null;
                            }
                        default:
                            {
                                errors.Add(new(
                                    $"Unsupported member ref parent type: {memRef.Parent.Kind}. Name: {memName}"));
                                return null;
                            }
                    }

                    MMemberRef memberRef;

                    switch (memRef.GetKind())
                    {
                        case MemberReferenceKind.Method:
                            {
                                MethodSignature<MType> sig = memRef.DecodeMethodSignature(new TypeProvider(), 0);

                                memberRef = new MMemberRefMethod(
                                    parent,
                                    memName,
                                    sig.ReturnType,
                                    sig.GenericParameterCount,
                                    sig.ParameterTypes);

                                break;
                            }
                        case MemberReferenceKind.Field:
                            {
                                MType fieldType = memRef.DecodeFieldSignature(new TypeProvider(), 0);
                                memberRef = new MMemberRefField(parent, memName, fieldType);
                                break;
                            }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return memberRef;
                })
                .Where(p => p != null)
                .ToList()!;
        }
        else
        {
            return reader.MemberReferences.Select(memRefHandle =>
                {
                    MemberReference memRef = reader.GetMemberReference(memRefHandle);
                    string memName = reader.GetString(memRef.Name);
                    MType parent;
                    switch (memRef.Parent.Kind)
                    {
                        // See II.22.25 in ECMA-335.
                        case HandleKind.TypeReference:
                            {
                                // Regular type reference.
                                try
                                {
                                    parent = ParseTypeReference(reader, (TypeReferenceHandle)memRef.Parent);
                                }
                                catch (UnsupportedMetadataException u)
                                {
                                    errors.Add(new(u));
                                    return null;
                                }

                                break;
                            }
                        case HandleKind.TypeDefinition:
                            {
                                try
                                {
                                    parent = GetTypeFromDefinition(reader, (TypeDefinitionHandle)memRef.Parent);
                                }
                                catch (UnsupportedMetadataException u)
                                {
                                    errors.Add(new(u));
                                    return null;
                                }

                                break;
                            }
                        case HandleKind.TypeSpecification:
                            {
                                TypeSpecification typeSpec = reader.GetTypeSpecification((TypeSpecificationHandle)memRef.Parent);
                                // Generic type reference.
                                TypeProvider provider = new();
                                parent = typeSpec.DecodeSignature(provider, 0);

                                if (parent.IsCoreTypeDefined())
                                {
                                    // Ensure this isn't a self-defined type.
                                    // This can happen due to generics since MethodSpec needs to point to MemberRef.
                                    return null;
                                }

                                break;
                            }
                        case HandleKind.ModuleReference:
                            {
                                errors.Add(new(
                                    $"Module global variables and methods are unsupported. Name: {memName}"));
                                return null;
                            }
                        case HandleKind.MethodDefinition:
                            {
                                errors.Add(new($"Vararg calls are unsupported. Name: {memName}"));
                                return null;
                            }
                        default:
                            {
                                errors.Add(new(
                                    $"Unsupported member ref parent type: {memRef.Parent.Kind}. Name: {memName}"));
                                return null;
                            }
                    }

                    MMemberRef memberRef;

                    switch (memRef.GetKind())
                    {
                        case MemberReferenceKind.Method:
                            {
                                MethodSignature<MType> sig = memRef.DecodeMethodSignature(new TypeProvider(), 0);

                                memberRef = new MMemberRefMethod(
                                    parent,
                                    memName,
                                    sig.ReturnType,
                                    sig.GenericParameterCount,
                                    sig.ParameterTypes);

                                break;
                            }
                        case MemberReferenceKind.Field:
                            {
                                MType fieldType = memRef.DecodeFieldSignature(new TypeProvider(), 0);
                                memberRef = new MMemberRefField(parent, memName, fieldType);
                                break;
                            }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return memberRef;
                })
                .Where(p => p != null)
                .ToList()!;
        }
    }

    internal static bool ParseInheritType(MType ownerType, EntityHandle handle, [NotNullWhen(true)] out MType? type, MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        type = default;

        switch (handle.Kind)
        {
            case HandleKind.TypeDefinition:
                // Definition to type in same assembly, allowed without hassle.
                return false;

            case HandleKind.TypeReference:
                // Regular type reference.
                try
                {
                    type = ParseTypeReference(reader, (TypeReferenceHandle)handle);
                    return true;
                }
                catch (UnsupportedMetadataException u)
                {
                    errors.Add(new(u));
                    return false;
                }

            case HandleKind.TypeSpecification:
                TypeSpecification typeSpec = reader.GetTypeSpecification((TypeSpecificationHandle)handle);
                // Generic type reference.
                TypeProvider provider = new();
                type = typeSpec.DecodeSignature(provider, 0);

                if (type.IsCoreTypeDefined())
                {
                    // Ensure this isn't a self-defined type.
                    // This can happen due to generics.
                    return false;
                }

                break;

            default:
                errors.Add(new(
                    $"Unsupported BaseType of kind {handle.Kind} on type {ownerType}"));
                return false;
        }

        type = default!;
        return false;
    }

    /// <exception href="UnsupportedMetadataException">
    ///     Thrown if the metadata does something funny we don't "support" like type forwarding.
    /// </exception>
    internal static MTypeReferenced ParseTypeReference(MetadataReader reader, TypeReferenceHandle handle)
    {
        TypeReference typeRef = reader.GetTypeReference(handle);
        string name = reader.GetString(typeRef.Name);
        string? nameSpace = NilNullString(reader, typeRef.Namespace);
        MResScope resScope;

        // See II.22.38 in ECMA-335
        if (typeRef.ResolutionScope.IsNil)
        {
            throw new UnsupportedMetadataException(
                $"Null resolution scope on type Name: {nameSpace}.{name}. This indicates exported/forwarded types");
        }

        switch (typeRef.ResolutionScope.Kind)
        {
            case HandleKind.AssemblyReference:
                {
                    // Different assembly.
                    AssemblyReference assemblyRef =
                        reader.GetAssemblyReference((AssemblyReferenceHandle)typeRef.ResolutionScope);
                    string assemblyName = reader.GetString(assemblyRef.Name);
                    resScope = new MResScopeAssembly(assemblyName);
                    break;
                }
            case HandleKind.TypeReference:
                {
                    // Nested type.
                    MTypeReferenced enclosingType = ParseTypeReference(reader, (TypeReferenceHandle)typeRef.ResolutionScope);
                    resScope = new MResScopeType(enclosingType);
                    break;
                }
            case HandleKind.ModuleReference:
                {
                    // Same-assembly-different-module
                    throw new UnsupportedMetadataException(
                        $"Cross-module reference to type {nameSpace}.{name}. ");
                }
            default:
                // Edge cases not handled:
                // https://github.com/dotnet/runtime/blob/b2e5a89085fcd87e2fa9300b4bb00cd499c5845b/src/libraries/System.Reflection.Metadata/tests/Metadata/Decoding/DisassemblingTypeProvider.cs#L130-L132
                throw new UnsupportedMetadataException(
                    $"TypeRef to {typeRef.ResolutionScope.Kind} for type {nameSpace}.{name}");
        }

        return new(resScope, name, nameSpace);
    }

    internal static string? NilNullString(MetadataReader reader, StringHandle handle)
    {
        return handle.IsNil ? null : reader.GetString(handle);
    }



    internal static MTypeDefined GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle)
    {
        TypeDefinition typeDef = reader.GetTypeDefinition(handle);
        string name = reader.GetString(typeDef.Name);
        string? ns = NilNullString(reader, typeDef.Namespace);
        MTypeDefined? enclosing = null;
        if (typeDef.IsNested)
        {
            enclosing = GetTypeFromDefinition(reader, typeDef.GetDeclaringType());
        }

        return new(name, ns, enclosing);
    }
}