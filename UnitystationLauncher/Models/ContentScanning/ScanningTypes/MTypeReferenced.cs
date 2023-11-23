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
}