namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed record MTypeGenericMethodPlaceHolder(int Index) : MType
{
    public override string ToString()
    {
        return $"!!{Index}";
    }

    public override bool WhitelistEquals(MType other)
    {
        return Equals(other);
    }
}