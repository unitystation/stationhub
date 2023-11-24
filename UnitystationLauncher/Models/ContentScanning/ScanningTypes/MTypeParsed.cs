namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed record MTypeParsed(string FullName, MTypeParsed? NestedParent = null) : MType
{
    public override string ToString()
    {
        return NestedParent != null ? $"{NestedParent}/{FullName}" : FullName;
    }

    public override bool WhitelistEquals(MType other)
    {
        switch (other)
        {
            case MTypeParsed parsed:
                if (NestedParent != null
                    && (parsed.NestedParent == null || NestedParent.WhitelistEquals(parsed.NestedParent) == false))
                {
                    return false;
                }

                return parsed.FullName == FullName;
            case MTypeReferenced referenced:
                if (NestedParent != null
                    && (referenced.ResolutionScope is not MResScopeType parentRes ||
                        NestedParent.WhitelistEquals(parentRes.Type) == false))
                {
                    return false;
                }

                string refFullName = referenced.Namespace == null
                    ? referenced.Name
                    : $"{referenced.Namespace}.{referenced.Name}";
                return FullName == refFullName;
            default:
                return false;
        }
    }
}