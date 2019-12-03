using Avalonia;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;
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
using System.Threading.Tasks;

namespace UnitystationLauncher.Models
{
    public class Installation
    {
        private Installation()
        {
            Play = ReactiveCommand.Create(StartImp);
            Open = ReactiveCommand.Create(OpenImp);
            Delete = ReactiveCommand.Create(DeleteImp);
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
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var archives = Directory.EnumerateDirectories(path);
                exe = archives.SingleOrDefault(s => Regex.IsMatch(Path.GetFileName(s), @".*station\.app"));
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
                    ProcessStartInfo startInfo;

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        startInfo = new ProcessStartInfo("open", $"-a {exe}");
                    }
                    else
                    {
                        startInfo = new ProcessStartInfo(exe);
                    }
                    startInfo.UseShellExecute = true;
                    var process = new Process();
                    process.StartInfo = startInfo;

                    process.Start();

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

        private async void DeleteImp()
        {
            try
            {
                var msgBox = MessageBoxWindow.CreateCustomWindow(new MessageBoxCustomParams
                {
                    Style = MessageBox.Avalonia.Enums.Style.None,
                    Icon = MessageBox.Avalonia.Enums.Icon.None,
                    ContentHeader = $"Remove {ForkName}-{BuildVersion}",
                    ContentMessage = "This action cannot be undone. Proceed?",
                    ButtonDefinitions = new[] { new ButtonDefinition { Name = "Cancel" }, new ButtonDefinition { Name = "Confirm" } }
                });

                var response = await msgBox.Show();
                if (response.Equals("Confirm"))
                {
                    Log.Information($"Perform delete of {InstallationPath}");
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        ProcessStartInfo startInfo;
                        startInfo = new ProcessStartInfo("rm", $"-r {InstallationPath}");
                        startInfo.UseShellExecute = false;
                        var process = new Process();
                        process.StartInfo = startInfo;

                        process.Start();
                    }
                    else
                    {
                        Directory.Delete(InstallationPath, true);
                    }
                }
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
