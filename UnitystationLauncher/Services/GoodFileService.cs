using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class GoodFileService
{
    private readonly HttpClient _httpClient;
    
    private readonly IPreferencesService _preferencesService;

    private const string GoodFileURL = "";
    
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
        var versionPath = Path.Combine(pathBase, version.Replace(".", "_"));

        if (Directory.Exists(versionPath) == false)
        {
            HttpResponseMessage request = await _httpClient.GetAsync(GoodFileURL, HttpCompletionOption.ResponseHeadersRead);
        }

        return ("AAA", false);


    }
    
    private static async Task<bool> ValidGoodFilesVersion(string goodFileVersion)
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

    private static string SanitiseStringPath(string inString)
    {
        return inString.Replace(@"\", "").Replace("/", "").Replace(".", "_");
    }

    private bool TryDownloadVersion()
    {
        return false;
    }
}