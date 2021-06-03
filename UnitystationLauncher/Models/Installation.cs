using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;
using Mono.Unix;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace UnitystationLauncher.Models
{
    public class Installation
    {
        public Installation(string folderPath)
        {
            ForkName = GetForkName(folderPath);
            BuildVersion = GetBuildVersion(folderPath);
            InstallationPath = folderPath;
        }

        public string ForkName { get; }
        public int BuildVersion { get; }
        public string InstallationPath { get; }
        public (string, int) ForkAndVersion => (ForkName, BuildVersion);

        public static string? FindExecutable(string path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }

            var files = Directory.EnumerateFiles(path);
            string? exe;

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
                    s.EndsWith("station"));
            }

            return exe;
        }

        public void Start(IPAddress ip, short port, string? refreshToken, string? uid)
        {
            Start($"--server {ip} --port {port} --refreshtoken {refreshToken} --uid {uid}");
        }

        public void Start()
        {
            Start("");
        }

        private void Start(string arguments)
        {
            var exe = FindExecutable(InstallationPath);
            if (exe == null)
            {
                Log.Information("Couldn't find executable to start");
                return;
            }

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.MainWindow.WindowState = Avalonia.Controls.WindowState.Minimized;
            }

            ProcessStartInfo startInfo;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                startInfo = new ProcessStartInfo("/bin/bash", $"-c \" open -a '{exe}' --args {arguments}; \"");
                Log.Information("Start OSX");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo = new ProcessStartInfo("/bin/bash", $"-c \" '{exe}' --args {arguments}; \"");
                Log.Information("Start Linux");
            }
            else
            {
                startInfo = new ProcessStartInfo(exe, $"{arguments}");
                Log.Information("Start Windows");
            }

            startInfo.UseShellExecute = false;
            var process = new Process { StartInfo = startInfo };

            process.Start();
        }

        public async Task Delete()
        {
            try
            {
                var msgBox = MessageBoxManager.GetMessageBoxCustomWindow(new MessageBoxCustomParams
                {
                    Style = MessageBox.Avalonia.Enums.Style.None,
                    Icon = MessageBox.Avalonia.Enums.Icon.None,
                    ShowInCenter = true,
                    ContentHeader = $"Remove {ForkName}-{BuildVersion}",
                    ContentMessage = "This action cannot be undone. Proceed?",
                    ButtonDefinitions = new[]
                        {new ButtonDefinition {Name = "Cancel"}, new ButtonDefinition {Name = "Confirm"}}
                });

                var response = await msgBox.Show();
                if (response.Equals("Confirm"))
                {
                    DeleteInstallation();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "An exception occurred during the deletion of an installation");
            }
        }

        public void DeleteInstallation()
        {
            Log.Information("Perform delete of {InstallationPath}", InstallationPath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ProcessStartInfo startInfo;
                startInfo = new ProcessStartInfo("/bin/bash", $"-c \" rm -r '{InstallationPath}'; \"");
                startInfo.UseShellExecute = false;
                var process = new Process();
                process.StartInfo = startInfo;

                process.Start();
            }
            else
            {
                DeleteFolder(InstallationPath);
            }
        }

        private void DeleteFolder(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteFolder(dir);
            }

            Directory.Delete(targetDir, false);
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

        public static void MakeExecutableExecutable(string installationPath)
        {
            var exe = FindExecutable(installationPath);

            var fileInfo = new UnixFileInfo(exe);
            fileInfo.FileAccessPermissions |= FileAccessPermissions.UserReadWriteExecute;
        }
    }
}