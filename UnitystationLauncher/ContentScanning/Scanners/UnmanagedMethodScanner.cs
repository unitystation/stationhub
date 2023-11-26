using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Metadata;
using UnitystationLauncher.Infrastructure;

namespace UnitystationLauncher.ContentScanning.Scanners;

internal static class UnmanagedMethodScanner
{
    internal static void CheckNoUnmanagedMethodDefs(MetadataReader reader, ConcurrentBag<SandboxError> errors)
    {
        foreach (MethodDefinitionHandle methodDefHandle in reader.MethodDefinitions)
        {
            MethodDefinition methodDef = reader.GetMethodDefinition(methodDefHandle);
            MethodImplAttributes implAttr = methodDef.ImplAttributes;
            MethodAttributes attr = methodDef.Attributes;

            if ((implAttr & MethodImplAttributes.Unmanaged) != 0 ||
                (implAttr & MethodImplAttributes.CodeTypeMask) is not (MethodImplAttributes.IL
                or MethodImplAttributes.Runtime))
            {
                string err = $"Method has illegal MethodImplAttributes: {reader.FormatMethodName(methodDef)}";
                errors.Add(new(err));
            }

            if ((attr & (MethodAttributes.PinvokeImpl | MethodAttributes.UnmanagedExport)) != 0)
            {
                string err = $"Method has illegal MethodAttributes: {reader.FormatMethodName(methodDef)}";
                errors.Add(new(err));
            }
        }
    }
}