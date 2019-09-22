using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationsPanelViewModel : PanelBase
    {
        public override string Name => "Installations";

        private Installation[] installations;

        private Installation? selectedInstallation;

        public InstallationsPanelViewModel()
        {
            Config.InstallationChanges.Subscribe(u =>
                {
                    if (Directory.Exists(Config.InstallationsPath))
                    {
                        Installations = Directory.EnumerateDirectories(Config.InstallationsPath)
                            .Select(d => new Installation(Path.GetFileName(d)))
                            .ToArray();
                    }
                });
        }

        public Installation[] Installations
        {
            get => installations;
            set => this.RaiseAndSetIfChanged(ref installations, value);
        }

        public Installation? SelectedInstallation
        {
            get => selectedInstallation;
            set => this.RaiseAndSetIfChanged(ref selectedInstallation, value);
        }
    }
}
