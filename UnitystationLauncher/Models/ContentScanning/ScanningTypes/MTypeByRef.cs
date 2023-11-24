namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed record MTypeByRef(MType ElementType) : MType
{
    public override string ToString()
    {
        return $"{ElementType}&";
    }

    public override string WhitelistToString()
    {
        return $"ref {ElementType.WhitelistToString()}";
    }

    public override bool WhitelistEquals(MType other)
    {
        return other is MTypeByRef byRef && ElementType.WhitelistEquals(byRef.ElementType);
    }
}