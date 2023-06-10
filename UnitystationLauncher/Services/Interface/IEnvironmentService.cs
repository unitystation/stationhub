using System.Diagnostics;
using UnitystationLauncher.Models.Enums;

namespace UnitystationLauncher.Services.Interface;

/// <summary>
///   The environment service should handle detecting the environment, and setting up environment specific configurations.
/// </summary>
public interface IEnvironmentService
{
    /// <summary>
    ///   Gets the current environment that the launcher is running on.
    /// </summary>
    /// <returns>The current environment that the launcher is running on.</returns>
    public CurrentEnvironment GetCurrentEnvironment();

    /// <summary>
    ///   Gets the userdata directory for the current environment.
    /// </summary>
    /// <returns>The userdata directory for the current environment.</returns>
    public string GetUserdataDirectory();

    /// <summary>
    ///   Checks if the update check for the launcher should be disabled for the current environment.
    /// </summary>
    /// <returns>True if the update check should be disabled, false otherwise.</returns>
    public bool ShouldDisableUpdateCheck();

    /// <summary>
    ///   Gets the process start info for the provided game executable.
    /// </summary>
    /// <param name="executable">The game executable path</param>
    /// <param name="arguments">The args to pass to the game</param>
    /// <returns>The ProcessStartInfo for the current environment, otherwise `null` if the environment is unhandled</returns>
    public ProcessStartInfo? GetGameProcessStartInfo(string executable, string arguments);
}