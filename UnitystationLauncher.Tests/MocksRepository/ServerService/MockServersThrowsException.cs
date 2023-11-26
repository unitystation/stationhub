using System.Collections.Generic;
using System.Threading.Tasks;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.ServerService;

public class MockServersThrowsException : IServerService
{
    public Task<List<Server>> GetServersAsync()
    {
        throw new();
    }

    public bool IsInstallationInUse(Installation installation)
    {
        throw new();
    }
}