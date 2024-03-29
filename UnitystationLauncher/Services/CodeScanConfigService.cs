﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Pidgin;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.ContentScanning;
using UnitystationLauncher.Exceptions;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class CodeScanConfigService : ICodeScanConfigService
{
    private static string _nameConfig = @"CodeScanList.json";

    private readonly HttpClient _httpClient;

    private readonly IPreferencesService _preferencesService;
    private readonly IEnvironmentService _environmentService;

    public CodeScanConfigService(HttpClient httpClient, IPreferencesService preferencesService, IEnvironmentService environmentService)
    {
        _httpClient = httpClient;
        _preferencesService = preferencesService;
        _environmentService = environmentService;

    }

    #region Public Interface
    public async Task<(string, bool)> GetGoodFileVersionAsync(string version)
    {
        if (await ValidGoodFilesVersionAsync(version) == false)
        {
            return ("", false);
        }

        string pathBase = _preferencesService.GetPreferences().InstallationPath;
        string folderName = GetFolderName(version);
        string versionPath = Path.Combine(pathBase, version, folderName);

        if (Directory.Exists(versionPath) == false)
        {
            string zipExtractPath = Path.Combine(pathBase, version);
            HttpResponseMessage request = await _httpClient.GetAsync($"{ApiUrls.GoodFilesBaseUrl}/{version}/{folderName}.zip", HttpCompletionOption.ResponseHeadersRead);
            await using Stream responseStream = await request.Content.ReadAsStreamAsync();
            ZipArchive archive = new(responseStream);
            archive.ExtractToDirectory(zipExtractPath, true);

            string zipDirectory = Path.Combine(zipExtractPath, GetZipFolderName());
            Directory.Move(zipDirectory, versionPath);
        }

        return (versionPath, true);
    }

    public async Task<bool> ValidGoodFilesVersionAsync(string goodFileVersion)
    {
        string jsonData = "";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(ApiUrls.AllowedGoodFilesUrl);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Unable to download config" + response);
                return false;
            }

            jsonData = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Log.Error("Unable to download ValidGoodFilesVersionAsync config" + e);
            return false;
        }


        HashSet<string>? allowedList = JsonSerializer.Deserialize<HashSet<string>>(jsonData, options: new()
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

    public async Task<SandboxConfig> LoadConfigAsync()
    {
        string configPath = Path.Combine(_environmentService.GetUserdataDirectory(), _nameConfig);
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(ApiUrls.CodeScanListUrl);
            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                File.Delete(configPath);
                await File.WriteAllTextAsync(configPath, jsonData);
                Log.Information("JSON file saved successfully.");
            }
            else
            {
                Log.Error("Unable to download config" + response);
            }
        }
        catch (Exception e)
        {
            Log.Error("Unable to download config" + e);
        }


        if (File.Exists(configPath) == false)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "UnitystationLauncher.CodeScanList.json";
            await using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    // Copy the contents of the resource to a file location
                    await using FileStream fileStream = File.Create(configPath);
                    stream.Seek(0L, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream);
                }
            }
            Log.Error("had to use backup config");
        }

        using StreamReader file = File.OpenText(configPath);
        try
        {
            SandboxConfig? data = JsonSerializer.Deserialize<SandboxConfig>(await file.ReadToEndAsync(), new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                Converters =
                {
                    new JsonStringEnumConverter(allowIntegerValues: false)
                }
            });

            if (data == null)
            {
                Log.Error("unable to de-serialise config");
                throw new DataException("unable to de-serialise config");
            }

            foreach (KeyValuePair<string, Dictionary<string, TypeConfig>> @namespace in data.Types)
            {
                foreach (KeyValuePair<string, TypeConfig> @class in @namespace.Value)
                {
                    ParseTypeConfig(@class.Value);
                }
            }

            return data;
        }
        catch (Exception e)
        {
            Log.Error(e, e.Message);
            throw;
        }
    }
    #endregion

    #region Private Helpers
    private string GetZipFolderName()
    {
        CurrentEnvironment os = _environmentService.GetCurrentEnvironment();
        switch (os)
        {
            case CurrentEnvironment.WindowsStandalone:
                return "Windows";
            case CurrentEnvironment.LinuxFlatpak:
            case CurrentEnvironment.LinuxStandalone:
                return "Linux";
            case CurrentEnvironment.MacOsStandalone:
                return "Mac";
            default:
                throw new UnsupportedPlatformException($"Unable to determine OS Version {os}");
        }
    }

    private string GetFolderName(string version)
    {
        CurrentEnvironment os = _environmentService.GetCurrentEnvironment();
        switch (os)
        {
            case CurrentEnvironment.WindowsStandalone:
                return version + "_Windows";
            case CurrentEnvironment.LinuxFlatpak:
            case CurrentEnvironment.LinuxStandalone:
                return version + "_Linux";
            case CurrentEnvironment.MacOsStandalone:
                return version + "_Mac";
            default:
                throw new UnsupportedPlatformException($"Unable to determine OS Version {os}");
        }
    }

    private static void ParseTypeConfig(TypeConfig cfg)
    {
        if (cfg.Methods != null)
        {
            List<WhitelistMethodDefine> list = new();
            foreach (string m in cfg.Methods)
            {
                try
                {
                    list.Add(Parsers.MethodParser.ParseOrThrow(m));
                }
                catch (ParseException e)
                {
                    Log.Error($"Parse exception for '{m}': {e}");
                }
            }

            cfg.MethodsParsed = list.ToArray();
        }
        else
        {
            cfg.MethodsParsed = Array.Empty<WhitelistMethodDefine>();
        }

        if (cfg.Fields != null)
        {
            List<WhitelistFieldDefine> list = new();
            foreach (string f in cfg.Fields)
            {
                try
                {
                    list.Add(Parsers.FieldParser.ParseOrThrow(f));
                }
                catch (ParseException e)
                {
                    Log.Error($"Parse exception for '{f}': {e}");
                    throw;
                }
            }

            cfg.FieldsParsed = list.ToArray();
        }
        else
        {
            cfg.FieldsParsed = Array.Empty<WhitelistFieldDefine>();
        }

        if (cfg.NestedTypes != null)
        {
            foreach (TypeConfig nested in cfg.NestedTypes.Values)
            {
                ParseTypeConfig(nested);
            }
        }
    }
    #endregion
}