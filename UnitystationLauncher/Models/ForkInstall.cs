using System;
using System.Collections.Generic;
using System.Linq;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Models;

public class ForkInstall
{
    public ForkInstall(Download? download, Installation? installation, IReadOnlyList<Server> servers)
    {
        Download = download;
        Installation = installation;
        Servers = servers;
    }

    public (string, int) ForkAndVersion =>
        Download?.ForkAndVersion ??
        Installation?.ForkAndVersion ??
        Servers.FirstOrDefault()?.ForkAndVersion ??
        throw new ArgumentNullException();

    public Download? Download { get; }
    public Installation? Installation { get; }
    public IReadOnlyList<Server> Servers { get; }
}