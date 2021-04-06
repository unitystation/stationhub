using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace UnitystationLauncher.Models
{
    public class DirectoryManager : IDisposable
    {
        private readonly FileSystemWatcher _fw = new FileSystemWatcher(Config.InstallationsPath) { EnableRaisingEvents = true };

        public DirectoryManager()
        {
            if (!Directory.Exists(Config.InstallationsPath))
            {
                Directory.CreateDirectory(Config.InstallationsPath);
            }

            Directories = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => _fw.Changed += h,
                h => _fw.Changed -= h)
                .Select(e => GetDirectories())
                .PublishLast();
        }

        public IObservable<IReadOnlyList<string>> Directories { get; }

        private static string[] GetDirectories() => Directory.EnumerateDirectories(Config.InstallationsPath).ToArray();

        public void Dispose()
        {
            _fw.Dispose();
        }
    }
}
