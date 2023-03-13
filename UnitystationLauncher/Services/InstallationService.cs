using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Mono.Unix;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services
{
    public class InstallationService : ReactiveObject, IDisposable
    {
        private bool _autoRemove;
        private readonly FileSystemWatcher _fileWatcher;
        private readonly IDisposable _autoRemoveSub;
        private readonly IEnvironmentService _environmentService;

        public InstallationService(IPreferencesService preferencesService, IEnvironmentService environmentService)
        {
            Preferences preferences = preferencesService.GetPreferences();
            _environmentService = environmentService;
            SetupInstallationPath(preferences.InstallationPath);
            _fileWatcher = new(preferences.InstallationPath)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            IObservable<EventPattern<FileSystemEventArgs>> changed = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Changed += h,
                h => _fileWatcher.Changed -= h);

            IObservable<EventPattern<FileSystemEventArgs>> created = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Created += h,
                h => _fileWatcher.Created -= h);

            IObservable<EventPattern<FileSystemEventArgs>> deleted = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fileWatcher.Deleted += h,
                h => _fileWatcher.Deleted -= h);

            IObservable<EventPattern<FileSystemEventArgs>> renamed = Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(
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
                    Directory.EnumerateDirectories(preferences.InstallationPath)
                        .Select(dir => new Installation(dir, preferencesService))
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
            IEnumerable<Installation> installationsToDelete = installations
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

        private void SetupInstallationPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                if (_environmentService.IsRunningOnWindows())
                {
                    return;
                }

                UnixFileInfo fileInfo = new(path);
                fileInfo.FileAccessPermissions |= FileAccessPermissions.UserReadWriteExecute;
            }
            catch (Exception e)
            {
                Log.Error("Error: {Error}", $"There was an issue setting up permissions for the installation directory: {e.Message}");
            }
        }

        public void Dispose()
        {
            _autoRemoveSub.Dispose();
            _fileWatcher.Dispose();
        }
    }
}