using Mono.Unix;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace UnitystationLauncher.Models
{
    public class Installation
    {
        private Installation()
        {
            Play = ReactiveCommand.Create(StartImp, Config.InstallationChanges
                .Select(u => FindExecutable(InstallationPath) != null));
            Open = ReactiveCommand.Create(OpenImp);
            Delete = ReactiveCommand.Create(DeleteImp);
        }

        public Installation(string forkName, int buildVersion) : this()
        {
            ForkName = forkName;
            BuildVersion = buildVersion;
            InstallationPath = Path.Combine(Config.InstallationsPath, ForkName + BuildVersion);
        }

        public Installation(string folderPath) : this()
        {
            ForkName = GetForkName(folderPath);
            BuildVersion = GetBuildVersion(folderPath);
            InstallationPath = folderPath;
        }

        public string ForkName { get; }
        public int BuildVersion { get; }
        public string InstallationPath { get; }
        public (string, int) Key => (ForkName, BuildVersion);


        public ReactiveCommand<Unit, Unit> Play { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> Delete { get; }


        public static string? FindExecutable(string path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            var files = Directory.EnumerateFiles(path);
            string? exe = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                exe = files.SingleOrDefault(s => Regex.IsMatch(Path.GetFileName(s), @".*station\.exe"));
            }
            else
            {
                exe = files.SingleOrDefault(s => 
                    s.EndsWith("station") &&
                    (new UnixFileInfo(s).FileAccessPermissions & FileAccessPermissions.UserExecute) > 0);
            }

            return exe;
        }

        public void StartImp()
        {
            var exe = FindExecutable(InstallationPath);
            if (exe != null)
            {
                try
                {
                    Process.Start(exe);
                }
                catch (Exception e)
                {
                    Log.Error(e, "An exception occurred during the start of an installation");
                }
            }
        }

        private void OpenImp()
        {
            try
            {
                Process.Start(InstallationPath);
            }
            catch (Exception e)
            {
                Log.Error(e, "An exception occurred during the opening of an installation");
            }
        }

        private void DeleteImp()
        {
            try
            {
                Directory.Delete(InstallationPath, true);
            }
            catch (Exception e)
            {
                Log.Error(e, "An exception occurred during the deletion of an installation");
            }
        }

        public static string GetForkName(string s)
        {
            var folderName = s.Replace(Config.InstallationsPath, "").Trim(Path.DirectorySeparatorChar);
            var match = Regex.Match(folderName, @"(.+?)(\d+)");
            return match.Groups[1].Value;
        }

        public static int GetBuildVersion(string s)
        {
            var folderName = s.Replace(Config.InstallationsPath, "").Trim(Path.DirectorySeparatorChar);
            var match = Regex.Match(folderName, @"(.+?)(\d+)");
            return int.Parse(match.Groups[2].Value);
        }
    }
}
