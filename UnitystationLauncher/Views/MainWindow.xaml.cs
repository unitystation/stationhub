using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
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
        private Path _maximizeIcon; //Update Icon
        private ToolTip _maximizeToolTip; //Update ToolTip 
        private DockPanel _titleBar; //For move WINDOW and hiding if user don't use Windows.
        private Border _contentcontrol;  //If user don't use Windows OS then Grid.Row update and Grid.RowSpan

        public MainWindow()
        {
            InitializeComponent();

            _titleBar = this.FindControl<DockPanel>("TitleBar"); 
            _contentcontrol = this.FindControl<Border>("ContentControl");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            {

                ExtendClientAreaToDecorationsHint = false;
                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;

                this.Width = 800;
                this.Height = 555;

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

                _maximizeIcon = this.FindControl<Path>("MaximizeIcon");
                _maximizeToolTip = this.FindControl<ToolTip>("MaximizeToolTip");

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
            else
            {
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
                    _maximizeIcon.Data = Avalonia.Media.Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                    _maximizeToolTip.Content = "Maximize";
                    break;

                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    _contentcontrol.BorderThickness = new Thickness();
                    _maximizeIcon.Data = Avalonia.Media.Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                    _maximizeToolTip.Content = "Restore Down";
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