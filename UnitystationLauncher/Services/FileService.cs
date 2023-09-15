using System.IO;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class FileService : IFileService
{
    public StreamReader OpenText(string path)
    {
        return File.OpenText(path);
    }

    public bool Exists(string path)
    {
        return File.Exists(path);
    }
   
}