using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed record MTypeReferenced(MResScope ResolutionScope, string Name, string? Namespace) : MType
{
    public override string ToString()
    {
        if (Namespace == null)
        {
            return $"{ResolutionScope}{Name}";
        }

        return $"{ResolutionScope}{Namespace}.{Name}";
    }

    public override string WhitelistToString()
    {
        if (Namespace == null)
        {
            return Name;
        }

        return $"{Namespace}.{Name}";
    }

    public override bool WhitelistEquals(MType other)
    {
        return other switch
        {
            MTypeParsed p => p.WhitelistEquals(this),
            // TODO: ResolutionScope doesn't actually implement equals
            // This is fine since we're not comparing these anywhere
            MTypeReferenced r => r.Namespace == Namespace && r.Name == Name &&
                                  r.ResolutionScope.Equals(ResolutionScope),
            _ => false
        };
    }

    public bool IsTypeAccessAllowed(SandboxConfig sandboxConfig, [NotNullWhen(true)] out TypeConfig? cfg)
    {
        if (Namespace == null)
        {
            if (ResolutionScope is MResScopeType parentType)
            {
                if (parentType.Type is MTypeReferenced parentReferencedType)
                {
                    if (parentReferencedType.IsTypeAccessAllowed(sandboxConfig, out TypeConfig? parentCfg) == false)
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
                    if (parentCfg.NestedTypes != null && parentCfg.NestedTypes.TryGetValue(Name, out cfg))
                    {
                        return true;
                    }

                    cfg = null;
                    return false;
                }

                if (ResolutionScope is MResScopeAssembly mResScopeAssembly &&
                    sandboxConfig.MultiAssemblyOtherReferences.Contains(mResScopeAssembly.Name))
                {
                    cfg = TypeConfig.DefaultAll;
                    return true;
                }

                // Types without namespaces or nesting parent are not allowed at all.
                cfg = null;
                return false;
            }
        }

        // Check if in whitelisted namespaces.
        foreach (string whNamespace in sandboxConfig.WhitelistedNamespaces)
        {
            if (Namespace?.StartsWith(whNamespace) ?? false)
            {
                cfg = TypeConfig.DefaultAll;
                return true;
            }
        }

        if (ResolutionScope is MResScopeAssembly resScopeAssembly &&
            sandboxConfig.MultiAssemblyOtherReferences.Contains(resScopeAssembly.Name))
        {
            cfg = TypeConfig.DefaultAll;
            return true;
        }


        if (Namespace == null || sandboxConfig.Types.TryGetValue(Namespace, out Dictionary<string, TypeConfig>? nsDict) == false)
        {
            cfg = null;
            return false;
        }

        return nsDict.TryGetValue(Name, out cfg);
    }
}