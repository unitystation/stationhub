using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using ILVerify;
using MoreLinq;
using UnitystationLauncher.ContentScanning;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

// psst
// You know ECMA-335 right? The specification for the CLI that .NET runs on?
// Yeah, you need it to understand a lot of this code. So get a copy.
// You know the cool thing?
// ISO has a version that has correct PDF metadata so there's an actual table of contents.
// Right here: https://standards.iso.org/ittf/PubliclyAvailableStandards/c058046_ISO_IEC_23271_2012(E).zip

namespace UnitystationLauncher.Services;

/// <summary>
///     Manages the type white/black list of types and namespaces, and verifies assemblies against them.
/// </summary>
public sealed partial class AssemblyTypeCheckerService : IAssemblyTypeCheckerService
{
    /// <summary>
    ///     Completely disables type checking, allowing everything.
    /// </summary>
    public bool DisableTypeCheck { get; init; }

    public DumpFlags Dump { get; init; } = DumpFlags.None;
    public bool VerifyIl { get; init; }

    private readonly Task<SandboxConfig> _config;

    private readonly IEnvironmentService _environmentService;


    private readonly ICodeScanConfigService _codeScanConfigService;

    private readonly HttpClient _httpClient;

    public AssemblyTypeCheckerService(IEnvironmentService environmentService, HttpClient httpClient, ICodeScanConfigService codeScanConfigService)
    {
        _environmentService = environmentService;
        VerifyIl = true;
        DisableTypeCheck = false;
        _httpClient = httpClient;
        _codeScanConfigService = codeScanConfigService;
        _config = Task.Run(_codeScanConfigService.LoadConfigAsync);
    }

    /// <summary>
    ///     Check the assembly for any illegal types. Any types not on the white list
    ///     will cause the assembly to be rejected.
    /// </summary>
    /// <param name="diskPath"></param>
    /// <param name="managedPath"></param>
    /// <param name="otherAssemblies"></param>
    /// <param name="info"></param>
    /// <param name="Errors"></param>
    /// <param name="assembly">Assembly to load.</param>
    /// <returns></returns>
    public bool CheckAssembly(FileInfo diskPath, DirectoryInfo managedPath, List<string> otherAssemblies,
        Action<string> info, Action<string> Errors)
    {
        using FileStream assembly = diskPath.OpenRead();
        Stopwatch fullStopwatch = Stopwatch.StartNew();

        Resolver resolver = AssemblyTypeCheckerHelpers.CreateResolver(managedPath);
        using PEReader peReader = new PEReader(assembly, PEStreamOptions.LeaveOpen);
        MetadataReader reader = peReader.GetMetadataReader();

        string asmName = reader.GetString(reader.GetAssemblyDefinition().Name);

        if (peReader.PEHeaders.CorHeader?.ManagedNativeHeaderDirectory is { Size: not 0 })
        {
            Errors.Invoke($"Assembly {asmName} contains native code.");
            return false;
        }

        if (VerifyIl)
        {
            if (DoVerifyIL(asmName, resolver, peReader, reader, info, Errors) == false)
            {
                Errors.Invoke($"Assembly {asmName} Has invalid IL code");
                return false;
            }
        }


        ConcurrentBag<SandboxError> errors = new ConcurrentBag<SandboxError>();

        List<MTypeReferenced> types = AssemblyTypeCheckerHelpers.GetReferencedTypes(reader, errors);
        List<MMemberRef> members = AssemblyTypeCheckerHelpers.GetReferencedMembers(reader, errors);
        List<(MType type, MType parent, ArraySegment<MType> interfaceImpls)> inherited = GetExternalInheritedTypes(reader, errors);
        info.Invoke($"References loaded... {fullStopwatch.ElapsedMilliseconds}ms");

        if (DisableTypeCheck)
        {
            resolver.Dispose();
            peReader.Dispose();
            return true;
        }


        SandboxConfig loadedConfig = _config.Result;

        loadedConfig.MultiAssemblyOtherReferences.Clear();
        loadedConfig.MultiAssemblyOtherReferences.AddRange(otherAssemblies);

        // We still do explicit type reference scanning, even though the actual whitelists work with raw members.
        // This is so that we can simplify handling of generic type specifications during member checking:
        // we won't have to check that any types in their type arguments are whitelisted.
        foreach (MTypeReferenced type in types)
        {
            if (IsTypeAccessAllowed(loadedConfig, type, out _) == false)
            {
                errors.Add(new SandboxError($"Access to type not allowed: {type} asmName {asmName}"));
            }
        }

        info.Invoke($"Types... {fullStopwatch.ElapsedMilliseconds}ms");

        CheckInheritance(loadedConfig, inherited, errors);

        info.Invoke($"Inheritance... {fullStopwatch.ElapsedMilliseconds}ms");

        AssemblyTypeCheckerHelpers.CheckNoUnmanagedMethodDefs(reader, errors);

        info.Invoke($"Unmanaged methods... {fullStopwatch.ElapsedMilliseconds}ms");

        AssemblyTypeCheckerHelpers.CheckNoTypeAbuse(reader, errors);

        info.Invoke($"Type abuse... {fullStopwatch.ElapsedMilliseconds}ms");

        CheckMemberReferences(loadedConfig, members, errors);

        errors = new ConcurrentBag<SandboxError>(errors.OrderBy(x => x.Message));

        foreach (SandboxError error in errors)
        {
            Errors.Invoke($"Sandbox violation: {error.Message}");
        }

        info.Invoke($"Checked assembly in {fullStopwatch.ElapsedMilliseconds}ms");
        resolver.Dispose();
        peReader.Dispose();
        return errors.IsEmpty;
    }

