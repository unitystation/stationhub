using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UnitystationLauncher.Services.Interface;

public interface IAssemblyTypeCheckerService
{
    public Task<bool> CheckAssemblyTypesAsync(FileInfo diskPath, DirectoryInfo managedPath, List<string> otherAssemblies, Action<string> infoAction, Action<string> errorsAction);
}