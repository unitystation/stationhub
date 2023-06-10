using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services.Interface;

public interface IServerService
{
    /// <summary>
    ///   Asynchronously gets the current list of servers from the Hub  
    /// </summary>
    /// <returns>A list of the servers currently in the hub, will be empty if something goes wrong contacting the hub.</returns>
    public Task<List<Server>> GetServersAsync();

    /// <summary>
    ///   Checks if the installation provided is in use by any of the servers via their fork name and version
    /// </summary>
    /// <param name="installation">Installation to check</param>
    /// <returns>true if at least one server is using this installation. false otherwise</returns>
    public bool IsInstallationInUse(Installation installation);
}