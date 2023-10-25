using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class GoodFileService : IGoodFileService
{
    private readonly HttpClient _httpClient;
    
    private readonly IPreferencesService _preferencesService;
    private readonly IEnvironmentService _environmentService;

    private const string GoodFileURL = "Https://unitystationfile.b-cdn.net/GoodFiles/";
    
    public GoodFileService(HttpClient httpClient, IPreferencesService preferencesService, IEnvironmentService environmentService)
    {
        _httpClient = httpClient;
        _preferencesService = preferencesService;
        _environmentService = environmentService;

    }

    public async Task<(string, bool)> GetGoodFileVersion(string version)
    {
        if (await ValidGoodFilesVersion(version) == false)
        {
            return ("", false);
        }
  
        var pathBase = _preferencesService.GetPreferences().InstallationPath;
        var folderName = GetFolderName(version);
        var versionPath = Path.Combine(pathBase, version, folderName);
        
        if (Directory.Exists(versionPath) == false)
        {
            
            HttpResponseMessage request = await _httpClient.GetAsync(GoodFileURL + version +"/" + folderName + ".zip", HttpCompletionOption.ResponseHeadersRead);
            await using Stream responseStream = await request.Content.ReadAsStreamAsync();
            ZipArchive archive = new(responseStream);
            archive.ExtractToDirectory(Path.Combine(versionPath), true);
        }
        
        return (versionPath, true);
    }

    private string GetFolderName(string version)
    {
        var OS = _environmentService.GetCurrentEnvironment();
        switch (OS)
        {
            case CurrentEnvironment.WindowsStandalone:
                return version + "_Windows";
            case CurrentEnvironment.LinuxFlatpak:
            case CurrentEnvironment.LinuxStandalone:
                return version + "_Linux";
            case CurrentEnvironment.MacOsStandalone:
                return version + "_Mac";
            default:
                throw new Exception($"Unable to determine OS Version {OS}");
        }
    }
    
    public async Task<bool> ValidGoodFilesVersion(string goodFileVersion)
    {
        var jsonData = "";
        try
        {
            var response = await _httpClient.GetAsync("https://unitystationfile.b-cdn.net/GoodFiles/AllowGoodFiles.json");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Unable to download config" + response);
                return false;
            }
        
            jsonData = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Log.Error("Unable to download ValidGoodFilesVersion config" + e);
            return false; 
        }

        
        var allowedList = JsonSerializer.Deserialize<HashSet<string>>(jsonData, options: new()
        {
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = true
        });

        if (allowedList == null)
        {
            return false;
        }
        
        return allowedList.Contains(goodFileVersion);
    }

    public string SanitiseStringPath(string inString)
    {
        return inString.Replace(@"\", "").Replace("/", "").Replace(".", "_");
    }

    private bool TryDownloadVersion()
    {
        return false;
    }
}