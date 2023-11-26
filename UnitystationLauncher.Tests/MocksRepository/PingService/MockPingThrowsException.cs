using System.Threading.Tasks;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.PingService;

public class MockPingThrowsException : IPingService
{
    public Task<string> GetPing(Server server)
    {
        throw new();
    }
}