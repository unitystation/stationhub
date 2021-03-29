using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.IO;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using UnitystationLauncher.Infrastructure;
using Serilog;

namespace UnitystationLauncher.Models
{
    public class InstallationManager : ReactiveObject
    {
        private readonly BehaviorSubject<IReadOnlyList<Installation>> _installationsSubject;
        private bool _autoRemove;
        public bool AutoRemove { get => _autoRemove; set { _autoRemove = value; if (_autoRemove) TryAutoRemove(); } }
        public Action? InstallListChange;

        public InstallationManager()
        {
            _installationsSubject = new BehaviorSubject<IReadOnlyList<Installation>>(new Installation[0]);
            var fileWatcher = new FileSystemWatcher(Config.InstallationsPath)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    h => fileWatcher.Changed += h,
                    h => fileWatcher.Changed -= h)
                .Select(e => Unit.Default)
                .Merge(Observable.Return(Unit.Default))
                .ObserveOn(SynchronizationContext.Current!)
                .ThrottleSubsequent(TimeSpan.FromMilliseconds(1000))
                .Select(f =>
                    Directory.EnumerateDirectories(Config.InstallationsPath)
                        .Select(dir => new Installation(dir))
                        .OrderByDescending(x => x.ForkName + x.BuildVersion)
                        .ToList())
                .Subscribe(_installationsSubject);

            _installationsSubject.Subscribe(ListHasChanged);
        }

        void ListHasChanged(IReadOnlyList<Installation> list)
        {
            InstallListChange?.Invoke();
        }

        public void TryAutoRemove()
        {
            if (!_autoRemove) return;

            var key = "";
            foreach (Installation i in _installationsSubject.Value)
            {
                if (!key.Equals(i.ForkName))
                {
                    key = i.ForkName;
                    continue;
                }
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

        public IObservable<IReadOnlyList<Installation>> Installations => _installationsSubject;
    }
}