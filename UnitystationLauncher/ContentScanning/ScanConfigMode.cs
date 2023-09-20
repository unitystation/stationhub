using System;
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

public sealed class WhitelistMethodDefine
{
    public string Name { get; }
    public MType ReturnType { get; }
    public List<MType> ParameterTypes { get; }
    public int GenericParameterCount { get; }

    public WhitelistMethodDefine(
        string name,
        MType returnType,
        List<MType> parameterTypes,
        int genericParameterCount)
    {
        Name = name;
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
        GenericParameterCount = genericParameterCount;
    }
}

public sealed class WhitelistFieldDefine
{
    public string Name { get; }
    public MType FieldType { get; }

    public WhitelistFieldDefine(string name, MType fieldType)
    {
        Name = name;
        FieldType = fieldType;
    }
}

public record MType
{
    public virtual bool WhitelistEquals(MType other)
    {
        return false;
    }

    public virtual bool IsCoreTypeDefined()
    {
        return false;
    }

    /// <summary>
    /// Outputs this type in a format re-parseable for the sandbox config whitelist.
    /// </summary>
    public virtual string? WhitelistToString()
    {
        return ToString();
    }
}

public sealed class TypeConfig
{
    // Used for type configs where the type config doesn't exist due to a bigger-scoped All whitelisting.
    // e.g. nested types or namespace whitelist.
    public static readonly TypeConfig DefaultAll = new TypeConfig { All = true };

    public bool All { get; set; }
    public InheritMode Inherit { get; set; } = InheritMode.Default;
    public string[]? Methods { get; set; }
    [NonSerialized] public WhitelistMethodDefine[] MethodsParsed = Array.Empty<WhitelistMethodDefine>();
    public string[]? Fields { get; set; }
    [NonSerialized] public WhitelistFieldDefine[] FieldsParsed = Array.Empty<WhitelistFieldDefine>();
    public Dictionary<string, TypeConfig>? NestedTypes { get; set; }
}