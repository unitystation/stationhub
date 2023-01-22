using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using FluentAssertions;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services;
using Xunit;

namespace UnitystationLauncher.UnitTests.Services;

public class AuthServiceTests : IDisposable
{
    private AuthService _authService;

    public AuthServiceTests()
    {
        // Need to clear this for the tests to run, otherwise it will try to read from here instead of using the mocked values
        string settingsPath = Path.Combine(Config.RootFolder, "settings.json");
        if (File.Exists(settingsPath))
        {
            File.Move(settingsPath, $"{settingsPath}.backup");
        }

        _authService = new AuthService(MocksRepository.MockHttpClient(), MocksRepository.MockFirebaseAuthProvider());
    }

    public void Dispose()
    {
        // Put the settings back once the tests complete
        string settingsPath = Path.Combine(Config.RootFolder, "settings.json");
        if (File.Exists($"{settingsPath}.backup"))
        {
            File.Move($"{settingsPath}.backup", settingsPath);
        }
    }

    #region AuthService.CreateAccountAsync

    [Fact]
    public void CreateAccountAsync_ShouldHandleNullValues()
    {
        Func<Task> act = async () => await _authService.CreateAccountAsync(null, null, null);
        act.Should().NotThrowAsync();
    }

    [Fact]
    public void CreateAccountAsync_ShouldHandleInvalidEmails()
    {
        Func<Task> act = async () => await _authService.CreateAccountAsync(TestConsts.DISPLAY_NAME, TestConsts.INVALID_EMAIL, TestConsts.PASSWORD);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async void CreateAccountAsync_ShouldCreateValidAccounts()
    {
        FirebaseAuthLink actual = await _authService.CreateAccountAsync(TestConsts.DISPLAY_NAME, TestConsts.VALID_EMAIL, TestConsts.PASSWORD);
        actual.FirebaseToken.Should().Be(TestConsts.VALID_FIREBASE_TOKEN);
        actual.User.DisplayName.Should().Be(TestConsts.DISPLAY_NAME);
    }

    #endregion
}