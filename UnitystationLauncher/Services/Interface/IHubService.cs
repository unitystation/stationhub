using System.Threading.Tasks;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.Services.Interface;

public interface IHubService
{
    public Task<HubClientConfig?> GetServerHubClientConfigAsync();
}