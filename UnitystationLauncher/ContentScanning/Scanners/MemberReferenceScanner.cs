using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;

namespace UnitystationLauncher.ContentScanning.Scanners;

internal static class MemberReferenceScanner
{
    private static readonly bool _parallelMemberReferencesScanning = true;

    public static void CheckMemberReferences(SandboxConfig sandboxConfig, List<MMemberRef> members, ConcurrentBag<SandboxError> errors)
    {
        // TODO: We should probably just pick one of these and remove the other
        if (_parallelMemberReferencesScanning)
        {
            ParallelCheckMemberReferences(sandboxConfig, members, errors);
        }
        else
        {
            NonParallelCheckMemberReferences(sandboxConfig, members, errors);
        }
    }


    private static void ParallelCheckMemberReferences(SandboxConfig sandboxConfig, List<MMemberRef> members, ConcurrentBag<SandboxError> errors)
    {
        Parallel.ForEach(members, memberRef =>
        {
            MType baseType = memberRef.ParentType;
            while (!(baseType is MTypeReferenced))
            {
                switch (baseType)
                {
                    case MTypeGeneric generic:
                        {
                            baseType = generic.GenericType;

                            break;
                        }
                    case MTypeWackyArray:
                        {
                            // Members on arrays (not to be confused with vectors) are all fine.
                            // See II.14.2 in ECMA-335.
                            return;
                        }
                    case MTypeDefined:
                        {
                            // Valid for this to show up, safe to ignore.
                            return;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }

            MTypeReferenced baseTypeReferenced = (MTypeReferenced)baseType;

            if (baseTypeReferenced.IsTypeAccessAllowed(sandboxConfig, out TypeConfig? typeCfg) == false)
            {
                // Technically this error isn't necessary since we have an earlier pass
                // checking all referenced types. That should have caught this
                // We still need the typeCfg so that's why we're checking. Might as well.
                errors.Add(new($"Access to type not allowed: {baseTypeReferenced}"));
                return;
            }

            if (typeCfg.All)
            {
                // Fully whitelisted for the type, we good.
                return;
            }

            switch (memberRef)
            {
                case MMemberRefField mMemberRefField:
                    {
                        foreach (WhitelistFieldDefine field in typeCfg.FieldsParsed)
                        {
                            if (field.Name == mMemberRefField.Name &&
                                mMemberRefField.FieldType.WhitelistEquals(field.FieldType))
                            {
                                return; // Found
                            }
                        }

                        errors.Add(new($"Access to field not allowed: {mMemberRefField}"));
                        break;
                    }
                case MMemberRefMethod mMemberRefMethod:
                    foreach (WhitelistMethodDefine parsed in typeCfg.MethodsParsed)
                    {
                        bool notParamMismatch = true;

                        if (parsed.Name == mMemberRefMethod.Name &&
                            mMemberRefMethod.ReturnType.WhitelistEquals(parsed.ReturnType) &&
                            mMemberRefMethod.ParameterTypes.Length == parsed.ParameterTypes.Count &&
                            mMemberRefMethod.GenericParameterCount == parsed.GenericParameterCount)
                        {
                            for (int i = 0; i < mMemberRefMethod.ParameterTypes.Length; i++)
                            {
                                MType a = mMemberRefMethod.ParameterTypes[i];
                                MType b = parsed.ParameterTypes[i];

                                if (a.WhitelistEquals(b) == false)
                                {
                                    notParamMismatch = false;
                                    break;
                                }
                            }

                            if (notParamMismatch)
                            {
                                return; // Found
                            }
                        }
                    }

                    errors.Add(new($"Access to method not allowed: {mMemberRefMethod}"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberRef));
            }
        });
    }

    private static void NonParallelCheckMemberReferences(SandboxConfig sandboxConfig, List<MMemberRef> members, ConcurrentBag<SandboxError> errors)
    {
        foreach (MMemberRef memberRef in members)
        {
            MType baseType = memberRef.ParentType;
            while (!(baseType is MTypeReferenced))
            {
                switch (baseType)
                {
                    case MTypeGeneric generic:
                        {
                            baseType = generic.GenericType;

                            break;
                        }
                    case MTypeWackyArray:
                        {
                            // Members on arrays (not to be confused with vectors) are all fine.
                            // See II.14.2 in ECMA-335.
                            continue;
                        }
                    case MTypeDefined:
                        {
                            // Valid for this to show up, safe to ignore.
                            continue;
                        }
                    default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                }
            }

            MTypeReferenced baseTypeReferenced = (MTypeReferenced)baseType;

            if (baseTypeReferenced.IsTypeAccessAllowed(sandboxConfig, out TypeConfig? typeCfg) == false)
            {
                // Technically this error isn't necessary since we have an earlier pass
                // checking all referenced types. That should have caught this
                // We still need the typeCfg so that's why we're checking. Might as well.
                errors.Add(new($"Access to type not allowed: {baseTypeReferenced}"));
                continue;
            }

            if (typeCfg.All)
            {
                // Fully whitelisted for the type, we good.
                continue;
            }

            switch (memberRef)
            {
                case MMemberRefField mMemberRefField:
                    {
                        foreach (WhitelistFieldDefine field in typeCfg.FieldsParsed)
                        {
                            if (field.Name == mMemberRefField.Name &&
                                mMemberRefField.FieldType.WhitelistEquals(field.FieldType))
                            {
                                continue; // Found
                            }
                        }

                        errors.Add(new($"Access to field not allowed: {mMemberRefField}"));
                        break;
                    }
                case MMemberRefMethod mMemberRefMethod:
                    bool notParamMismatch = true;
                    foreach (WhitelistMethodDefine parsed in typeCfg.MethodsParsed)
                    {
                        if (parsed.Name == mMemberRefMethod.Name &&
                            mMemberRefMethod.ReturnType.WhitelistEquals(parsed.ReturnType) &&
                            mMemberRefMethod.ParameterTypes.Length == parsed.ParameterTypes.Count &&
                            mMemberRefMethod.GenericParameterCount == parsed.GenericParameterCount)
                        {
                            for (int i = 0; i < mMemberRefMethod.ParameterTypes.Length; i++)
                            {
                                MType a = mMemberRefMethod.ParameterTypes[i];
                                MType b = parsed.ParameterTypes[i];

                                if (!a.WhitelistEquals(b))
                                {
                                    notParamMismatch = false;
                                    break;

                                }
                            }

                            if (notParamMismatch)
                            {
                                break; // Found
                            }
                            break;
                        }
                    }

                    if (notParamMismatch == false)
                    {
                        continue;
                    }

                    errors.Add(new($"Access to method not allowed: {mMemberRefMethod}"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberRef));
            }
        }
    }
}