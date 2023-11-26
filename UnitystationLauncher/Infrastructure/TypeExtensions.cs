using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Internal.TypeSystem.Ecma;
using UnitystationLauncher.ContentScanning;
using UnitystationLauncher.Models.ContentScanning;

namespace UnitystationLauncher.Infrastructure;

public static class TypeExtensions
{
    public static IEnumerable<string> DumpMetaMembers(this Type type)
    {
        string assemblyLoc = type.Assembly.Location;

        // Load assembly with System.Reflection.Metadata.
        using FileStream fs = File.OpenRead(assemblyLoc);
        using PEReader peReader = new(fs);

        MetadataReader metaReader = peReader.GetMetadataReader();

        // Find type definition in raw assembly metadata.
        // Is there a better way to do this than iterating??
        TypeDefinition typeDef = default;
        bool found = false;
        foreach (TypeDefinitionHandle typeDefHandle in metaReader.TypeDefinitions)
        {
            TypeDefinition tempTypeDef = metaReader.GetTypeDefinition(typeDefHandle);
            string name = metaReader.GetString(tempTypeDef.Name);
            string? @namespace = AssemblyTypeCheckerHelpers.NilNullString(metaReader, tempTypeDef.Namespace);
            if (name == type.Name && @namespace == type.Namespace)
            {
                typeDef = tempTypeDef;
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new InvalidOperationException("Type didn't exist??");
        }

        // Dump the list.
        TypeProvider provider = new();

        foreach (FieldDefinitionHandle fieldHandle in typeDef.GetFields())
        {
            FieldDefinition fieldDef = metaReader.GetFieldDefinition(fieldHandle);

            if ((fieldDef.Attributes & FieldAttributes.FieldAccessMask) != FieldAttributes.Public)
            {
                continue;
            }

            string fieldName = metaReader.GetString(fieldDef.Name);
            MType fieldType = fieldDef.DecodeSignature(provider, 0);

            yield return $"{fieldType.WhitelistToString()} {fieldName}";
        }

        foreach (MethodDefinitionHandle methodHandle in typeDef.GetMethods())
        {
            MethodDefinition methodDef = metaReader.GetMethodDefinition(methodHandle);

            if (!methodDef.Attributes.IsPublic())
            {
                continue;
            }

            string methodName = metaReader.GetString(methodDef.Name);
            MethodSignature<MType> methodSig = methodDef.DecodeSignature(provider, 0);

            string paramString = string.Join(", ", methodSig.ParameterTypes.Select(t => t.WhitelistToString()));
            int genericCount = methodSig.GenericParameterCount;
            string typeParamString = genericCount == 0
                ? ""
                : $"<{new string(',', genericCount - 1)}>";

            yield return $"{methodSig.ReturnType.WhitelistToString()} {methodName}{typeParamString}({paramString})";
        }
    }
}