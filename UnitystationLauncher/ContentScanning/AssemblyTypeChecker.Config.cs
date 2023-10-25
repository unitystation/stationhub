using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ILVerify;
using Pidgin;
using Serilog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ContentScanning;

public sealed partial class AssemblyTypeChecker
{
    private static string NameConfig = @"CodeScanList.json"; 

    private async Task<SandboxConfig> LoadConfig()
    {
        var configPath = Path.Combine(_environmentService.GetUserdataDirectory(), NameConfig);
        try
        {
            var response = await _httpClient.GetAsync("https://raw.githubusercontent.com/unitystation/unitystation/develop/CodeScanList.json");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                File.Delete(configPath);
                await File.WriteAllTextAsync(configPath, jsonData);
                Console.WriteLine("JSON file saved successfully.");
            }
            else
            {
                Log.Error("Unable to download config" + response.ToString());
            }
        }
        catch (Exception e)
        {
            Log.Error("Unable to download config" + e.ToString());
        }
        
        
        if (_fileService.Exists(configPath) == false)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "UnitystationLauncher.CodeScanList.json";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                // Copy the contents of the resource to a file location
                using (var fileStream = File.Create(configPath))
                {
                    stream.Seek(0L, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream);
                }
            }
            Log.Error("had to use backup config");
        }

        using (StreamReader file = _fileService.OpenText(configPath))
        {
            try
            {
                var data = JsonSerializer.Deserialize<SandboxConfig>(file.ReadToEnd(), new JsonSerializerOptions
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

                foreach (var @namespace in data.Types)
                {
                    foreach (var @class in @namespace.Value)
                    {
                        ParseTypeConfig(@class.Value);
                    }
                }

                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private static void ParseTypeConfig(TypeConfig cfg)
    {
        if (cfg.Methods != null)
        {
            var list = new List<WhitelistMethodDefine>();
            foreach (var m in cfg.Methods)
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
            var list = new List<WhitelistFieldDefine>();
            foreach (var f in cfg.Fields)
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
            foreach (var nested in cfg.NestedTypes.Values)
            {
                ParseTypeConfig(nested);
            }
        }
    }
}