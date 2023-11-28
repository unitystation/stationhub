using System.Threading.Tasks;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.Services.Interface;

public interface ICodeScanConfigService
{
    public Task<(string, bool)> GetGoodFileVersionAsync(string version);

    public Task<bool> ValidGoodFilesVersionAsync(string goodFileVersion);

    public Task<SandboxConfig> LoadConfigAsync();
}