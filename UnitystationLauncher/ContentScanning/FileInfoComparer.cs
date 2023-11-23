using System;
using System.Collections.Generic;
using System.IO;

namespace UnitystationLauncher.ContentScanning;

internal class FileInfoComparer : IEqualityComparer<FileInfo>
{
    public bool Equals(FileInfo? x, FileInfo? y)
    {
        if (x == null || y == null) return false;
        return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(FileInfo obj)
    {
        return obj.Name.GetHashCode();
    }
}