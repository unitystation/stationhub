namespace UnitystationLauncher.Constants;

public static class ApiUrls
{
    public const string ApiBaseUrl = "https://api.unitystation.org";
    public const string ServerListUrl = $"{ApiBaseUrl}/serverlist";
    public const string ValidateUrl = $"{ApiBaseUrl}/validatehubclient";
    public const string ValidateTokenUrl = $"{ApiBaseUrl}/validatetoken?data=";
    public const string SignOutUrl = $"{ApiBaseUrl}/signout?data=";

    public const string ChangelogBaseUrl = "https://changelog.unitystation.org";
    public const string Latest10VersionsUrl = $"{ChangelogBaseUrl}/all-changes?format=json&limit=10";
}