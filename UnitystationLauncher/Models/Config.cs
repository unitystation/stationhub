using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnitystationLauncher.Models
{
    public class Config
    {
        //Whenever you change the currentBuild here, please also update the one in UnitystationLauncher/Assets/StationHub.metainfo.xml for Linux software stores. Thank you.
        public const int CurrentBuild = 927;

        //file names
        private const string WinExeName = "StationHub.exe";
        private const string UnixExeName = "StationHub";
        private const string InstallationFolder = "Installations";
        public const string ApiUrl = "https://api.unitystation.org/serverlist";
        public const string ValidateUrl = "https://api.unitystation.org/validatehubclient";
        public const string SiteUrl = "https://unitystation.org/";
        public const string SupportUrl = "https://www.patreon.com/unitystation";
        public const string ReportUrl = "https://github.com/unitystation/unitystation/issues";

        private readonly HttpClient _http;
        public Config(HttpClient http)
        {
            _http = http;
        }

        public static string InstallationsPath => Path.Combine(RootFolder, InstallationFolder);
        public static string TempFolder => Path.Combine(RootFolder, "temp");
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

        private HubClientConfig? _clientConfig;
        public async Task<HubClientConfig> GetServerHubClientConfig()
        {
            if (_clientConfig == null)
            {
                
                var data = await _http.GetStringAsync(ValidateUrl);
                _clientConfig = JsonConvert.DeserializeObject<HubClientConfig>(data);
            }

            return _clientConfig;
        }
    }
}