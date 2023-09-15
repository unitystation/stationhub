using System.IO;

namespace UnitystationLauncher.Services.Interface;

public interface IFileService
{
    public StreamReader OpenText(string path);

    public bool Exists(string path);
}