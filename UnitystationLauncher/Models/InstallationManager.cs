using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.IO;
using System.Reactive.Subjects;
using UnitystationLauncher.Infrastructure;

namespace UnitystationLauncher.Models
{

    public class InstallationManager : ReactiveObject
    {
        private readonly BehaviorSubject<IReadOnlyList<Installation>> installationsSubject;

        public InstallationManager()
        {
            installationsSubject = new BehaviorSubject<IReadOnlyList<Installation>>(new Installation[0]);

            Config.InstallationChanges
                .ThrottleSubsequent(TimeSpan.FromMilliseconds(200))
                .Select(f =>
                    Directory.EnumerateDirectories(Config.InstallationsPath)
                        .Select(dir => new Installation(dir)).ToList())
                .Subscribe(installationsSubject);
        }

        public IObservable<IReadOnlyList<Installation>> Installations => installationsSubject;
    }
}