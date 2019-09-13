using Serilog;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace UnitystationLauncher.Models
{
    static class Config
    {
        public static string email;
        public static string InstallationFolder = "Installations";
        public static string apiUrl = "https://api.unitystation.org/serverlist";

        static Config()
        {
            Directory.CreateDirectory(InstallationsPath);

            FileWatcher = new FileSystemWatcher(InstallationsPath) { EnableRaisingEvents = true, IncludeSubdirectories = true };

            InstallationChanges = Observable.Merge(
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    h => FileWatcher.Changed += h,
                    h => FileWatcher.Changed -= h)
                .Select(e => Unit.Default),
                Observable.Return(Unit.Default))
                .ObserveOn(SynchronizationContext.Current)
                .Do(o => Log.Debug("File refresh"));
        }

        public static string InstallationsPath => Path.Combine(Environment.CurrentDirectory, InstallationFolder);

        public static FileSystemWatcher FileWatcher { get; }

        public static IObservable<Unit> InstallationChanges { get; }
    }
}