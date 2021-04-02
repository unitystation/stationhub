using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace UnitystationLauncher.Models
{
    public class DirectoryManager : IDisposable
    {
        public IObservable<IReadOnlyList<string>> Directories { get; }
        private FileSystemWatcher fw = new FileSystemWatcher(Config.InstallationsPath) { EnableRaisingEvents = true };

        public DirectoryManager()
        {
            if (!Directory.Exists(Config.InstallationsPath))
            {
                Directory.CreateDirectory(Config.InstallationsPath);
            }

            Directories = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => fw.Changed += h,
                h => fw.Changed -= h)
                .Select(e => GetDirectories())
                .PublishLast();
        }

        private static string[] GetDirectories() => Directory.EnumerateDirectories(Config.InstallationsPath).ToArray();

        public void Dispose()
        {
            fw.Dispose();
        }
    }
}
