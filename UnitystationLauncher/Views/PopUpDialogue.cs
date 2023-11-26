using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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
