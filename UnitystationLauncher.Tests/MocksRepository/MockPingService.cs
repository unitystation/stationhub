using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository;

public static class MockPingService
{
    public static IPingService StaticPingTime(int pingTime)
    {
        Mock<IPingService> mock = new();
        mock.Setup(x => x.GetPingAsync(It.IsAny<Server>()))
            .ReturnsAsync($"{pingTime}ms");
        return mock.Object;
    }

    public static IPingService NullPingTime()
    {
        Mock<IPingService> mock = new();
        mock.Setup(x => x.GetPingAsync(It.IsAny<Server>()))
            !.ReturnsAsync(null as string);
        return mock.Object;
    }

    public static IPingService ThrowsException()
    {
        Mock<IPingService> mock = new();
        mock.Setup(x => x.GetPingAsync(It.IsAny<Server>()))
            .Throws<Exception>();
        return mock.Object;
    }
}