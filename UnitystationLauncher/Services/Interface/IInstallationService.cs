using System;
using System.Collections.Generic;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.Services.Interface;

public interface IInstallationService : IDisposable
{
    public IObservable<IReadOnlyList<Installation>> GetInstallations();

    public new void Dispose();
}