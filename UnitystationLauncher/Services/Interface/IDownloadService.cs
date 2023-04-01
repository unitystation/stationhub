using System.Threading.Tasks;
using Avalonia.Collections;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services.Interface;

/// <summary>
///   Handles downloading builds for servers.
/// </summary>
public interface IDownloadService
{
    /// <summary>
    ///   Gets the list of current downloads.
    /// </summary>
    /// <returns>A list of the current downloads.</returns>
    public IAvaloniaReadOnlyList<Download> GetDownloads();

    /// <summary>
    ///   Downloads the build for a server.
    /// </summary>
    /// <param name="server">The server to download a build for.</param>
    public Task DownloadAsync(Server server);

    /// <summary>
    ///   Checks if the servers build is able to be downloaded.
    /// </summary>
    /// <param name="server">The server to check.</param>
    /// <returns>True if it can be downloaded, false otherwise.</returns>
    public bool CanDownload(Server server);
}