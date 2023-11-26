using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using UnitystationLauncher.Exceptions;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;

namespace UnitystationLauncher.ContentScanning.Scanners;

internal static class ReferencedMembersScanner
{
    internal static List<MMemberRef> ParallelReferencedMembersCheck(MetadataReader reader, ConcurrentBag<SandboxError> errors)
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
                                    parent = reader.ParseTypeReference((TypeReferenceHandle)memRef.Parent);
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
                                    parent = reader.GetTypeFromDefinition((TypeDefinitionHandle)memRef.Parent);
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

    internal static List<MMemberRef> NonParallelReferencedMembersCheck(MetadataReader reader, ConcurrentBag<SandboxError> errors)
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
                                    parent = reader.ParseTypeReference((TypeReferenceHandle)memRef.Parent);
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
                                    parent = reader.GetTypeFromDefinition((TypeDefinitionHandle)memRef.Parent);
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