using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;
using Mono.Unix;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Models
{
    public class Installation
    {
        public string ForkName { get; }
        public int BuildVersion { get; }
        public string InstallationPath { get; }
        public (string, int) ForkAndVersion => (ForkName, BuildVersion);

        private IEnvironmentService _environmentService;

        public Installation(string folderPath, IPreferencesService preferencesService, IEnvironmentService environmentService)
        {
            ForkName = GetForkName(folderPath, preferencesService);
            BuildVersion = GetBuildVersion(folderPath, preferencesService);
            InstallationPath = folderPath;
            _environmentService = environmentService;
        }

        public static string? FindExecutable(string path, IEnvironmentService environmentService)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }

            return environmentService.GetCurrentEnvironment() switch
            {
                CurrentEnvironment.WindowsStandalone
                    => Path.Combine(path, "Unitystation.exe"),
                CurrentEnvironment.MacOsStandalone
                    => Path.Combine(path, "Unitystation.app", "Contents", "MacOS", "unitystation"),
                CurrentEnvironment.LinuxStandalone or CurrentEnvironment.LinuxFlatpak
                    => Path.Combine(path, "Unitystation"),
                _ => null
            };
        }

        public void LaunchWithArgs(string ip, short port)
        {
            Start($"--server {ip} --port {port}");
        }

        public void LaunchWithoutArgs()
        {
            Start("");
        }

        // TODO: Move to installation service
        private void Start(string arguments)
        {
            var exe = FindExecutable(InstallationPath, _environmentService);
            if (exe == null)
            {
                Log.Information("Couldn't find executable to start");
                return;
            }

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
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

        // TODO: Move to installation service
        public async Task DeleteAsync()
        {
            try
            {
                var msgBox = MessageBoxManager.GetMessageBoxCustomWindow(new MessageBoxCustomParams
                {
                    SystemDecorations = Avalonia.Controls.SystemDecorations.BorderOnly,
                    WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen,
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

        // TODO: Move to installation service
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

        // TODO: Move to installation service
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

        public static string GetForkName(string path, IPreferencesService preferencesService)
        {
            Preferences preferences = preferencesService.GetPreferences();
            string folderName = path.Replace(preferences.InstallationPath, "").Trim(Path.DirectorySeparatorChar);
            Match match = Regex.Match(folderName, @"(.+?)(\d+)");
            return match.Groups[1].Value;
        }

        public static int GetBuildVersion(string path, IPreferencesService preferencesService)
        {
            Preferences preferences = preferencesService.GetPreferences();
            string folderName = path.Replace(preferences.InstallationPath, "").Trim(Path.DirectorySeparatorChar);
            Match match = Regex.Match(folderName, @"(.+?)(\d+)");
            return int.Parse(match.Groups[2].Value);
        }

        // TODO: Move to installation service
        public static void MakeExecutableExecutable(string installationPath, IEnvironmentService environmentService)
        {
            string? exe = FindExecutable(installationPath, environmentService);

            UnixFileInfo fileInfo = new(exe);
            fileInfo.FileAccessPermissions |= FileAccessPermissions.UserReadWriteExecute;
        }
    }
}