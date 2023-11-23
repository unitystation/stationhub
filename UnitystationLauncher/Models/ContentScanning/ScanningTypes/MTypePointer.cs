namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed record MTypePointer(MType ElementType) : MType
{
    public override string ToString()
    {
        return $"{ElementType}*";
    }

    public override string WhitelistToString()
    {
        return $"{ElementType.WhitelistToString()}*";
    }

    public override bool WhitelistEquals(MType other)
    {
        return other is MTypePointer ptr && ElementType.WhitelistEquals(ptr.ElementType);
    }
}