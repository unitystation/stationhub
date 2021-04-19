using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.IO;
using System.Reactive;
using UnitystationLauncher.Infrastructure;
using Serilog;

namespace UnitystationLauncher.Models
{
    public class InstallationManager : ReactiveObject, IDisposable
    {
        private bool _autoRemove;
        private readonly FileSystemWatcher _fileWatcher;
        private readonly IDisposable _autoRemoveSub;

        public InstallationManager()
        {
            _fileWatcher = new FileSystemWatcher(Config.InstallationsPath)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            var changed = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Changed += h,
                h => _fileWatcher.Changed -= h);

            var created = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Created += h,
                h => _fileWatcher.Created -= h);

            var deleted = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Deleted += h,
                h => _fileWatcher.Deleted -= h);

            var renamed = Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Renamed += h,
                h => _fileWatcher.Renamed -= h);

            Installations = changed
                .Merge(created)
                .Merge(deleted)
                .Merge(renamed)
                .Do(x => Log.Verbose("Filewatcher event: {@Event}", x.EventArgs))
                .Select(e => Unit.Default)
                .ThrottleSubsequent(TimeSpan.FromMilliseconds(200))
                .Merge(Observable.Return(Unit.Default))
                .Select(f =>
                    Directory.EnumerateDirectories(Config.InstallationsPath)
                        .Select(dir => new Installation(dir))
                        .OrderByDescending(x => x.ForkName + x.BuildVersion)
                        .ToList())
                .Do(x => Log.Information("Installations changed"))
                .Replay(1)
                .RefCount();

            _autoRemoveSub = this.WhenAnyValue(x => x.AutoRemove)
                .Merge(Observable.Return(_autoRemove))
                .CombineLatest(Installations, (a, installations) => installations)
                .Subscribe(RemoveOldVersions);
        }

        public IObservable<IReadOnlyList<Installation>> Installations { get; }

        public bool AutoRemove
        {
            get => _autoRemove;
            set => this.RaiseAndSetIfChanged(ref _autoRemove, value);
        }

        private void RemoveOldVersions(IReadOnlyList<Installation> installations)
        {
            if (!_autoRemove)
            {
                return;
            }

            // For each fork delete all installations except the one with the highest version number
            var installationsToDelete = installations
                .GroupBy(installation => installation.ForkName)
                .SelectMany(installationsForFork => installationsForFork
                    .OrderByDescending(installation => installation.BuildVersion)
                    .Skip(1));

            foreach (Installation i in installationsToDelete)
            {
                try
                {
                    i.DeleteInstallation();
                }
                catch (Exception e)
                {
                    Log.Error(e, "An exception occurred during the deletion of an installation");
                }
            }
        }


        public void Dispose()
        {
            _autoRemoveSub.Dispose();
            _fileWatcher.Dispose();
        }
    }
}