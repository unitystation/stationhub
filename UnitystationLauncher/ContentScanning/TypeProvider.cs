using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.ContentScanning;
using UnitystationLauncher.Models.ContentScanning.ScanningTypes;

namespace UnitystationLauncher.ContentScanning;

internal sealed class TypeProvider : ISignatureTypeProvider<MType, int>
{
    public MType GetSZArrayType(MType elementType)
    {
        return new MTypeSzArray(elementType);
    }

    public MType GetArrayType(MType elementType, ArrayShape shape)
    {
        return new MTypeWackyArray(elementType, shape);
    }

    public MType GetByReferenceType(MType elementType)
    {
        return new MTypeByRef(elementType);
    }

    public MType GetGenericInstantiation(MType genericType, ImmutableArray<MType> typeArguments)
    {
        return new MTypeGeneric(genericType, typeArguments);
    }

    public MType GetPointerType(MType elementType)
    {
        return new MTypePointer(elementType);
    }

    public MType GetPrimitiveType(PrimitiveTypeCode typeCode)
    {
        return new MTypePrimitive(typeCode);
    }

    public MType GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
    {
        return reader.GetTypeFromDefinition(handle);
    }

    public MType GetTypeFromReference(MetadataReader inReader, TypeReferenceHandle inHandle, byte inRawTypeKind)
    {
        return inReader.ParseTypeReference(inHandle);
    }

    public MType GetFunctionPointerType(MethodSignature<MType> signature)
    {
        throw new NotImplementedException();
    }

    public MType GetGenericMethodParameter(int genericContext, int index)
    {
        return new MTypeGenericMethodPlaceHolder(index);
    }

    public MType GetGenericTypeParameter(int genericContext, int index)
    {
        return new MTypeGenericTypePlaceHolder(index);
    }

    public MType GetModifiedType(MType modifier, MType unmodifiedType, bool isRequired)
    {
        return new MTypeModified(unmodifiedType, modifier, isRequired);
    }

    public MType GetPinnedType(MType elementType)
    {
        throw new NotImplementedException();
    }

    public MType GetTypeFromSpecification(MetadataReader reader, int genericContext,
        TypeSpecificationHandle handle,
        byte rawTypeKind)
    {
        return reader.GetTypeSpecification(handle).DecodeSignature(this, 0);
    }
}