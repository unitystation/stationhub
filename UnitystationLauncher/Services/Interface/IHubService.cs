using System.Threading.Tasks;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.Services.Interface;

/// <summary>
///   Handles getting the client config from the hub.
/// </summary>
public interface IHubService
{
    /// <summary>
    ///   Calls the hub server to get the current client config deployed there.
    /// </summary>
    /// <returns>The current hub client config.</returns>
    public Task<HubClientConfig?> GetServerHubClientConfigAsync();
}