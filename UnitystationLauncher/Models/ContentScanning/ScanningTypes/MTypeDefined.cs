namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed record MTypeDefined(string Name, string? Namespace, MTypeDefined? Enclosing) : MType
{
    public override string ToString()
    {
        string name = Namespace != null ? $"{Namespace}.{Name}" : Name;

        if (Enclosing != null)
        {
            return $"{Enclosing}/{name}";
        }

        return name;
    }

    public override bool IsCoreTypeDefined()
    {
        return true;
    }
}