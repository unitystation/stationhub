using System.Threading.Tasks;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.PingService;

public class MockPingStaticPingTime : IPingService
{
    private readonly string _pingTime;

    public MockPingStaticPingTime(int pingInMilliseconds)
    {
        _pingTime = $"{pingInMilliseconds}ms";
    }
    public Task<string> GetPingAsync(Server server)
    {
        return Task.FromResult(_pingTime);
    }
}