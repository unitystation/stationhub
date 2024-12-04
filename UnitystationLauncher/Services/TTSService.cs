using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using Mono.Unix;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Exceptions;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;
using Path = System.IO.Path;

namespace UnitystationLauncher.Services;

public class TTSService : ITTSService
{
    private static string _nameConfig = @"CodeScanList.json";

    private readonly HttpClient _httpClient;

    private readonly IPreferencesService _preferencesService;
    private readonly IEnvironmentService _environmentService;

    private static Process? process;

    public TTSService(HttpClient httpClient, IPreferencesService preferencesService,
        IEnvironmentService environmentService)
    {
        _httpClient = httpClient;
        _preferencesService = preferencesService;
        _environmentService = environmentService;
    }

    public async Task CheckAndDownloadLatestVersion(Download Download)
    {
        if ((_preferencesService.GetPreferences().TTSEnabled is true) == false) return;

        if (_environmentService.GetCurrentEnvironment() == CurrentEnvironment.MacOsStandalone)
        {
            Log.Error(
                "MAC TTS Is not currently supported, If you would like to add support Join the discord and contribute");
            return;
        }

        string jsonData = "";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(ApiUrls.TTSVersionFile);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Unable to download config" + response);
                return;
            }

            jsonData = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Log.Error("Unable to download ValidGoodFilesVersionAsync config" + e);
            return;
        }


        VersionModel? CurrentVersion = JsonSerializer.Deserialize<VersionModel>(jsonData, options: new()
        {
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = true
        });

        if (CurrentVersion == null)
        {
            return;
        }

        string installationBasePath = _preferencesService.GetPreferences().InstallationPath;

        VersionModel? localVersionModel = null;

        try
        {
            var LocalVersion = System.IO.Path.Combine(installationBasePath, "tts", "version.txt");
            if (System.IO.File.Exists(LocalVersion))
            {
                // Read the JSON file content
                string jsonContent = System.IO.File.ReadAllText(LocalVersion);

                // Deserialize the JSON content into an object
                localVersionModel = JsonSerializer.Deserialize<VersionModel>(jsonContent);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred: {ex.Message}");
        }


        try
        {
            if (localVersionModel == null || localVersionModel.version != CurrentVersion.version)
            {
                var LocalVersion = System.IO.Path.Combine(installationBasePath, "tts");
                if (System.IO.Directory.Exists(LocalVersion))
                {
                    System.IO.Directory.Delete(LocalVersion, true);
                }


                var zip = _environmentService.GetCurrentEnvironment() switch
                {
                    CurrentEnvironment.WindowsStandalone => "win.zip",
                    //CurrentEnvironment.MacOsStandalone => "mac.zip",
                    CurrentEnvironment.LinuxStandalone or CurrentEnvironment.LinuxFlatpak => "lnx.tar.xz",
                    _ => null
                };

                Download.Active = true;
                Download.DownloadState = DownloadState.InProgress;
                HttpResponseMessage request = await _httpClient.GetAsync(ApiUrls.TTSFiles + "/" + zip,
                    HttpCompletionOption.ResponseHeadersRead);

                Download.Size = request.Content.Headers.ContentLength ??
                                throw new ContentLengthNullException(ApiUrls.TTSFiles + "/" + zip);
                using Stream responseStream = await request.Content.ReadAsStreamAsync();
                Log.Information("Download connection established");
                await using ProgressStream progressStream = new(responseStream);
                using IDisposable logProgressDisposable = InstallationService.LogProgress(progressStream, Download);

                using IDisposable progressDisposable =
                    progressStream.Progress.Subscribe(p => { Download.Downloaded = p; });

                await Task.Run(() => ExtractTo(progressStream, LocalVersion, Download));
            }
            //Is find no need to update
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    private void ExtractTo(Stream progressStream, string LocalVersion, Download Download)
    {
        Download.DownloadState = DownloadState.Extracting;
        switch (_environmentService.GetCurrentEnvironment())
        {
            case CurrentEnvironment.WindowsStandalone:
                {
                    ZipArchive archive = new(progressStream);
                    archive.ExtractToDirectory(LocalVersion, true);
                    break;
                }
            case CurrentEnvironment.LinuxStandalone or CurrentEnvironment.LinuxFlatpak:
                {
                    using var decompressedStream = DecompressXz(progressStream); // Decompress XZ stream to get .tar
                    ExtractTar(decompressedStream, LocalVersion);
                    break;
                }
            default:
                throw new Exception("Unsupported OS");
        }

        Download.Active = false;
        Download.DownloadState = DownloadState.InProgress;
    }

    private static Stream DecompressXz(Stream compressedStream)
    {
        var decompressedStream = new MemoryStream();
        using (var xzStream = new SharpCompress.Compressors.Xz.XZStream(compressedStream))
        {
            xzStream.CopyTo(decompressedStream);
        }

        decompressedStream.Seek(0, SeekOrigin.Begin);
        return decompressedStream;
    }


    private void ExtractTar(Stream tarStream, string destinationPath)
    {
        using var archive = TarArchive.Open(tarStream);
        foreach (var entry in archive.Entries)
        {
            if (!entry.IsDirectory)
            {
                entry.WriteToDirectory(destinationPath, new ExtractionOptions
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }
    }

    private (string?, string?) FindExecutable()
    {
        string installationBasePath = _preferencesService.GetPreferences().InstallationPath;
        if (string.IsNullOrWhiteSpace(installationBasePath) || !Directory.Exists(installationBasePath))
        {
            return (null, null);
        }

        return _environmentService.GetCurrentEnvironment() switch
        {
            CurrentEnvironment.WindowsStandalone
                => (Path.Combine(installationBasePath, "tts", "python-3.10.11.amd64", "python.exe"), Path.Combine(installationBasePath, "tts", "scripts")),
            CurrentEnvironment.MacOsStandalone
                => throw new NotImplementedException("tts Mac Support not implemented"),
            CurrentEnvironment.LinuxStandalone or CurrentEnvironment.LinuxFlatpak
                => (Path.Combine(installationBasePath, "tts", "bin", "python"), Path.Combine(installationBasePath, "tts", "bin")),
            _ => (null, null)
        };
    }

    private void EnsureExecutableFlagOnUnixSystems(string executablePath)
    {
        if (_environmentService.GetCurrentEnvironment() != CurrentEnvironment.WindowsStandalone)
        {
            UnixFileInfo fileInfo = new(executablePath);
            fileInfo.FileAccessPermissions |= FileAccessPermissions.UserReadWriteExecute;
        }
    }

    public void StartTTS()
    {
        var Preference = _preferencesService.GetPreferences();
        if ((Preference.TTSEnabled is true) == false) return;

        if (process != null && process.HasExited == false)
        {
            return;
        }

        string installationBasePath = _preferencesService.GetPreferences().InstallationPath;
        var LocalVersion = System.IO.Path.Combine(installationBasePath, "tts");
        if (System.IO.Directory.Exists(LocalVersion) == false)
        {
            return; //Not installed
        }

        (string?, string?) executable = FindExecutable();
        if (string.IsNullOrWhiteSpace(executable.Item1))
        {
            const string failureReason = "Couldn't find executable to start.";
            Log.Warning(failureReason + $" Installation Path: {executable.Item1 ?? "null"}");
            return;
        }

        EnsureExecutableFlagOnUnixSystems(executable.Item1);

        string arguments = "TTS_local_Server.py";
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            WorkingDirectory = executable.Item2,
            FileName = executable.Item1,
            ArgumentList = { arguments },
            UseShellExecute = false, // Don't use the shell
            CreateNoWindow = true, // Run without creating a window
            RedirectStandardOutput = true, // Optional: Redirect output for logging
            RedirectStandardError = true, // Optional: Redirect error output
        };

        if (startInfo == null)
        {
            const string failureReason = "Unhandled platform.";
            Log.Warning(failureReason + $" Platform: {Enum.GetName(_environmentService.GetCurrentEnvironment())}");
            return;
        }

        // Start the process
        process = new Process();

        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;

        // Handle process exit to clean up if needed
        process.Exited += (sender, e) => { Log.Information($"Subprocess with PID {process.Id} exited."); };

        // Ensure subprocess ends when the main application exits
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            if (process.HasExited == false)
            {
                process.Kill();
            }
        };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            Log.Error($"Error starting process: {ex.Message}");
        }
    }

    public void StopTTS()
    {
        if (process != null)
        {
            if (process.HasExited == false)
            {
                process.Kill();
            }
        }
    }
}