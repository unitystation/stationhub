using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ILVerify;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.ContentScanning.Scanners;

internal static class IlScanner
{
    internal static bool DoVerifyIl(string name, IResolver resolver, PEReader peReader,
        MetadataReader reader, Action<string> info, Action<string> logErrors, SandboxConfig loadedCfg)
    {
        info.Invoke($"{name}: Verifying IL...");
        Stopwatch sw = Stopwatch.StartNew();
        ConcurrentBag<VerificationResult> bag = new();

        IlScanning(reader, resolver, peReader, bag);

        bool verifyErrors = false;
        foreach (VerificationResult res in bag)
        {
            bool error = AssemblyTypeCheckerHelpers.CheckVerificationResult(loadedCfg, res, name, reader, logErrors);
            if (error)
            {
                verifyErrors = true;
            }
        }

        info.Invoke($"{name}: Verified IL in {sw.Elapsed.TotalMilliseconds}ms");

        if (verifyErrors)
        {
            return false;
        }

        return true;
    }

    // TODO: We should probably just remove this if we aren't going to use it
    //private static void ParallelIlScanning(MetadataReader reader, IResolver resolver, PEReader peReader, ConcurrentBag<VerificationResult> bag)
    //{
    //    OrderablePartitioner<TypeDefinitionHandle> partitioner = Partitioner.Create(reader.TypeDefinitions);
    //    Parallel.ForEach(partitioner.GetPartitions(Environment.ProcessorCount), handle =>
    //    {
    //        Verifier ver = new(resolver);
    //        ver.SetSystemModuleName(new(AssemblyNames.SystemAssemblyName));
    //        while (handle.MoveNext())
    //        {
    //            foreach (VerificationResult? result in ver.Verify(peReader, handle.Current, verifyMethods: true))
    //            {
    //                bag.Add(result);
    //            }
    //        }
    //    });
    //}

    // Using the Non-parallel implementation of this
    private static void IlScanning(MetadataReader reader, IResolver resolver, PEReader peReader, ConcurrentBag<VerificationResult> bag)
    {
        Verifier ver = new(resolver);
        //mscorlib
        ver.SetSystemModuleName(new(AssemblyNames.SystemAssemblyName));
        foreach (TypeDefinitionHandle definition in reader.TypeDefinitions)
        {
            IEnumerable<VerificationResult> errors = ver.Verify(peReader, definition, verifyMethods: true);
            foreach (VerificationResult error in errors)
            {
                bag.Add(error);
            }
        }
    }
}