using System;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class PreferencesService : IPreferencesService, IDisposable
{
    private Preferences? _preferences;
    private IDisposable? _preferenceSub;
    private readonly string _preferencesFilePath;
    private readonly IEnvironmentService _environmentService;

    public PreferencesService(IEnvironmentService environmentService)
    {
        _environmentService = environmentService;
        _preferencesFilePath = Path.Combine(_environmentService.GetUserdataDirectory(), "prefs.json");
    }

    public Preferences GetPreferences()
    {
        if (_preferences != null)
        {
            return _preferences;
        }

        if (File.Exists(_preferencesFilePath))
        {
            string preferencesJson = File.ReadAllText(_preferencesFilePath);
            _preferences = JsonSerializer.Deserialize<Preferences>(preferencesJson);
        }

        _preferences ??= new();
        _preferenceSub?.Dispose();
        _preferenceSub = _preferences.Changed
            .Select(_ => Observable.FromAsync(SerializePreferencesAsync))
            .Concat()
            .Subscribe();

        if (string.IsNullOrWhiteSpace(_preferences.InstallationPath))
        {
            _preferences.InstallationPath = Path.Combine(_environmentService.GetUserdataDirectory(), "Installations");
        }

        return _preferences;
    }

    private async Task SerializePreferencesAsync()
    {
        await using FileStream file = File.Create(_preferencesFilePath);
        await JsonSerializer.SerializeAsync(file, _preferences, new JsonSerializerOptions
        { IgnoreReadOnlyProperties = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
    }

    public void Dispose()
    {
        _preferenceSub?.Dispose();
    }
}