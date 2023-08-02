using System.Threading.Tasks;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services.Interface;

public interface IPingService
{
    public Task<string> GetPing(Server server);
}