using System.Collections.Generic;
using System.Threading.Tasks;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.ServerService;

public class MockRandomServers : IServerService
{
    private static readonly List<string> _serverNames = new()
    {
        "UnitTestStation",
        "Unitystation",
        "SS15",
        "PlayStation",
        "A really long name that is really long for no real reason other than to be really long, its kind of funny thinking about how the launcher would probably render this.",
        "HonkStation",
        "RobustStation",
        "ЮнитиСтанция",
        "The name above is Unitystation in Russian",
        "🤡 Station",
        "I have no idea how many names this needs but I'll just end it here."
    };

    private static readonly Random _random = new();

    private readonly int _min;
    private readonly int _max;

    public MockRandomServers(int min, int max)
    {
        _min = min;
        _max = max;
    }

    public Task<List<Server>> GetServersAsync()
    {
        List<Server> servers = new();

        int numberOfServers = _random.Next(_min, _max);
        for (int i = 0; i < numberOfServers; i++)
        {
            servers.Add(new(_serverNames[_random.Next(_serverNames.Count)], _random.Next(0, 10), "127.0.0.1", _random.Next(1, 65535)));
        }

        return Task.FromResult(servers);
    }

    public bool IsInstallationInUse(Installation installation)
    {
        throw new NotImplementedException();
    }
}