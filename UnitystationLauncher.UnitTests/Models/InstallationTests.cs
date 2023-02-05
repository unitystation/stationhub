using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using UnitystationLauncher.Models;
using Xunit;

namespace UnitystationLauncher.UnitTests.Models;

public class InstallationTests
{
    [Fact]
    public void Constructor_ShouldHandleNullInputs()
    {
        Action act = () => _ = new Installation(null);
        act.Should().Throw<ArgumentNullException>();
    }

    #region Installation.FindExecutable
    [Fact]
    public void FindExecutable_ShouldHandleNullInputs()
    {
        Action act = () => Installation.FindExecutable(null);
        act.Should().NotThrow();
    }

    [Fact]
    public void FindExecutable_ShouldHandleInvalidInputs()
    {
        Action act = () => Installation.FindExecutable("<>|\"");
        act.Should().NotThrow();
    }

    [Fact]
    public void FindExecutable_ShouldHandleValidInputs()
    {
        string testDir = "./Test";
        if (!Directory.Exists(testDir))
        {
            Directory.CreateDirectory(testDir);
        }

        Action act = () => Installation.FindExecutable(testDir);
        act.Should().NotThrow();
    }
    #endregion

    #region Installation.Start
    [Fact]
    public void Start_ShouldHandleNullInputs()
    {
        Installation installation = new(Directory.GetCurrentDirectory());
        Action act = () => installation.Start(null, 0, null, null);
        act.Should().NotThrow();
    }

    [Fact]
    public void Start_ShouldHandleNoInputs()
    {
        Installation installation = new(Directory.GetCurrentDirectory());
        Action act = () => installation.Start();
        act.Should().NotThrow();
    }
    #endregion

    #region Installation.DeleteInstallation
    [Fact]
    public void DeleteInstallation_ShouldHandleValidInputs()
    {
        string deleteTestDirectory = Directory.GetCurrentDirectory() + "/DeleteInstallationTest23020422";
        if (!Directory.Exists(deleteTestDirectory))
        {
            Directory.CreateDirectory(deleteTestDirectory);
        }

        Installation deleteInstallation = new(deleteTestDirectory);
        Action act = () => deleteInstallation.DeleteInstallation();

        act.Should().NotThrow();

        // Wait a second for the delete request to go through
        Thread.Sleep(1000);

        Directory.Exists(deleteTestDirectory).Should().BeFalse();
    }
    #endregion

    #region Installation.GetForkName
    [Fact]
    public void GetForkName_ShouldHandleNullInputs()
    {
        Action act = () => Installation.GetForkName(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetForkName_ShouldHandleValidInputs()
    {
        string forkNameTestDirectory = "ForkNameTest23020422";
        string actual = Installation.GetForkName(forkNameTestDirectory);
        actual.Should().Be("ForkNameTest");
    }
    #endregion

    #region Installation.GetBuildVersion
    [Fact]
    public void GetBuildVersion_ShouldHandleNullInputs()
    {
        Action act = () => Installation.GetBuildVersion(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetBuildVersion_ShouldHandleValidInputs()
    {
        string buildVersionTestDirectory = "BuildVersionTest23020422";
        int actual = Installation.GetBuildVersion(buildVersionTestDirectory);
        actual.Should().Be(23020422);
    }
    #endregion

    #region Installation.MakeExecutableExecutable
    [Fact]
    public void MakeExecutableExecutable_ShouldHandleNullInputs()
    {
        Action act = () => Installation.MakeExecutableExecutable(null);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}