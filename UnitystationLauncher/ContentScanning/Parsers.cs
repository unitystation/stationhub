using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using Pidgin;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace UnitystationLauncher.ContentScanning;

public static class Parsers
{
    // Contains primary parsing code for method and field declarations in the sandbox whitelist.

    private static readonly Parser<char, PrimitiveTypeCode> _voidTypeParser =
        String("void").ThenReturn(PrimitiveTypeCode.Void);

    private static readonly Parser<char, PrimitiveTypeCode> _booleanTypeParser =
        String("bool").ThenReturn(PrimitiveTypeCode.Boolean);

    private static readonly Parser<char, PrimitiveTypeCode> _charTypeParser =
        String("char").ThenReturn(PrimitiveTypeCode.Char);

    private static readonly Parser<char, PrimitiveTypeCode> _sByteTypeParser =
        String("sbyte").ThenReturn(PrimitiveTypeCode.SByte);

    private static readonly Parser<char, PrimitiveTypeCode> _byteTypeParser =
        String("byte").ThenReturn(PrimitiveTypeCode.Byte);

    private static readonly Parser<char, PrimitiveTypeCode> _int16TypeParser =
        String("short").ThenReturn(PrimitiveTypeCode.Int16);

    private static readonly Parser<char, PrimitiveTypeCode> _uInt16TypeParser =
        String("ushort").ThenReturn(PrimitiveTypeCode.UInt16);

    private static readonly Parser<char, PrimitiveTypeCode> _int32TypeParser =
        String("int").ThenReturn(PrimitiveTypeCode.Int32);

    private static readonly Parser<char, PrimitiveTypeCode> _uInt32TypeParser =
        String("uint").ThenReturn(PrimitiveTypeCode.UInt32);

    private static readonly Parser<char, PrimitiveTypeCode> _int64TypeParser =
        String("long").ThenReturn(PrimitiveTypeCode.Int64);

    private static readonly Parser<char, PrimitiveTypeCode> _uInt64TypeParser =
        String("ulong").ThenReturn(PrimitiveTypeCode.UInt64);

    private static readonly Parser<char, PrimitiveTypeCode> _intPtrTypeParser =
        String("nint").ThenReturn(PrimitiveTypeCode.IntPtr);

    private static readonly Parser<char, PrimitiveTypeCode> _uIntPtrTypeParser =
        String("nuint").ThenReturn(PrimitiveTypeCode.UIntPtr);

    private static readonly Parser<char, PrimitiveTypeCode> _singleTypeParser =
        String("float").ThenReturn(PrimitiveTypeCode.Single);

    private static readonly Parser<char, PrimitiveTypeCode> _doubleTypeParser =
        String("double").ThenReturn(PrimitiveTypeCode.Double);

    private static readonly Parser<char, PrimitiveTypeCode> _stringTypeParser =
        String("string").ThenReturn(PrimitiveTypeCode.String);

    private static readonly Parser<char, PrimitiveTypeCode> _objectTypeParser =
        String("object").ThenReturn(PrimitiveTypeCode.Object);

    private static readonly Parser<char, PrimitiveTypeCode> _typedReferenceTypeParser =
        String("typedref").ThenReturn(PrimitiveTypeCode.TypedReference);

    private static readonly Parser<char, MType> _primitiveTypeParser =
        OneOf(
                Try(_voidTypeParser),
                Try(_booleanTypeParser),
                Try(_charTypeParser),
                Try(_sByteTypeParser),
                Try(_byteTypeParser),
                Try(_int16TypeParser),
                Try(_uInt16TypeParser),
                Try(_int32TypeParser),
                Try(_uInt32TypeParser),
                Try(_int64TypeParser),
                Try(_uInt64TypeParser),
                Try(_intPtrTypeParser),
                Try(_uIntPtrTypeParser),
                Try(_singleTypeParser),
                Try(_doubleTypeParser),
                Try(_stringTypeParser),
                Try(_objectTypeParser),
                _typedReferenceTypeParser)
            .Select(code => (MType)new MTypePrimitive(code)).Labelled("Primitive type");

