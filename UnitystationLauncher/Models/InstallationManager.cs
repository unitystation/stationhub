using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.IO;
using System.Reactive.Subjects;
using UnitystationLauncher.Infrastructure;
using Serilog;

namespace UnitystationLauncher.Models
{
    public class InstallationManager : ReactiveObject
    {
        private readonly BehaviorSubject<IReadOnlyList<Installation>> installationsSubject;
        private bool autoRemove;
        public bool AutoRemove { get => autoRemove; set { autoRemove = value; if (autoRemove) TryAutoRemove(); } }
        public Action InstallListChange;
        public InstallationManager()
        {
            installationsSubject = new BehaviorSubject<IReadOnlyList<Installation>>(new Installation[0]);
            Config.InstallationChanges
                .ThrottleSubsequent(TimeSpan.FromMilliseconds(1000))
                .Select(f =>
                    Directory.EnumerateDirectories(Config.InstallationsPath)
                        .Select(dir => new Installation(dir))
                        .OrderByDescending(x => x.ForkName + x.BuildVersion)
                        .ToList())
                .Subscribe(installationsSubject);

            installationsSubject.Subscribe(ListHasChanged);
        }

        void ListHasChanged(IReadOnlyList<Installation> list)
        {
            InstallListChange?.Invoke();
        }

        public void TryAutoRemove()
        {
            if (!autoRemove) return;

            var key = "";
            foreach(Installation i in installationsSubject.Value)
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

        public IObservable<IReadOnlyList<Installation>> Installations => installationsSubject;
    }
}