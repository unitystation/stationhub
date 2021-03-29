using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Runtime.InteropServices;

namespace UnitystationLauncher.Views
{
    public class MainWindow : Window
    {
        private Button _minimiseButton; //CustomButton Window
        private Button _closeButton; //CustomButton Window
        private Button _maximizeeButton; //CustomButton Window
        /// <summary>
        /// This is used for moving the window when the titlebar is grabbed, also for disabling on non-windows OSs.
        /// </summary>
        private DockPanel _titleBar;
        /// <summary>
        /// If user don't use Windows OS then Grid.Row update and Grid.RowSpan
        /// </summary>
        private Border _contentcontrol;

        public MainWindow()
        {
            InitializeComponent();

            _titleBar = this.FindControl<DockPanel>("TitleBar");
            _contentcontrol = this.FindControl<Border>("ContentControl");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            {

                ExtendClientAreaToDecorationsHint = false;
                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;

                Width = 800;
                Height = 555;

                _titleBar.IsVisible = false;
                Grid.SetRow(_contentcontrol, 0);
                Grid.SetRowSpan(_contentcontrol, 2);
                _contentcontrol.BorderThickness = new Thickness();
            }
            else
            {
                _minimiseButton = this.FindControl<Button>("MinimiseButton");
                _maximizeeButton = this.FindControl<Button>("MaximizeeButton");
                _closeButton = this.FindControl<Button>("CloseButton");

                _minimiseButton.Click += (sender, ee) => { WindowState = WindowState.Minimized; };

                _maximizeeButton.Click += (sender, ee) => { ToggleWindowState(); };

                _closeButton.Click += (sender, ee) => { Close(); };

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
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    _contentcontrol.BorderThickness = new Thickness(0.4, 0, 0.4, 0.4);
                    break;

                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    _contentcontrol.BorderThickness = new Thickness();
                    break;
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