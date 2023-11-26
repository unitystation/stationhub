using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using ILVerify;
using Serilog;

namespace UnitystationLauncher.ContentScanning;

public sealed class Resolver : IResolver
{
    private readonly DirectoryInfo _managedPath;


    private readonly Dictionary<string, PEReader> _dictionaryLookup = new();

    public Resolver(DirectoryInfo inManagedPath)
    {
        _managedPath = inManagedPath;
    }

    public void Dispose()
    {
        foreach (KeyValuePair<string, PEReader> lookup in _dictionaryLookup)
        {
            lookup.Value.Dispose();
        }
    }

    PEReader IResolver.ResolveAssembly(AssemblyName assemblyName)
    {
        if (assemblyName.Name == null)
        {
            throw new FileNotFoundException("Unable to find " + assemblyName.FullName);
        }

        if (_dictionaryLookup.TryGetValue(assemblyName.Name, out PEReader? assembly))
        {
            return assembly;
        }

        FileInfo[] files = _managedPath.GetFiles("*.dll"); // Change the file extension to match your DLLs

        foreach (FileInfo file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (string.Equals(fileName, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information($"Found DLL for assembly '{assemblyName.Name}': {file.FullName}");
                _dictionaryLookup[assemblyName.Name] =
                    new(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read));
                return _dictionaryLookup[assemblyName.Name];
            }
        }

        files = _managedPath.GetFiles("*.so"); // Change the file extension to match Linux stuff to

        foreach (FileInfo file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (string.Equals(fileName, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information($"Found DLL for assembly '{assemblyName.Name}': {file.FullName}");
                _dictionaryLookup[assemblyName.Name] =
                    new(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read));
                return _dictionaryLookup[assemblyName.Name];
            }
        }

        files = _managedPath.GetFiles("*.dylib"); // Change the file extension to match mac stuff to

        foreach (FileInfo file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (string.Equals(fileName, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information($"Found DLL for assembly '{assemblyName.Name}': {file.FullName}");
                _dictionaryLookup[assemblyName.Name] =
                    new(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read));
                return _dictionaryLookup[assemblyName.Name];
            }
        }

        throw new FileNotFoundException("Unable to find it " + assemblyName.FullName);
    }

    PEReader IResolver.ResolveModule(AssemblyName referencingAssembly, string fileName)
    {
        //TODO idk This is never used anywhere
        throw new NotImplementedException(
            $"idk How IResolver.ResolveModule(AssemblyName {referencingAssembly}, string {fileName}) , And it's never been called so.. ");
    }
}