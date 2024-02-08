using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnitystationLauncher.Models.ContentScanning;

namespace UnitystationLauncher.Services.Interface;

public interface IAssemblyTypeCheckerService
{
    public Task<bool> CheckAssemblyTypesAsync(FileInfo diskPath, DirectoryInfo managedPath, List<string> otherAssemblies, Action<ScanLog> scanLog);
}