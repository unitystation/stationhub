namespace UnitystationLauncher.Constants;

public static class ApiUrls
{
    private static string ApiBaseUrl => "https://api.unitystation.org";
    public static string ServerListUrl => $"{ApiBaseUrl}/serverlist";
    public static string ValidateUrl => $"{ApiBaseUrl}/validatehubclient";
    public static string ValidateTokenUrl => $"{ApiBaseUrl}/validatetoken?data=";
    public static string SignOutUrl => $"{ApiBaseUrl}/signout?data=";

    private static string ChangelogBaseUrl => "https://changelog.unitystation.org";
    public static string Latest10VersionsUrl => $"{ChangelogBaseUrl}/all-changes?format=json&limit=10";
    public static string LatestBlogPosts => $"{ChangelogBaseUrl}/posts/?format=json";

    private static string CdnBaseUrl => "https://unitystationfile.b-cdn.net";
    public static string GoodFilesBaseUrl => $"{CdnBaseUrl}/GoodFiles";
    public static string AllowedGoodFilesUrl => $"{GoodFilesBaseUrl}/AllowGoodFiles.json";

    private static string RawGitHubFileBaseUrl => "https://raw.githubusercontent.com/unitystation/unitystation/develop";
    public static string CodeScanListUrl => $"{RawGitHubFileBaseUrl}/CodeScanList.json";
}