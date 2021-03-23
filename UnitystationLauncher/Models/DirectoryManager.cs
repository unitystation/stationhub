using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace UnitystationLauncher.Models
{
    public class DirectoryManager
    {
        public IObservable<IReadOnlyList<string>> Directories { get; }

        public DirectoryManager()
        {
            if (!Directory.Exists(Config.InstallationsPath))
            {
                Directory.CreateDirectory(Config.InstallationsPath);
            }

            var fw = new FileSystemWatcher(Config.InstallationsPath) { EnableRaisingEvents = true };
            var bs = new BehaviorSubject<IReadOnlyList<string>>(GetDirectories());

            Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => fw.Changed += h,
                h => fw.Changed -= h)
                .Select(e => GetDirectories())
                .Subscribe(bs);

            Directories = bs;
        }

        private static string[] GetDirectories() => Directory.EnumerateDirectories(Config.InstallationsPath).ToArray();
    }
}
