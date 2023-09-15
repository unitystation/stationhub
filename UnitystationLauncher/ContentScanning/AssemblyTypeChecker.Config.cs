using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ILVerify;
using Newtonsoft.Json;
using Pidgin;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ContentScanning;

public sealed partial class AssemblyTypeChecker
{
    private static string NameConfig = @"CodeScanList.json"; //TODO!!!
    
    private SandboxConfig LoadConfig()
    {
        
        if (_FileService.Exists(Path.Combine(_environmentService.GetUserdataDirectory(), NameConfig)) == false) return null; //TODO Needs a file on the server to download
        
        using (StreamReader file = _FileService.OpenText(Path.Combine(_environmentService.GetUserdataDirectory(), NameConfig)))
        {
            JsonSerializer serializer = new JsonSerializer();
            try
            {
                var data = (SandboxConfig) serializer.Deserialize(file, typeof(SandboxConfig));
                foreach (var Namespace in data.Types)
                {
                    foreach (var Class in Namespace.Value)
                    {
                        ParseTypeConfig(Class.Value);
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
                    Console.WriteLine("oh no..");
                    // sawmill.Error($"Parse exception for '{m}': {e}");
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