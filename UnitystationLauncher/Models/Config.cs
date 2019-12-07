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
        public static string winExeNameOld = "StationHubOld.exe";

        public static string unixExeName = "StationHub";
        public static string unixExeNameOld = "StationHubOld";

        public static string WinExeFullPath => Path.Combine(RootFolder, winExeName);
        public static string WinExeOldFullPath => Path.Combine(RootFolder, winExeNameOld);

        public static string UnixExeFullPath => Path.Combine(RootFolder, unixExeName);
        public static string UnixExeOldFullPath => Path.Combine(RootFolder, unixExeNameOld);

        public static int currentBuild = 921;
        public static HubClientConfig serverHubClientConfig;

        public static string InstallationsPath => Path.Combine(Environment.CurrentDirectory, InstallationFolder);
        public static string RootFolder { get; }
        public static FileSystemWatcher FileWatcher { get; }
        public static IObservable<Unit> InstallationChanges { get; }

        static Config()
        {
            Directory.CreateDirectory(InstallationsPath);

            FileWatcher = new FileSystemWatcher(InstallationsPath) { EnableRaisingEvents = true, IncludeSubdirectories = true };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                RootFolder = Environment.CurrentDirectory;
            }
            else
            {
                RootFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            }

            InstallationChanges = Observable.Merge(
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    h => FileWatcher.Changed += h,
                    h => FileWatcher.Changed -= h)
                .Do(o => Log.Debug("File refresh: {FileName}", o.EventArgs.Name))
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
                    startInfo = new ProcessStartInfo("chmod", $"-R 755 {path}");
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