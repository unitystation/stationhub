using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Runtime.InteropServices;

namespace UnitystationLauncher.Views
{
    public class MainWindow : Window
    {
        /// <summary>
        /// This is used for moving the window when the titlebar is grabbed, also for disabling on non-windows OSs.
        /// </summary>
        private readonly DockPanel _titleBar;
        /// <summary>
        /// If user don't use Windows OS then Grid.Row update and Grid.RowSpan
        /// </summary>
        private readonly Border _contentControl;

        public MainWindow()
        {
            InitializeComponent();

            _titleBar = this.FindControl<DockPanel>("TitleBar");
            _contentControl = this.FindControl<Border>("ContentControl");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            {

                ExtendClientAreaToDecorationsHint = false;
                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;

                Width = 800;
                Height = 555;

                _titleBar.IsVisible = false;
                Grid.SetRow(_contentControl, 0);
                Grid.SetRowSpan(_contentControl, 2);
                _contentControl.BorderThickness = new Thickness();
            }
            else
            {
                var minimizeButton = this.FindControl<Button>("MinimizeButton");
                var maximizeButton = this.FindControl<Button>("MaximizeButton");
                var closeButton = this.FindControl<Button>("CloseButton");

                minimizeButton.Click += (sender, ee) => { WindowState = WindowState.Minimized; };

                maximizeButton.Click += (sender, ee) => { ToggleWindowState(); };

                closeButton.Click += (sender, ee) => { Close(); };

            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (_titleBar.IsPointerOver)
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    BeginMoveDrag(e);
                }
            }
            base.OnPointerPressed(e);
        }

        private void ToggleWindowState()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                _contentControl.BorderThickness = new Thickness(0.4, 0, 0.4, 0.4);
            }
            else if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                _contentControl.BorderThickness = new Thickness();
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == WindowStateProperty)
            {
                PseudoClasses.Set(":maximised", change.NewValue.HasValue && change.NewValue.GetValueOrDefault<WindowState>() == WindowState.Maximized);
            }
        }
    }
}