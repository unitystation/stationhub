namespace UnitystationLauncher.Infrastructure;

public static class StringExtensions
{
    public static string SanitiseStringPath(this string input)
    {
        return input.Replace(@"\", "").Replace("/", "").Replace(".", "_");
    }
}