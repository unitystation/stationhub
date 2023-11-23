using System.Collections.Immutable;

namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed class MMemberRefMethod : MMemberRef
{
    public readonly MType ReturnType;
    public readonly int GenericParameterCount;
    public readonly ImmutableArray<MType> ParameterTypes;

    public MMemberRefMethod(MType parentType, string name, MType returnType,
        int genericParameterCount, ImmutableArray<MType> parameterTypes) : base(parentType, name)
    {
        ReturnType = returnType;
        GenericParameterCount = genericParameterCount;
        ParameterTypes = parameterTypes;
    }

    public override string ToString()
    {
        return $"{ParentType}.{Name}({string.Join(", ", ParameterTypes)}) Returns {ReturnType}";
    }
}