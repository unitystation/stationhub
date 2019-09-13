using Mono.Unix;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace UnitystationLauncher.Models
{
    class Installation
    {
        private Installation()
        {
            Play = ReactiveCommand.Create(StartImp);
            Open = ReactiveCommand.Create(OpenImp);
            Delete = ReactiveCommand.Create(DeleteImp);
        }

        public Installation(string forkName, int buildVersion) : this()
        {
            ForkName = forkName;
            BuildVersion = buildVersion;
            InstallationPath = Path.Combine(Config.InstallationsPath, ForkName + BuildVersion);
        }

        public Installation(string folderName) : this()
        {
            var match = Regex.Match(folderName, @"(.+?)(\d+)");
            ForkName = match.Groups[1].Value;
            BuildVersion = int.Parse(match.Groups[2].Value);
            InstallationPath = Path.Combine(Config.InstallationsPath, folderName);
        }

        public string ForkName { get; }
        public int BuildVersion { get; }
        public string InstallationPath { get; }

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
                exe = files.SingleOrDefault(s => (new UnixFileInfo(s).FileAccessPermissions & FileAccessPermissions.UserExecute) > 0);
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
                    throw;
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
                Log.Error(e, "An exception occurred during the opening on an installation");
                throw;
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
                Log.Error(e, "An exception occurred during the deletion on an installation");
                throw;
            }
        }
    }
}
