using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using UnitystationLauncher.Models;
using Xunit;

namespace UnitystationLauncher.UnitTests.Models;

public class InstallationTests
{
    private readonly Installation _installation;

    public InstallationTests()
    {
        // Should take place in the test execution directory
        _installation = new(Directory.GetCurrentDirectory());
    }

    #region Installation.FindExecutable
    [Fact]
    public void FindExecutable_ShouldHandleNullInputs()
    {
        Action act = () => Installation.FindExecutable(null);
        act.Should().NotThrow();
    }

    // Check invalid directory paths

    // Check valid directory paths

    // Not sure how to check if it actually finds it, but check that too
    #endregion

    #region Installation.Start
    [Fact]
    public void Start_ShouldHandleNullInputs()
    {
        Action act = () => _installation.Start(null, 0, null, null);
        act.Should().NotThrow();

        // Move to its own test
        act = () => _installation.Start();
        act.Should().NotThrow();
    }

    // Not sure how to check if it actually starts, but check that too
    #endregion

    #region Installation.DeleteAsync
    [Fact]
    public async Task DeleteAsync_ShouldHandleNullInputs()
    {
        // placeholder to fail test
        true.Should().BeFalse();
    }

    #endregion

    #region Installation.DeleteInstallation
    [Fact]
    public void DeleteInstallation_ShouldHandleNullInputs()
    {
        // placeholder to fail test
        true.Should().BeFalse();
    }

    #endregion

    #region Installation.DeleteFolder
    [Fact]
    public void DeleteFolder_ShouldHandleNullInputs()
    {
        // placeholder to fail test
        true.Should().BeFalse();
    }


    #endregion

    #region Installation.GetForkName
    [Fact]
    public void GetForkName_ShouldHandleNullInputs()
    {
        // placeholder to fail test
        true.Should().BeFalse();
    }

    #endregion

    #region Installation.GetBuildVersion
    [Fact]
    public void GetBuildVersion_ShouldHandleNullInputs()
    {
        // placeholder to fail test
        true.Should().BeFalse();
    }

    #endregion

    #region Installation.MakeExecutableExecutable
    [Fact]
    public void MakeExecutableExecutable_ShouldHandleNullInputs()
    {
        // placeholder to fail test
        true.Should().BeFalse();
    }

    #endregion
}