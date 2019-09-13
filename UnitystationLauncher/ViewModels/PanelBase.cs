using System;
using System.Collections.Generic;
using System.Text;

namespace UnitystationLauncher.ViewModels
{
    public abstract class PanelBase : ViewModelBase
    {
        public abstract string Name { get; }
    }
}
