using System;
using System.Collections.Generic;
using ILVerify;
using UnitystationLauncher.Models.ContentScanning;

namespace UnitystationLauncher.Models.ConfigFile;

[Serializable]
public sealed class SandboxConfig
{
    public string? SystemAssemblyName { get; set; }
    public List<VerifierError> AllowedVerifierErrors { get; set; } = new();
    public List<string> WhitelistedNamespaces { get; set; } = new();
    public List<string> MultiAssemblyOtherReferences { get; set; } = new();

    public Dictionary<string, Dictionary<string, TypeConfig>> Types { get; set; } = new();
}