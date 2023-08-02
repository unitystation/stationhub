using Moq;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository;

public static class MockPingService
{
    public static IPingService GetStaticPingTime(int pingTime)
    {
        Mock<IPingService> mock = new();
        mock.Setup(x => x.GetPing(It.IsAny<Server>())).Returns(() => $"{pingTime}ms");
        return mock.Object;
    }
}