using System;
using System.IO;
using System.Reflection.Metadata;
using ILVerify;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.ConfigFile;
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
    internal static Resolver CreateResolver(DirectoryInfo managedPath)
    {
        return new(managedPath);
    }

    internal static bool CheckVerificationResult(SandboxConfig loadedCfg, VerificationResult res, string name, MetadataReader reader, Action<string> logErrors)
    {
        if (loadedCfg.AllowedVerifierErrors.Contains(res.Code))
        {
            return false;
        }

        string formatted = res.Args == null ? res.Message : string.Format(res.Message, res.Args);
        string msg = $"{name}: ILVerify: {formatted}";

        if (!res.Method.IsNil)
        {
            MethodDefinition method = reader.GetMethodDefinition(res.Method);
            string methodName = reader.FormatMethodName(method);

            msg = $"{msg}, method: {methodName}";
        }

        if (!res.Type.IsNil)
        {
            MTypeDefined type = reader.GetTypeFromDefinition(res.Type);
            msg = $"{msg}, type: {type}";
        }

        logErrors.Invoke(msg);
        return true;
    }


}