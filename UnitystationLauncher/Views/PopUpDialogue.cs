﻿using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia.ViewModels.Commands;

namespace UnitystationLauncher.Views
{
    public class PopUpDialogue : Window
    {
        public event EventHandler<bool>? DialogResult;


        public PopUpDialogue()
        {
 
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void AYesCommand(object? sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult?.Invoke(this, true);
            Close();
        }

        public void ANoCommand(object? sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult?.Invoke(this, false);
            Close();
        }
    }
}
