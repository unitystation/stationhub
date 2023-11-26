using System.Reflection.Metadata;

namespace UnitystationLauncher.Infrastructure;

internal static class StringHandleExtensions
{
    internal static string? NilNullString(this StringHandle handle, MetadataReader reader)
    {
        return handle.IsNil ? null : reader.GetString(handle);
    }
}