using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Mono.Unix;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services
{
    public class InstallationService : ReactiveObject, IInstallationService
    {
        private readonly FileSystemWatcher _fileWatcher;
        private readonly IDisposable _autoRemoveSub;
        private readonly IEnvironmentService _environmentService;
        private readonly IPreferencesService _preferencesService;
        private readonly IObservable<IReadOnlyList<Installation>> _installations;

        public InstallationService(IPreferencesService preferencesService, IEnvironmentService environmentService)
        {
            _preferencesService = preferencesService;
            _environmentService = environmentService;

            Preferences preferences = _preferencesService.GetPreferences();
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

            _installations = changed
                .Merge(created)
                .Merge(deleted)
                .Merge(renamed)
                .Do(x => Log.Verbose("Filewatcher event: {@Event}", x.EventArgs))
                .Select(e => Unit.Default)
                .ThrottleSubsequent(TimeSpan.FromMilliseconds(200))
                .Merge(Observable.Return(Unit.Default))
                .Select(f =>
                    Directory.EnumerateDirectories(preferences.InstallationPath)
                        .Select(dir => new Installation(dir, preferencesService, _environmentService))
                        .OrderByDescending(x => x.ForkName + x.BuildVersion)
                        .ToList())
                .Do(x => Log.Information("Installations changed"))
                .Replay(1)
                .RefCount();

            _autoRemoveSub = preferences.WhenAnyValue(x => x.AutoRemove)
                .CombineLatest(_installations, (a, installations) => installations)
                .Subscribe(RemoveOldVersions);
        }

        public IObservable<IReadOnlyList<Installation>> GetInstallations()
        {
            return _installations;
        }

        private void RemoveOldVersions(IReadOnlyList<Installation> installations)
        {
            if (!_preferencesService.GetPreferences().AutoRemove)
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

            if (_environmentService.GetCurrentEnvironment() == CurrentEnvironment.WindowsStandalone)
            {
                return;
            }

            try
            {
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