    private bool DoVerifyIL(
        string name,
        IResolver resolver,
        PEReader peReader,
        MetadataReader reader,
        Action<string> info,
        Action<string> logErrors)
    {
        info.Invoke($"{name}: Verifying IL...");
        Stopwatch sw = Stopwatch.StartNew();
        ConcurrentBag<VerificationResult> bag = new ConcurrentBag<VerificationResult>();


        bool UesParallel = false;

        if (UesParallel)
        {
            OrderablePartitioner<TypeDefinitionHandle> partitioner = Partitioner.Create(reader.TypeDefinitions);
            Parallel.ForEach(partitioner.GetPartitions(Environment.ProcessorCount), handle =>
            {
                Verifier ver = new Verifier(resolver);
                ver.SetSystemModuleName(new AssemblyName(AssemblyTypeCheckerHelpers.SystemAssemblyName));
                while (handle.MoveNext())
                {
                    foreach (VerificationResult? result in ver.Verify(peReader, handle.Current, verifyMethods: true))
                    {
                        bag.Add(result);
                    }
                }
            });
        }
        else
        {
            Verifier ver = new Verifier(resolver);
            //mscorlib
            ver.SetSystemModuleName(new AssemblyName(AssemblyTypeCheckerHelpers.SystemAssemblyName));
            foreach (TypeDefinitionHandle Definition in reader.TypeDefinitions)
            {
                IEnumerable<VerificationResult> Errors = ver.Verify(peReader, Definition, verifyMethods: true);
                foreach (VerificationResult? Error in Errors)
                {
                    bag.Add(Error);
                }
            }
        }

        SandboxConfig loadedCfg = _config.Result;

        bool verifyErrors = false;
        foreach (VerificationResult res in bag)
        {
            if (loadedCfg.AllowedVerifierErrors.Contains(res.Code))
            {
                continue;
            }

            string formatted = res.Args == null ? res.Message : string.Format(res.Message, res.Args);
            string msg = $"{name}: ILVerify: {formatted}";

            if (!res.Method.IsNil)
            {
                MethodDefinition method = reader.GetMethodDefinition(res.Method);
                string methodName = AssemblyTypeCheckerHelpers.FormatMethodName(reader, method);

                msg = $"{msg}, method: {methodName}";
            }

            if (!res.Type.IsNil)
            {
                MTypeDefined type = AssemblyTypeCheckerHelpers.GetTypeFromDefinition(reader, res.Type);
                msg = $"{msg}, type: {type}";
            }


            verifyErrors = true;
            logErrors.Invoke(msg);
        }

        info.Invoke($"{name}: Verified IL in {sw.Elapsed.TotalMilliseconds}ms");

        if (verifyErrors)
        {
            return false;
        }

        return true;
    }

