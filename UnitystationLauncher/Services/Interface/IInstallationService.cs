using System;
using System.Collections.Generic;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.Services.Interface;

public interface IInstallationService
{
    public IObservable<IReadOnlyList<Installation>> GetInstallations();
}