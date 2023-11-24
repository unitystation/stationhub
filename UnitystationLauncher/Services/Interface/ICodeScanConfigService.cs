using System.Threading.Tasks;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.Services.Interface;

public interface ICodeScanConfigService
{
    public Task<(string, bool)> GetGoodFileVersion(string version);

    public Task<bool> ValidGoodFilesVersion(string goodFileVersion);

    public string SanitiseStringPath(string inString);

    public Task<SandboxConfig> LoadConfigAsync();
}