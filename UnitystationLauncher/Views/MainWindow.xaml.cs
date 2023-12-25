using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Runtime.InteropServices;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher.Views;

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

        // TODO: Proper exception for these
        _titleBar = this.FindControl<DockPanel>("TitleBar") ?? throw new Exception();
        _contentControl = this.FindControl<Border>("ContentControl") ?? throw new Exception();

        SetupTitleBar();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    

    private void SetupTitleBar()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
        {
            ExtendClientAreaToDecorationsHint = false;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;

            Width = 800;
            Height = 555;

            _titleBar.IsVisible = false;
            Grid.SetRow(_contentControl, 0);
            Grid.SetRowSpan(_contentControl, 2);
            _contentControl.BorderThickness = new();
        }
        else
        {
            // TODO: Proper exception for these
            Button minimizeButton = this.FindControl<Button>("MinimizeButton") ?? throw new Exception();
            Button maximizeButton = this.FindControl<Button>("MaximizeButton") ?? throw new Exception();
            Button closeButton = this.FindControl<Button>("CloseButton") ?? throw new Exception();

            minimizeButton.Click += MinimizeWindow;
            maximizeButton.Click += ToggleMaximizeWindowState;
            closeButton.Click += CloseWindow;
        }
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

    private void MinimizeWindow(object? sender, RoutedEventArgs eventArgs)
    {
        WindowState = WindowState.Minimized;
    }

    private void ToggleMaximizeWindowState(object? sender, RoutedEventArgs eventArgs)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            _contentControl.BorderThickness = new(0.4, 0, 0.4, 0.4);
        }
        else if (WindowState == WindowState.Normal)
        {
            WindowState = WindowState.Maximized;
            _contentControl.BorderThickness = new();
        }
    }

    private void CloseWindow(object? sender, RoutedEventArgs eventArgs)
    {
        Close();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WindowStateProperty && change.NewValue is WindowState windowState)
        {
            PseudoClasses.Set(":maximised", windowState == WindowState.Maximized);
        }
    }
}
