using System.Threading.Tasks;

namespace UnitystationLauncher.Services.Interface;

public interface IGoodFileService
{
    public Task<(string, bool)> GetGoodFileVersion(string version);

    public Task<bool> ValidGoodFilesVersion(string goodFileVersion);

    public string SanitiseStringPath(string inString);
}