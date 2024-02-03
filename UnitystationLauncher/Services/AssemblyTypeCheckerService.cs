﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using UnitystationLauncher.ContentScanning;
using UnitystationLauncher.ContentScanning.Scanners;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;

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
public sealed class AssemblyTypeCheckerService : IAssemblyTypeCheckerService
{
    private readonly Task<SandboxConfig> _config;

    public AssemblyTypeCheckerService(ICodeScanConfigService codeScanConfigService)
    {
        _config = codeScanConfigService.LoadConfigAsync();
    }

    /// <summary>
    ///     Check the assembly for any illegal types. Any types not on the white list
    ///     will cause the assembly to be rejected.
    /// </summary>
    /// <param name="diskPath"></param>
    /// <param name="managedPath"></param>
    /// <param name="otherAssemblies"></param>
    /// <param name="scanLog"></param>
    /// <returns></returns>
    public async Task<bool> CheckAssemblyTypesAsync(FileInfo diskPath, DirectoryInfo managedPath, List<string> otherAssemblies, Action<ScanLog> scanLog)
    {
        await using FileStream assembly = diskPath.OpenRead();
        Stopwatch fullStopwatch = Stopwatch.StartNew();

        Resolver resolver = AssemblyTypeCheckerHelpers.CreateResolver(managedPath);
        using PEReader peReader = new(assembly, PEStreamOptions.LeaveOpen);
        MetadataReader reader = peReader.GetMetadataReader();

        string asmName = reader.GetString(reader.GetAssemblyDefinition().Name);

        // Check for native code
        if (peReader.PEHeaders.CorHeader?.ManagedNativeHeaderDirectory is { Size: not 0 })
        {
            scanLog.Invoke(new()
            {
                Type = ScanLog.LogType.Error,
                LogMessage = $"Assembly {asmName} contains native code."
            });

            return false;
        }

        // Verify the IL
        if (ILScanner.IsILValid(asmName, resolver, peReader, reader, scanLog, await _config) == false)
        {
            scanLog.Invoke(new()
            {
                Type = ScanLog.LogType.Error,
                LogMessage = $"Assembly {asmName} Has invalid IL code"
            });

            return false;
        }

        ConcurrentBag<SandboxError> errors = new();

        // Load all the references
        List<MTypeReferenced> types = reader.GetReferencedTypes(errors);
        List<MMemberRef> members = reader.GetReferencedMembers(errors);
        List<(MType type, MType parent, ArraySegment<MType> interfaceImpls)> inherited = reader.GetExternalInheritedTypes(errors);

        scanLog.Invoke(new()
        {
            LogMessage = errors.IsEmpty ? "No sandbox violations." : $"Total violations: {errors.Count}"
        });
        scanLog.Invoke(new()
        {
            LogMessage = $"References loaded... {fullStopwatch.ElapsedMilliseconds}ms"
        });

        SandboxConfig loadedConfig = await _config;

        loadedConfig.MultiAssemblyOtherReferences.Clear();
        loadedConfig.MultiAssemblyOtherReferences.AddRange(otherAssemblies);

        // We still do explicit type reference scanning, even though the actual whitelists work with raw members.
        // This is so that we can simplify handling of generic type specifications during member checking:
        // we won't have to check that any types in their type arguments are whitelisted.
        foreach (MTypeReferenced type in types.Where(type => type.IsTypeAccessAllowed(loadedConfig, out _) == false))
        {
            errors.Add(new($"Access to type not allowed: {type} asmName {asmName}"));
        }

        scanLog.Invoke(new()
        {
            LogMessage = errors.IsEmpty ? "No sandbox violations." : $"Total violations: {errors.Count}"
        });
        scanLog.Invoke(new()
        {
            LogMessage = $"Types... {fullStopwatch.ElapsedMilliseconds}ms"
        });

        InheritanceScanner.CheckInheritance(loadedConfig, inherited, errors);
        scanLog.Invoke(new()
        {
            LogMessage = errors.IsEmpty ? "No sandbox violations." : $"Total violations: {errors.Count}"
        });
        scanLog.Invoke(new()
        {
            LogMessage = $"Inheritance... {fullStopwatch.ElapsedMilliseconds}ms"
        });

        UnmanagedMethodScanner.CheckNoUnmanagedMethodDefs(reader, errors);
        scanLog.Invoke(new()
        {
            LogMessage = errors.IsEmpty ? "No sandbox violations." : $"Total violations: {errors.Count}"
        });
        scanLog.Invoke(new()
        {
            LogMessage = $"Unmanaged methods... {fullStopwatch.ElapsedMilliseconds}ms"
        });

        TypeAbuseScanner.CheckNoTypeAbuse(reader, errors);
        scanLog.Invoke(new()
        {
            LogMessage = errors.IsEmpty ? "No sandbox violations." : $"Total violations: {errors.Count}"
        });
        scanLog.Invoke(new()
        {
            LogMessage = $"Type abuse... {fullStopwatch.ElapsedMilliseconds}ms"
        });

        MemberReferenceScanner.CheckMemberReferences(loadedConfig, members, errors);
        scanLog.Invoke(new()
        {
            LogMessage = errors.IsEmpty ? "No sandbox violations." : $"Total violations: {errors.Count}"
        });
        scanLog.Invoke(new()
        {
            LogMessage = $"Member References... {fullStopwatch.ElapsedMilliseconds}ms"
        });

        errors = new(errors.OrderBy(x => x.Message));

        foreach (SandboxError error in errors)
        {
            scanLog.Invoke(new()
            {
                Type = ScanLog.LogType.Error,
                LogMessage = $"Sandbox violation: {error.Message}"
            });
        }

        scanLog.Invoke(new()
        {
            LogMessage = errors.IsEmpty ? "No sandbox violations." : $"Total violations: {errors.Count}"
        });
        scanLog.Invoke(new()
        {
            LogMessage = $"Checked assembly in {fullStopwatch.ElapsedMilliseconds}ms"
        });

        resolver.Dispose();
        return errors.IsEmpty;
    }
}