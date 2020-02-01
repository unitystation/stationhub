using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace UnitystationLauncher.Models
{
    static class Config
    {
        public static string email;
        public static string InstallationFolder = "Installations";
        public static string apiUrl = "https://api.unitystation.org/serverlist";
        public static string validateUrl = "https://api.unitystation.org/validatehubclient";

        //file names
        public static string winExeName = "StationHub.exe";
        public static string unixExeName = "StationHub";

        public static string WinExeFullPath => Path.Combine(RootFolder, winExeName);
        public static string WinExeTempPath => Path.Combine(TempFolder, winExeName);

        public static string UnixExeFullPath => Path.Combine(RootFolder, unixExeName);
        public static string UnixExeTempPath => Path.Combine(TempFolder, unixExeName);

        public static int currentBuild = 923;
        public static HubClientConfig serverHubClientConfig;

        public static string InstallationsPath => Path.Combine(RootFolder, InstallationFolder);
        public static string RootFolder { get; }
        public static string TempFolder => Path.Combine(RootFolder, "temp");
        public static FileSystemWatcher FileWatcher { get; }
        public static IObservable<Unit> InstallationChanges { get; }

        static Config()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                RootFolder = Environment.CurrentDirectory;
            }
            else
            {
                RootFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            }

            Directory.CreateDirectory(InstallationsPath);
            SetPermissions(InstallationsPath);
            FileWatcher = new FileSystemWatcher(InstallationsPath)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            InstallationChanges = Observable.Merge(
            Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => FileWatcher.Changed += h,
                h => FileWatcher.Changed -= h)
            .Select(e => Unit.Default),
            Observable.Return(Unit.Default))
            .ObserveOn(SynchronizationContext.Current);
        }

        public static void SetPermissions(string path)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    ProcessStartInfo startInfo;
                    startInfo = new ProcessStartInfo("/bin/bash", $"-c \" chmod -R 755 {path}; \"");
                    var process = new Process();
                    process.StartInfo = startInfo;

                    process.Start();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "An exception occurred when setting the permissions");
            }
        }

        public static string GetHubExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return UnixExeFullPath;
            }

            return WinExeFullPath;
        }

    }

    [Serializable]
    public class HubClientConfig
    {
        public int buildNumber;
        public string winURL;
        public string osxURL;
        public string linuxURL;
        public string dailyMessage;

        public string GetDownloadURL()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return winURL;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return osxURL;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return linuxURL;
            }
            return "";
        }
    }
}