    private static readonly Parser<char, string> _namespacedIdentifier =
        Token(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '`')
            .AtLeastOnceString()
            .Labelled("valid identifier");

    private static readonly Parser<char, IEnumerable<MType>> _genericParametersParser =
        Rec(() => _maybeArrayTypeParser!)
            .Between(SkipWhitespaces)
            .Separated(Char(','))
            .Between(Char('<'), Char('>'));

    private static readonly Parser<char, MType> _genericMethodPlaceholderParser =
        String("!!")
            .Then(Digit.AtLeastOnceString())
            .Select(p => (MType)new MTypeGenericMethodPlaceHolder(int.Parse(p, CultureInfo.InvariantCulture)));

    private static readonly Parser<char, MType> _genericTypePlaceholderParser =
        String("!")
            .Then(Digit.AtLeastOnceString())
            .Select(p => (MType)new MTypeGenericTypePlaceHolder(int.Parse(p, CultureInfo.InvariantCulture)));

    private static readonly Parser<char, MType> _genericPlaceholderParser = Try(_genericTypePlaceholderParser)
        .Or(Try(_genericMethodPlaceholderParser)).Labelled("Generic placeholder");

    private static readonly Parser<char, MTypeParsed> _typeNameParser =
        Map(
            (a, b) => b.Aggregate(new MTypeParsed(a), (parsed, s) => new(s, parsed)),
            _namespacedIdentifier,
            Char('/').Then(_namespacedIdentifier).Many());

    private static readonly Parser<char, MType> _constructedObjectTypeParser =
        Map((arg1, arg2) =>
            {
                MType type = arg1;
                if (arg2.HasValue)
                {
                    type = new MTypeGeneric(type, arg2.Value.ToImmutableArray());
                }

                return type;
            },
            _typeNameParser,
            _genericParametersParser.Optional());

    private static readonly Parser<char, MType> _maybeArrayTypeParser = Map(
        (a, b) => b.Aggregate(a, (type, _) => new MTypeSzArray(type)),
        Try(_genericPlaceholderParser).Or(Try(_primitiveTypeParser)).Or(_constructedObjectTypeParser),
        String("[]").Many());

    private static readonly Parser<char, MType> _byRefTypeParser =
        String("ref")
            .Then(SkipWhitespaces)
            .Then(_maybeArrayTypeParser)
            .Select(t => (MType)new MTypeByRef(t))
            .Labelled("ByRef type");

    private static readonly Parser<char, MType> _typeParser = Try(_byRefTypeParser).Or(_maybeArrayTypeParser);

    private static readonly Parser<char, ImmutableArray<MType>> _methodParamsParser =
        _typeParser
            .Between(SkipWhitespaces)
            .Separated(Char(','))
            .Between(Char('('), Char(')'))
            .Select(p => p.ToImmutableArray());

    internal static readonly Parser<char, int> MethodGenericParameterCountParser =
        Try(Char(',').Many().Select(p => p.Count() + 1).Between(Char('<'), Char('>'))).Or(Return(0));

    internal static readonly Parser<char, WhitelistMethodDefine> MethodParser =
        Map(
            (a, b, d, c) => new WhitelistMethodDefine(b, a, c.ToList(), d),
            SkipWhitespaces.Then(_typeParser),
            SkipWhitespaces.Then(_namespacedIdentifier),
            MethodGenericParameterCountParser,
            SkipWhitespaces.Then(_methodParamsParser));

    internal static readonly Parser<char, WhitelistFieldDefine> FieldParser = Map(
        (a, b) => new WhitelistFieldDefine(b, a),
        _maybeArrayTypeParser.Between(SkipWhitespaces),
        _namespacedIdentifier);
}