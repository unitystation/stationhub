using Serilog;
using System;
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
        public static int currentBuild = 901;
        public static HubClientConfig serverHubClientConfig;

        static Config()
        {
            Directory.CreateDirectory(InstallationsPath);

            FileWatcher = new FileSystemWatcher(InstallationsPath) { EnableRaisingEvents = true, IncludeSubdirectories = true };
            RootFolder = Environment.CurrentDirectory;
            InstallationChanges = Observable.Merge(
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    h => FileWatcher.Changed += h,
                    h => FileWatcher.Changed -= h)
                .Do(o => Log.Debug("File refresh: {FileName}", o.EventArgs.Name))
                .Select(e => Unit.Default),
                Observable.Return(Unit.Default))
                .ObserveOn(SynchronizationContext.Current);
        }

        public static string InstallationsPath => Path.Combine(Environment.CurrentDirectory, InstallationFolder);
        public static string RootFolder { get; }
        public static FileSystemWatcher FileWatcher { get; }

        public static IObservable<Unit> InstallationChanges { get; }
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