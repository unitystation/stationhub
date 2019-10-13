using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System.Text.Json;
using System.Reactive;
using System.Reactive.Linq;
using System.IO;

namespace UnitystationLauncher.Models
{

    public class InstallationManager : ReactiveObject, IDisposable
    {
        private readonly ObservableCollection<Installation> installations;

        private FileSystemWatcher FileWatcher { get; }

        private IDisposable InstallationChanges { get; }

        public InstallationManager()
        {
            installations = new ObservableCollection<Installation>();
            Installations = new ReadOnlyObservableCollection<Installation>(installations);

            FileWatcher = new FileSystemWatcher(Config.InstallationsPath) { EnableRaisingEvents = true, IncludeSubdirectories = true };
            InstallationChanges = Observable.Merge(
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    h => FileWatcher.Changed += h,
                    h => FileWatcher.Changed -= h)
                    .Select(e => Unit.Default),
                Observable.Return(Unit.Default))
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Subscribe(f =>
                {
                    installations.Clear();
                    foreach (var dir in Directory.EnumerateDirectories(Config.InstallationsPath))
                    {
                        installations.Add(new Installation(dir));
                    }
                });

        }

        public ReadOnlyObservableCollection<Installation> Installations { get; }

        public void Dispose()
        {
            InstallationChanges.Dispose();
            ((IDisposable)FileWatcher).Dispose();
        }
    }
}