using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ILVerify;
using Pidgin;
using Serilog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ContentScanning;

public sealed partial class AssemblyTypeChecker
{
    private static string NameConfig = @"CodeScanList.json"; //TODO!!!
    
    private SandboxConfig LoadConfig()
    {
        
        if (_fileService.Exists(Path.Combine(_environmentService.GetUserdataDirectory(), NameConfig)) == false) throw new NotImplementedException("Config is not downloaded"); //TODO Needs a file on the server to download
        
        using (StreamReader file = _fileService.OpenText(Path.Combine(_environmentService.GetUserdataDirectory(), NameConfig)))
        {
            try
            {
                var data = JsonSerializer.Deserialize<SandboxConfig>(file.ReadToEnd(), new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true
                });
                if (data == null)
                {
                    Log.Error("unable to de-serialise config");
                    throw new Exception("unable to de-serialise config");
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
                    list.Add(MethodParser.ParseOrThrow(m));
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
                // try
                // {
                list.Add(FieldParser.ParseOrThrow(f));
                // }
                // catch (ParseException e)
                // {
                //     //sawmill.Error($"Parse exception for '{f}': {e}");
                //     
                // }
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