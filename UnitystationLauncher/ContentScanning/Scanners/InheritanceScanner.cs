using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;
using UnitystationLauncher.Models.Enums;

namespace UnitystationLauncher.ContentScanning.Scanners;

internal static class InheritanceScanner
{
    internal static void CheckInheritance(
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
                errors.Add(new($"Inheriting of type not allowed: {baseType}"));
            }

            foreach (MType @interface in interfaces)
            {
                if (CanInherit(@interface) == false)
                {
                    errors.Add(new($"Implementing of interface not allowed: {@interface}"));
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

                if (realBaseType.IsTypeAccessAllowed(sandboxConfig, out TypeConfig? cfg) == false)
                {
                    return false;
                }

                return cfg.Inherit != InheritMode.Block && (cfg.Inherit == InheritMode.Allow || cfg.All);
            }
        }
    }
}