using UnitystationLauncher.GameCommunicationPipe;

namespace UnitystationLauncher.Services;

public class GameCommunicationPipeService : IGameCommunicationPipeService
{

    private PipeHubBuildCommunication? _coolPipeHubBuildCommunication;

    public void Init()
    {
        PipeHubBuildCommunication data = new();
        _ = data.StartServerPipe();
        _coolPipeHubBuildCommunication = data;
    }
}