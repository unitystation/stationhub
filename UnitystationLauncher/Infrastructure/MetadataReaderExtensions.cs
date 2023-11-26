using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using UnitystationLauncher.ContentScanning;
using UnitystationLauncher.ContentScanning.Scanners;
using UnitystationLauncher.Exceptions;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;

namespace UnitystationLauncher.Infrastructure;

internal static class MetadataReaderExtensions
{
    private static readonly bool _parallelReferencedMembersScanning = true;

    internal static List<(MType type, MType parent, ArraySegment<MType> interfaceImpls)> GetExternalInheritedTypes(
        this MetadataReader reader,
        ConcurrentBag<SandboxError> errors)
    {
        List<(MType, MType, ArraySegment<MType>)> list = new();
        foreach (TypeDefinitionHandle typeDefHandle in reader.TypeDefinitions)
        {
            TypeDefinition typeDef = reader.GetTypeDefinition(typeDefHandle);
            ArraySegment<MType> interfaceImpls;
            MTypeDefined type = reader.GetTypeFromDefinition(typeDefHandle);

            if (!type.ParseInheritType(typeDef.BaseType, out MType? parent, reader, errors))
            {
                continue;
            }

            InterfaceImplementationHandleCollection interfaceImplsCollection = typeDef.GetInterfaceImplementations();
            if (interfaceImplsCollection.Count == 0)
            {
                interfaceImpls = Array.Empty<MType>();
            }
            else
            {
                interfaceImpls = new MType[interfaceImplsCollection.Count];
                int i = 0;
                foreach (InterfaceImplementationHandle implHandle in interfaceImplsCollection)
                {
                    InterfaceImplementation interfaceImpl = reader.GetInterfaceImplementation(implHandle);

                    if (type.ParseInheritType(interfaceImpl.Interface, out MType? implemented, reader, errors))
                    {
                        interfaceImpls[i++] = implemented;
                    }
                }

                interfaceImpls = interfaceImpls[..i];
            }

            list.Add((type, parent, interfaceImpls));
        }

        return list;
    }

    internal static List<MTypeReferenced> GetReferencedTypes(this MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        return reader.TypeReferences.Select(typeRefHandle =>
            {
                try
                {
                    return reader.ParseTypeReference(typeRefHandle);
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

    /// <exception href="UnsupportedMetadataException">
    ///     Thrown if the metadata does something funny we don't "support" like type forwarding.
    /// </exception>
    internal static MTypeReferenced ParseTypeReference(this MetadataReader reader, TypeReferenceHandle handle)
    {
        TypeReference typeRef = reader.GetTypeReference(handle);
        string name = reader.GetString(typeRef.Name);
        string? nameSpace = typeRef.Namespace.NilNullString(reader);
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

    internal static List<MMemberRef> GetReferencedMembers(this MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        if (_parallelReferencedMembersScanning)
        {
            return ReferencedMembersScanner.ParallelReferencedMembersCheck(reader, errors);
        }
        else
        {
            return ReferencedMembersScanner.NonParallelReferencedMembersCheck(reader, errors);
        }
    }

    internal static MTypeDefined GetTypeFromDefinition(this MetadataReader reader, TypeDefinitionHandle handle)
    {
        TypeDefinition typeDef = reader.GetTypeDefinition(handle);
        string name = reader.GetString(typeDef.Name);
        string? ns = typeDef.Namespace.NilNullString(reader);
        MTypeDefined? enclosing = null;
        if (typeDef.IsNested)
        {
            enclosing = reader.GetTypeFromDefinition(typeDef.GetDeclaringType());
        }

        return new(name, ns, enclosing);
    }

    internal static string FormatMethodName(this MetadataReader reader, MethodDefinition method)
    {
        MethodSignature<MType> methodSig = method.DecodeSignature(new TypeProvider(), 0);
        MTypeDefined type = reader.GetTypeFromDefinition(method.GetDeclaringType());

        return
            $"{type}.{reader.GetString(method.Name)}({string.Join(", ", methodSig.ParameterTypes)}) Returns {methodSig.ReturnType} ";
    }
}