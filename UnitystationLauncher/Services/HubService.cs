using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class HubService : IHubService
{
    private HubClientConfig? _hubClientConfig;
    private readonly HttpClient _httpClient;

    public HubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HubClientConfig?> GetServerHubClientConfigAsync()
    {
        if (_hubClientConfig == null)
        {
            string data = await _httpClient.GetStringAsync(ApiUrls.ValidateUrl);
            _hubClientConfig = JsonSerializer.Deserialize<HubClientConfig>(data);
        }

        return _hubClientConfig;
    }
}