namespace UnitystationLauncher.Services.Interface;

public interface IEnvironmentService
{
    public string GetUserdataDirectory();
    public bool IsRunningOnWindows();
    public bool ShouldDisableUpdateCheck();
}