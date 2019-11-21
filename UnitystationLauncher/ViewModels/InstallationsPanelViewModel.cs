using ReactiveUI;
using System;
using System.IO;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using UnitystationLauncher.Models;
using Reactive.Bindings;
using Newtonsoft.Json;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Models;
using MessageBox.Avalonia.DTO;
using System.Reactive;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationsPanelViewModel : PanelBase
    {
        public override string Name => "Installations";
        public override IBitmap Icon => new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>()
            .Open(new Uri("avares://UnitystationLauncher/Assets/archiveicon.png")));
        private InstallationManager installationManager;

        private Installation? selectedInstallation;

        public InstallationsPanelViewModel(InstallationManager installationManager)
        {
            this.installationManager = installationManager;
            if (File.Exists("prefs.json"))
            {
                var data = File.ReadAllText("prefs.json");
                AutoRemove.Value = JsonConvert.DeserializeObject<Prefs>(data).AutoRemove;
            }
            else
            {
                var data = JsonConvert.SerializeObject(new Prefs { AutoRemove = true, Refresh = "" });
                File.WriteAllText("prefs.json", data);
                AutoRemove.Value = true;
            }
            installationManager.AutoRemove = AutoRemove.Value;
            CheckBoxClick = ReactiveUI.ReactiveCommand.Create(OnCheckBoxClick, null);
        }

        public IObservable<IReadOnlyList<Installation>> Installations => installationManager.Installations;
        public ReactiveProperty<bool> AutoRemove { get; } = new ReactiveProperty<bool>();
        public ReactiveCommand<Unit, Unit> CheckBoxClick { get; }
        public Installation? SelectedInstallation
        {
            get => selectedInstallation;
            set => this.RaiseAndSetIfChanged(ref selectedInstallation, value);
        }

        //This is a Reactive Command action as confirmation needs to happen with the
        //message box.
        async void OnCheckBoxClick()
        {
            if(AutoRemove.Value == true)
            {
                var msgBox = MessageBoxWindow.CreateCustomWindow(new MessageBoxCustomParams
                {
                    Style = MessageBox.Avalonia.Enums.Style.None,
                    Icon = MessageBox.Avalonia.Enums.Icon.None,
                    ContentHeader = "Are you sure?",
                    ContentMessage = "This will remove older installations from disk. Proceed?",
                    ButtonDefinitions = new[] { new ButtonDefinition { Name = "Cancel" }, new ButtonDefinition { Name = "Confirm" } }
                });

                var response = await msgBox.Show();
                if (response.Equals("Confirm"))
                {
                    SaveChoice();
                } else
                {
                    AutoRemove.Value = false;
                }
            } else
            {
                SaveChoice();
            }
        }

        void SaveChoice()
        {
            var data = "";
            if (File.Exists("prefs.json"))
            {
                data = File.ReadAllText("prefs.json");
                var prefs = JsonConvert.DeserializeObject<Prefs>(data);
                prefs.AutoRemove = AutoRemove.Value;
                data = JsonConvert.SerializeObject(prefs);
            }
            else
            {
                data = JsonConvert.SerializeObject(new Prefs { AutoRemove = AutoRemove.Value, Refresh = "" });
            }
            File.WriteAllText("prefs.json", data);
            installationManager.AutoRemove = AutoRemove.Value;
        }
    }
}
