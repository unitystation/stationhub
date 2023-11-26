using System.ComponentModel;

namespace UnitystationLauncher.Models.Enums;

[DefaultValue(NotDownloaded)]
public enum DownloadState
{

    NotDownloaded,
    InProgress,
    Scanning,
    Installed,
    Failed
}