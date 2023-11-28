using System.Collections.Generic;
using System.Threading.Tasks;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.InstallationService;

public class MockNoActiveDownloads : IInstallationService
{
    public List<Installation> GetInstallations()
    {
        throw new NotImplementedException();
    }

    public Installation? GetInstallation(string forkName, int buildVersion)
    {
        return null;
    }

    public Download? GetInProgressDownload(string forkName, int buildVersion)
    {
        return null;
    }

    public Task<(Download?, string)> DownloadInstallationAsync(Server server)
    {
        throw new NotImplementedException();
    }

    public (bool, string) StartInstallation(Guid installationId, string? server = null, short? port = null)
    {
        throw new NotImplementedException();
    }

    public (bool, string) DeleteInstallation(Guid installationId)
    {
        throw new NotImplementedException();
    }

    public (bool, string) CleanupOldVersions(bool isAutoRemoveAction)
    {
        throw new NotImplementedException();
    }

    public bool MoveInstallations(string newBasePath)
    {
        throw new NotImplementedException();
    }

    public (bool, string) IsValidInstallationBasePath(string path)
    {
        throw new NotImplementedException();
    }
}