using System;

namespace UnitystationLauncher.Models.Enums;

[Flags]
public enum DumpFlags : byte
{
    None = 0,
    Types = 1,
    Members = 2,
    Inheritance = 4,

    All = Types | Members | Inheritance
}