    private void CheckMemberReferences(
        SandboxConfig sandboxConfig,
        List<MMemberRef> members,
        ConcurrentBag<SandboxError> errors)
    {
        bool IsParallel = true;

        if (IsParallel)
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

                if (IsTypeAccessAllowed(sandboxConfig, baseTypeReferenced, out TypeConfig? typeCfg) == false)
                {
                    // Technically this error isn't necessary since we have an earlier pass
                    // checking all referenced types. That should have caught this
                    // We still need the typeCfg so that's why we're checking. Might as well.
                    errors.Add(new SandboxError($"Access to type not allowed: {baseTypeReferenced}"));
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

                            errors.Add(new SandboxError($"Access to field not allowed: {mMemberRefField}"));
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

                        errors.Add(new SandboxError($"Access to method not allowed: {mMemberRefMethod}"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(memberRef));
                }
            });
        }
        else
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

                if (IsTypeAccessAllowed(sandboxConfig, baseTypeReferenced, out TypeConfig? typeCfg) == false)
                {
                    // Technically this error isn't necessary since we have an earlier pass
                    // checking all referenced types. That should have caught this
                    // We still need the typeCfg so that's why we're checking. Might as well.
                    errors.Add(new SandboxError($"Access to type not allowed: {baseTypeReferenced}"));
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

                            errors.Add(new SandboxError($"Access to field not allowed: {mMemberRefField}"));
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

                        errors.Add(new SandboxError($"Access to method not allowed: {mMemberRefMethod}"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(memberRef));
                }
            }
        }
    }

    private void CheckInheritance(
        SandboxConfig sandboxConfig,
        List<(MType type, MType parent, ArraySegment<MType> interfaceImpls)> inherited,
        ConcurrentBag<SandboxError> errors)
    {
        // This inheritance whitelisting primarily serves to avoid content doing funny stuff
        // by e.g. inheriting Type.
        foreach ((MType _, MType baseType, ArraySegment<MType> interfaces) in inherited)
        {
            if (CanInherit(baseType) == false)
            {
                errors.Add(new SandboxError($"Inheriting of type not allowed: {baseType}"));
            }

            foreach (MType @interface in interfaces)
            {
                if (CanInherit(@interface) == false)
                {
                    errors.Add(new SandboxError($"Implementing of interface not allowed: {@interface}"));
                }
            }

            bool CanInherit(MType inheritType)
            {
                MTypeReferenced realBaseType = inheritType switch
                {
                    MTypeGeneric generic => (MTypeReferenced)generic.GenericType,
                    MTypeReferenced referenced => referenced,
                    _ => throw new InvalidOperationException() // Can't happen.
                };

                if (IsTypeAccessAllowed(sandboxConfig, realBaseType, out TypeConfig? cfg) == false)
                {
                    return false;
                }

                return cfg.Inherit != InheritMode.Block && (cfg.Inherit == InheritMode.Allow || cfg.All);
            }
        }
    }

    private bool IsTypeAccessAllowed(SandboxConfig sandboxConfig, MTypeReferenced type,
        [NotNullWhen(true)] out TypeConfig? cfg)
    {
        if (type.Namespace == null)
        {
            if (type.ResolutionScope is MResScopeType parentType)
            {
                if (IsTypeAccessAllowed(sandboxConfig, (MTypeReferenced)parentType.Type, out TypeConfig? parentCfg) == false)
                {
                    cfg = null;
                    return false;
                }

                if (parentCfg.All)
                {
                    // Enclosing type is namespace-whitelisted so we don't have to check anything else.
                    cfg = TypeConfig.DefaultAll;
                    return true;
                }

                // Found enclosing type, checking if we are allowed to access this nested type.
                // Also pass it up in case of multiple nested types.
                if (parentCfg.NestedTypes != null && parentCfg.NestedTypes.TryGetValue(type.Name, out cfg))
                {
                    return true;
                }

                cfg = null;
                return false;
            }

            if (type.ResolutionScope is MResScopeAssembly mResScopeAssembly &&
                sandboxConfig.MultiAssemblyOtherReferences.Contains(mResScopeAssembly.Name))
            {
                cfg = TypeConfig.DefaultAll;
                return true;
            }

            // Types without namespaces or nesting parent are not allowed at all.
            cfg = null;
            return false;
        }

        // Check if in whitelisted namespaces.
        foreach (string whNamespace in sandboxConfig.WhitelistedNamespaces)
        {
            if (type.Namespace.StartsWith(whNamespace))
            {
                cfg = TypeConfig.DefaultAll;
                return true;
            }
        }

        if (type.ResolutionScope is MResScopeAssembly resScopeAssembly &&
            sandboxConfig.MultiAssemblyOtherReferences.Contains(resScopeAssembly.Name))
        {
            cfg = TypeConfig.DefaultAll;
            return true;
        }


        if (sandboxConfig.Types.TryGetValue(type.Namespace, out Dictionary<string, TypeConfig>? nsDict) == false)
        {
            cfg = null;
            return false;
        }

        return nsDict.TryGetValue(type.Name, out cfg);
    }

    private List<(MType type, MType parent, ArraySegment<MType> interfaceImpls)> GetExternalInheritedTypes(
        MetadataReader reader,
        ConcurrentBag<SandboxError> errors)
    {
        List<(MType, MType, ArraySegment<MType>)> list = new List<(MType, MType, ArraySegment<MType>)>();
        foreach (TypeDefinitionHandle typeDefHandle in reader.TypeDefinitions)
        {
            TypeDefinition typeDef = reader.GetTypeDefinition(typeDefHandle);
            ArraySegment<MType> interfaceImpls;
            MTypeDefined type = AssemblyTypeCheckerHelpers.GetTypeFromDefinition(reader, typeDefHandle);

            if (!AssemblyTypeCheckerHelpers.ParseInheritType(type, typeDef.BaseType, out MType? parent, reader, errors))
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

                    if (AssemblyTypeCheckerHelpers.ParseInheritType(type, interfaceImpl.Interface, out MType? implemented, reader, errors))
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
}