namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;


internal sealed record MResScopeType(MType Type) : MResScope
{
    public override string ToString()
    {
        return $"{Type}/";
    }
}