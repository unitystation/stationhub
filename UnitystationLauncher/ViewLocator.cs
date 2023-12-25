// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher
{
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object? data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "data is null");
            }
            
            string viewName = data.GetType().FullName!.Replace("ViewModel", "View");
            Type? type = Type.GetType(viewName);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + viewName };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}