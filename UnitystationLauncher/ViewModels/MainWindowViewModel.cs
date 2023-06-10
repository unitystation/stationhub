using System;
using ReactiveUI;
using System.Reactive.Linq;
using Serilog;
using System.Threading;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Media;
using UnitystationLauncher.Services;

namespace UnitystationLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _content;
        private Geometry _maximizeIcon;
        private string _maximizeToolTip;

        public Geometry MaximizeIcon
        {
            get => _maximizeIcon;
            set => this.RaiseAndSetIfChanged(ref _maximizeIcon, value);
        }

        public string MaximizeToolTip
        {
            get => _maximizeToolTip;
            set => this.RaiseAndSetIfChanged(ref _maximizeToolTip, value);
        }

        public MainWindowViewModel(LauncherViewModel launcherVm)
        {
            _maximizeIcon = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
            _maximizeToolTip = "Maximize";
            Content = _content = launcherVm;
        }

        private void Maximize()
        {
            if (MaximizeToolTip == "Maximize")
            {
                MaximizeIcon =
                    Geometry.Parse(
                        "M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                MaximizeToolTip = "Restore Down";
            }
            else if (MaximizeToolTip == "Restore Down")
            {
                MaximizeIcon = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                MaximizeToolTip = "Maximize";
            }
        }

        public ViewModelBase Content
        {
            get => _content;
            private set
            {
                this.RaiseAndSetIfChanged(ref _content, value);
                ContentChanged();
            }
        }

        private void ContentChanged()
        {
            SubscribeToVm(Content switch
            {
                LauncherViewModel launcherVm => Observable.Merge(
                    launcherVm.ShowUpdateView.Select(vm => (ViewModelBase)vm)),

                HubUpdateViewModel hubUpdateViewModel => Observable.Merge(
                    hubUpdateViewModel.Skip,
                    hubUpdateViewModel.Ignore),

                _ => throw new ArgumentException($"ViewModel type is not handled and will never be able to change")
            });
        }

        private void SubscribeToVm(IObservable<ViewModelBase?> observable)
        {
            observable
                .SkipWhile(vm => vm == null)
                .Select(vm => vm!)
                .Take(1)
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(vm => Content = vm);
        }

        public override void Refresh()
        {
            // Do nothing
        }
    }
}
