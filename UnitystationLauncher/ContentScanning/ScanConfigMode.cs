using System;
using System.Collections.Generic;
using System.Globalization;
using ILVerify;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UnitystationLauncher.ContentScanning;




public sealed class SandboxConfig
{
    public string SystemAssemblyName = default!;
    public HashSet<VerifierError> AllowedVerifierErrors = default!;
    public List<string> WhitelistedNamespaces = default!;
    public List<string> WhitelistedAssembliesDEBUG = new List<string>();
    public Dictionary<string, Dictionary<string, TypeConfig>> Types = default!;
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

public abstract record MType
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
    public static readonly TypeConfig DefaultAll = new TypeConfig {All = true};

    public bool All;
    public InheritMode Inherit = InheritMode.Default;
    public string[]? Methods;
    [NonSerialized] public WhitelistMethodDefine[] MethodsParsed = Array.Empty<WhitelistMethodDefine>();
    public string[]? Fields;
    [NonSerialized] public WhitelistFieldDefine[] FieldsParsed = Array.Empty<WhitelistFieldDefine>();
    public Dictionary<string, TypeConfig>? NestedTypes;
}

