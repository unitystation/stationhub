using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.Services.Interface;

/// <summary>
///   Handles the users preferences for the launcher.
/// </summary>
public interface IPreferencesService
{
    /// <summary>
    ///   Returns the current preferences to the user. Any updates made to the returned object should be automatically saved.
    /// </summary>
    /// <returns>The users preferences.</returns>
    public Preferences GetPreferences();
}