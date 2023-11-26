using System.Threading.Tasks;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.PingService;

public class MockPingReturnsNull : IPingService
{
    public Task<string> GetPing(Server server)
    {
        return Task.FromResult(null as string)!;
    }
}