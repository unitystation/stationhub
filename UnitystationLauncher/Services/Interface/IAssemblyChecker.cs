using System;
using System.Collections.Generic;
using System.IO;

namespace UnitystationLauncher.Services.Interface;

public interface IAssemblyChecker
{
    public bool CheckAssembly(FileInfo diskPath, DirectoryInfo managedPath, List<string> otherAssemblies, Action<string> info, Action<string> errors);
}