using System;
using System.Collections.Generic;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.Services.Interface;

public interface IStateService
{
    public IObservable<IReadOnlyDictionary<(string ForkName, int BuildVersion), ForkInstall>> GetState();
}