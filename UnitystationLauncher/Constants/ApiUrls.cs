namespace UnitystationLauncher.Constants;

public static class ApiUrls
{
    private const string ApiBaseUrl = "https://api.unitystation.org";
    public const string ServerListUrl = $"{ApiBaseUrl}/serverlist";
    public const string ValidateUrl = $"{ApiBaseUrl}/validatehubclient";
    public const string ValidateTokenUrl = $"{ApiBaseUrl}/validatetoken?data=";
    public const string SignOutUrl = $"{ApiBaseUrl}/signout?data=";

    private const string ChangelogBaseUrl = "https://changelog.unitystation.org";
    public const string Latest10VersionsUrl = $"{ChangelogBaseUrl}/all-changes?format=json&limit=10";
    public const string LatestBlogPosts = $"{ChangelogBaseUrl}/posts/?format=json";

    private const string CdnBaseUrl = "https://unitystationfile.b-cdn.net";
    public const string GoodFilesBaseUrl = $"{CdnBaseUrl}/GoodFiles";
    public const string AllowedGoodFilesUrl = $"{GoodFilesBaseUrl}/AllowGoodFiles.json";

    private const string RawGitHubFileBaseUrl = "https://raw.githubusercontent.com/unitystation/unitystation/develop";
    public const string CodeScanListUrl = $"{RawGitHubFileBaseUrl}/CodeScanList.json";
}