using System;
using System.Collections.Immutable;
using System.Linq;

namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed record MTypeGeneric(MType GenericType, ImmutableArray<MType> TypeArguments) : MType
{
    public override string ToString()
    {
        return $"{GenericType}<{string.Join(", ", TypeArguments)}>";
    }

    public override string WhitelistToString()
    {
        return
            $"{GenericType.WhitelistToString()}<{string.Join(", ", TypeArguments.Select(t => t.WhitelistToString()))}>";
    }

    public override bool WhitelistEquals(MType other)
    {
        if (!(other is MTypeGeneric generic))
        {
            return false;
        }

        if (TypeArguments.Length != generic.TypeArguments.Length)
        {
            return false;
        }

        for (int i = 0; i < TypeArguments.Length; i++)
        {
            MType argA = TypeArguments[i];
            MType argB = generic.TypeArguments[i];

            if (!argA.WhitelistEquals(argB))
            {
                return false;
            }
        }

        return GenericType.WhitelistEquals(generic.GenericType);
    }

    public bool Equals(MTypeGeneric? otherGeneric)
    {
        return otherGeneric != null && GenericType.Equals(otherGeneric.GenericType) &&
                TypeArguments.SequenceEqual(otherGeneric.TypeArguments);
    }

    public override int GetHashCode()
    {
        HashCode hc = new();
        hc.Add(GenericType);
        foreach (MType typeArg in TypeArguments)
        {
            hc.Add(typeArg);
        }

        return hc.ToHashCode();
    }

    public override bool IsCoreTypeDefined()
    {
        return GenericType.IsCoreTypeDefined();
    }
}