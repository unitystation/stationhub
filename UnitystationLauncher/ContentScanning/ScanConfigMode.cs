﻿using System;
using System.Collections.Generic;
using System.Globalization;
using ILVerify;

namespace UnitystationLauncher.ContentScanning;

public sealed class SandboxConfig
{
    public string? SystemAssemblyName { get; set; }
    public List<VerifierError> AllowedVerifierErrors { get; set; } = new List<VerifierError>();
    public List<string> WhitelistedNamespaces { get; set; } = new List<string>();
    public List<string> MultiAssemblyOtherReferences { get; set; } = new List<string>();

    public Dictionary<string, Dictionary<string, TypeConfig>> Types { get; set; } =
        new Dictionary<string, Dictionary<string, TypeConfig>>();
}

public enum InheritMode : byte
{
    // Allow if All is set, block otherwise
    Default,
    Allow,

    // Block even is All is set
    Block
}