namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

// Normal single dimensional array with zero lower bound.
internal sealed record MTypeSZArray(MType ElementType) : MType
{
    public override string ToString()
    {
        return $"{ElementType}[]";
    }

    public override string WhitelistToString()
    {
        return $"{ElementType.WhitelistToString()}[]";
    }

    public override bool WhitelistEquals(MType other)
    {
        return other is MTypeSZArray arr && ElementType.WhitelistEquals(arr.ElementType);
    }
}