using System.Collections.Generic;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository;

public static class MockServerService
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
    
    public static IServerService RandomServersRange(int min, int max)
    {
        Mock<IServerService> mockServerService = new();

        mockServerService.Setup(x => x.GetServersAsync())
            .ReturnsAsync(GetRandomServers(min, max));

        return mockServerService.Object;
    }
    
    public static IServerService ThrowsException()
    {
        Mock<IServerService> mockServerService = new();

        mockServerService.Setup(x => x.GetServersAsync())
            .Throws<Exception>();

        return mockServerService.Object;
    }

    private static List<Server> GetRandomServers(int min, int max)
    {
        List<Server> servers = new();

        int numberOfServers = _random.Next(min, max);
        for (int i = 0; i < numberOfServers; i++)
        {
            servers.Add(new(_serverNames[_random.Next(_serverNames.Count)], _random.Next(0, 10), "127.0.0.1", _random.Next(1, 65535)));
        }

        return servers;
    }
}