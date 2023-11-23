using System;
using System.Linq;
using System.Reflection.Metadata;

namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

// Multi-dimension arrays with funny lower and upper bounds.
internal sealed record MTypeWackyArray(MType ElementType, ArrayShape Shape) : MType
{
    public override string ToString()
    {
        return $"{ElementType}[TODO]";
    }

    public override string WhitelistToString()
    {
        return $"{ElementType.WhitelistToString()}[TODO]";
    }

    public override bool WhitelistEquals(MType other)
    {
        return other is MTypeWackyArray arr && ShapesEqual(Shape, arr.Shape) && ElementType.WhitelistEquals(arr);
    }

    private static bool ShapesEqual(in ArrayShape a, in ArrayShape b)
    {
        return a.Rank == b.Rank && a.LowerBounds.SequenceEqual(b.LowerBounds) && a.Sizes.SequenceEqual(b.Sizes);
    }

    public override bool IsCoreTypeDefined()
    {
        return ElementType.IsCoreTypeDefined();
    }
}