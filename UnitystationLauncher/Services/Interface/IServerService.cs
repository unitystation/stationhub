using System;
using System.Collections.Generic;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services.Interface;

public interface IServerService
{
    public IObservable<IReadOnlyList<Server>> GetServers();
}