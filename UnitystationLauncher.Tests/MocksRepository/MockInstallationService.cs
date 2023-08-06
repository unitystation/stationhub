using UnitystationLauncher.Models;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository;

public static class MockInstallationService
{
    public static IInstallationService NoActiveDownloads()
    {
        Mock<IInstallationService> mockInstallationService = new();
        mockInstallationService.Setup(x => x.GetInProgressDownload(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(null as Download);
        
        return mockInstallationService.Object;
    }
}