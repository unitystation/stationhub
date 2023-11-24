using System.Reflection.Metadata;

namespace UnitystationLauncher.Models.ContentScanning.ScanningTypes;

internal sealed record MTypePrimitive(PrimitiveTypeCode TypeCode) : MType
{
    public override string ToString()
    {
        return TypeCode switch
        {
            PrimitiveTypeCode.Void => "void",
            PrimitiveTypeCode.Boolean => "bool",
            PrimitiveTypeCode.Char => "char",
            PrimitiveTypeCode.SByte => "int8",
            PrimitiveTypeCode.Byte => "unsigned int8",
            PrimitiveTypeCode.Int16 => "int16",
            PrimitiveTypeCode.UInt16 => "unsigned int16",
            PrimitiveTypeCode.Int32 => "int32",
            PrimitiveTypeCode.UInt32 => "unsigned int32",
            PrimitiveTypeCode.Int64 => "int64",
            PrimitiveTypeCode.UInt64 => "unsigned int64",
            PrimitiveTypeCode.Single => "float32",
            PrimitiveTypeCode.Double => "float64",
            PrimitiveTypeCode.String => "string",
            // ReSharper disable once StringLiteralTypo
            PrimitiveTypeCode.TypedReference => "typedref",
            PrimitiveTypeCode.IntPtr => "native int",
            PrimitiveTypeCode.UIntPtr => "unsigned native int",
            PrimitiveTypeCode.Object => "object",
            _ => "???"
        };
    }

    public override string WhitelistToString()
    {
        return TypeCode switch
        {
            PrimitiveTypeCode.Void => "void",
            PrimitiveTypeCode.Boolean => "bool",
            PrimitiveTypeCode.Char => "char",
            PrimitiveTypeCode.SByte => "sbyte",
            PrimitiveTypeCode.Byte => "byte",
            PrimitiveTypeCode.Int16 => "short",
            PrimitiveTypeCode.UInt16 => "ushort",
            PrimitiveTypeCode.Int32 => "int",
            PrimitiveTypeCode.UInt32 => "uint",
            PrimitiveTypeCode.Int64 => "long",
            PrimitiveTypeCode.UInt64 => "ulong",
            PrimitiveTypeCode.Single => "float",
            PrimitiveTypeCode.Double => "double",
            PrimitiveTypeCode.String => "string",
            // ReSharper disable once StringLiteralTypo
            PrimitiveTypeCode.TypedReference => "typedref",
            // ReSharper disable once StringLiteralTypo
            PrimitiveTypeCode.IntPtr => "nint",
            // ReSharper disable once StringLiteralTypo
            PrimitiveTypeCode.UIntPtr => "unint",
            PrimitiveTypeCode.Object => "object",
            _ => "???"
        };
    }

    public override bool WhitelistEquals(MType other)
    {
        return Equals(other);
    }
}