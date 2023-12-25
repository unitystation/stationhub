using System.Collections.Generic;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.Enums;

namespace UnitystationLauncher.Infrastructure;

public static class MessageBoxBuilder
{
    public static IMsBox<string> CreateMessageBox(MessageBoxButtons buttonLayout, string header, string message)
    {
        IMsBox<string> msgBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            SystemDecorations = Avalonia.Controls.SystemDecorations.BorderOnly,
            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen,
            ContentHeader = header,
            ContentMessage = message,
            ButtonDefinitions = buttonLayout switch
            {
                MessageBoxButtons.Ok => new()
                {
                    new() { Name = MessageBoxResults.Ok, IsDefault = true }
                },
                MessageBoxButtons.OkCancel => new()
                {
                    new() { Name = MessageBoxResults.Ok, IsDefault = true },
                    new() { Name = MessageBoxResults.Cancel }
                },
                MessageBoxButtons.YesNo => new()
                {
                    new() { Name = MessageBoxResults.Yes },
                    new() { Name = MessageBoxResults.No }
                },
                MessageBoxButtons.YesNoCancel => new()
                {
                    new() { Name = MessageBoxResults.Yes },
                    new() { Name = MessageBoxResults.No },
                    new() { Name = MessageBoxResults.Cancel }
                },
                _ => new List<ButtonDefinition>()
            }
        });

        return msgBox;
    }
}