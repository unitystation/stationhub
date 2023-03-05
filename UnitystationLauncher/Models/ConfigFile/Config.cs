using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnitystationLauncher.Constants;

namespace UnitystationLauncher.Models.ConfigFile
{
    public class Config : IDisposable
    {
        //Whenever you change the currentBuild here, please also update the one in UnitystationLauncher/Assets/org.unitystation.StationHub.metainfo.xml for Linux software stores. Thank you.
        public const int CurrentBuild = 930;

        //file names
        private const string WinExeName = "StationHub.exe";
        private const string UnixExeName = "StationHub";
        private const string InstallationFolder = "Installations";

        // Other URLs
        public const string MainSiteUrl = "https://unitystation.org";
        public const string PatreonUrl = "https://www.patreon.com/unitystation";
        public const string GameIssuesUrl = "https://github.com/unitystation/unitystation/issues";
        public const string LauncherIssuesUrl = "https://github.com/unitystation/stationhub/issues";
        public const string DiscordInviteUrl = "https://discord.com/invite/tFcTpBp";

        private HubClientConfig? _hubClientConfig;
        private readonly HttpClient _http;
        private Preferences? _preferences;
        private IDisposable? _preferenceSub;

        public Config(HttpClient http)
        {
            _http = http;
        }

        public static string InstallationsPath => Path.Combine(RootFolder, InstallationFolder);
        public static string TempFolder => Path.Combine(RootFolder, "temp");
        public static string PreferencesFilePath => Path.Combine(RootFolder, "prefs.json");
        public static string WinExeFullPath => Path.Combine(RootFolder, WinExeName);
        public static string UnixExeFullPath => Path.Combine(RootFolder, UnixExeName);

        public static string RootFolder
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Environment.CurrentDirectory;
                }
                //If ran with the FLATPAK compiler symbol, will put mutable files where the Flatpak standard says
                //else, will put in the modern standard Linux folder (which is still legal on MacOS)
#if FLATPAK
            	return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.var/app/org.unitystation.StationHub";
#else
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.local/share/StationHub";
#endif
            }
        }

        public async Task<HubClientConfig?> GetServerHubClientConfigAsync()
        {
            if (_hubClientConfig == null)
            {
                string data = await _http.GetStringAsync(ApiUrls.ValidateUrl);
                _hubClientConfig = JsonSerializer.Deserialize<HubClientConfig>(data);
            }

            return _hubClientConfig;
        }

        public async Task<Preferences> GetPreferencesAsync()
        {
            if (_preferences != null)
            {
                return _preferences;
            }

            if (File.Exists(PreferencesFilePath))
            {
                await using FileStream data = File.OpenRead(PreferencesFilePath);
                _preferences = await JsonSerializer.DeserializeAsync<Preferences>(data);
            }

            _preferences ??= new Preferences();
            _preferenceSub?.Dispose();
            _preferenceSub = _preferences.Changed
                .Select(_ => Observable.FromAsync(SerializePreferencesAsync))
                .Concat()
                .Subscribe();

            return _preferences;
        }

        private async Task SerializePreferencesAsync()
        {
            await using FileStream file = File.Create(PreferencesFilePath);
            await JsonSerializer.SerializeAsync(file, _preferences, new JsonSerializerOptions
            { IgnoreReadOnlyProperties = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        }

        public void Dispose()
        {
            _preferenceSub?.Dispose();
            _http.Dispose();
        }
    }
}