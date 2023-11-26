using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services.Interface;

/// <summary>
///   Handles everything to do with game installations.
/// </summary>
public interface IInstallationService
{
    /// <summary>
    ///   Gets the current list of installations.
    /// </summary>
    /// <returns>A list of the known installations.</returns>
    public List<Installation> GetInstallations();

    /// <summary>
    ///   Gets a specific installation by fork name and version.
    /// </summary>
    /// <param name="forkName">Name of of the fork to check for</param>
    /// <param name="buildVersion">Version of the fork to check for</param>
    /// <returns>The installation if its found, `null` otherwise.</returns>
    public Installation? GetInstallation(string forkName, int buildVersion);

    /// <summary>
    ///   Gets a specific download by fork name and version.
    /// </summary>
    /// <param name="forkName">Name of of the fork to check for</param>
    /// <param name="buildVersion">Version of the fork to check for</param>
    /// <returns>The download if its found, `null` otherwise.</returns>
    public Download? GetInProgressDownload(string forkName, int buildVersion);

    /// <summary>
    ///   Starts the download for an installation.
    /// </summary>
    /// <param name="server">The server to get the download from</param>
    /// <returns>Download will be `null` if it was unsuccessful in starting, and the string will have the reason</returns>
    public Task<(Download?, string)> DownloadInstallationAsync(Server server);

    /// <summary>
    ///   Starts an installation so we can finally just play the game.
    /// </summary>
    /// <param name="installationId">ID of the installation to start</param>
    /// <param name="server">Server to connect to, either IP or domain</param>
    /// <param name="port">Port to use, requires server parameter</param>
    /// <returns>Status code for if it was successful, if unsuccessful the string will have the reason</returns>
    public (bool, string) StartInstallation(Guid installationId, string? server = null, short? port = null);

    /// <summary>
    ///   Deletes an installation 
    /// </summary>
    /// <param name="installationId">ID of the installation to delete</param>
    /// <returns>Status code for if it was successful, if unsuccessful the string will have the reason</returns>
    public (bool, string) DeleteInstallation(Guid installationId);

    /// <summary>
    ///   Cleans up old installs that should be safe to delete now
    /// </summary>
    /// <param name="isAutoRemoveAction">Is this cleanup request coming from an auto-remove action?</param>
    /// <returns>Status code for if it was successful, if unsuccessful the string will have the reason</returns>
    public (bool, string) CleanupOldVersions(bool isAutoRemoveAction);

    /// <summary>
    ///   Moves installations from the current installation base path to the new installation base path
    /// </summary>
    /// <param name="newBasePath">The new base path to move them to</param>
    /// <returns>true if the move was successful, false otherwise</returns>
    public bool MoveInstallations(string newBasePath);

    /// <summary>
    ///   Checks if the base path provided would be valid
    /// </summary>
    /// <param name="path">Path to check</param>
    /// <returns>Status code for if it is valid, if invalid the string will have the reason</returns>
    public (bool, string) IsValidInstallationBasePath(string path);
}