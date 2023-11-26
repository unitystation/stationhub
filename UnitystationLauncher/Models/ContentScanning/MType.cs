using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using UnitystationLauncher.ContentScanning;
using UnitystationLauncher.Exceptions;
using UnitystationLauncher.Infrastructure;

namespace UnitystationLauncher.Models.ContentScanning;


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

    internal bool ParseInheritType(EntityHandle handle, [NotNullWhen(true)] out MType? type, MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        type = default;

        switch (handle.Kind)
        {
            case HandleKind.TypeDefinition:
                // Definition to type in same assembly, allowed without hassle.
                return false;

            case HandleKind.TypeReference:
                // Regular type reference.
                try
                {
                    type = reader.ParseTypeReference((TypeReferenceHandle)handle);
                    return true;
                }
                catch (UnsupportedMetadataException u)
                {
                    errors.Add(new(u));
                    return false;
                }

            case HandleKind.TypeSpecification:
                TypeSpecification typeSpec = reader.GetTypeSpecification((TypeSpecificationHandle)handle);
                // Generic type reference.
                TypeProvider provider = new();
                type = typeSpec.DecodeSignature(provider, 0);

                if (type.IsCoreTypeDefined())
                {
                    // Ensure this isn't a self-defined type.
                    // This can happen due to generics.
                    return false;
                }

                break;

            default:
                errors.Add(new(
                    $"Unsupported BaseType of kind {handle.Kind} on type {this}"));
                return false;
        }

        type = default!;
        return false;
    }
}

