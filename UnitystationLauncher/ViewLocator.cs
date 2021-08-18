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
        public bool SupportsRecycling => false;

        public IControl Build(object data)
        {
            var viewName = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(viewName);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + viewName };
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}