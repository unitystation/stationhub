using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class GoodFileService : IGoodFileService
{
    private readonly HttpClient _httpClient;
    
    private readonly IPreferencesService _preferencesService;

    private const string GoodFileURL = "Https://unitystationfile.b-cdn.net/GoodFiles/";
    
    public GoodFileService(HttpClient httpClient, IPreferencesService preferencesService)
    {
        _httpClient = httpClient;
        _preferencesService = preferencesService;
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

    private static string GetFolderName(string version)
    {

        var OS = "Windows";
        switch (OS) //TODO get OS
        {
            case "Windows":
                return version + "_Windows";
        }

        return "idk";
    }
    
    public async Task<bool> ValidGoodFilesVersion(string goodFileVersion)
    {
        var jsonData = "";
        try
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://unitystationfile.b-cdn.net/GoodFiles/AllowGoodFiles.json");
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Unable to download config" + response.ToString());
                return false;
            }
        
            jsonData = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Log.Error("Unable to download ValidGoodFilesVersion config" + e.ToString());
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