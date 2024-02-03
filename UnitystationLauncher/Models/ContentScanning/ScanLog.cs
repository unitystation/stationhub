namespace UnitystationLauncher.Models.ContentScanning;

public class ScanLog
{
    // Not 100% sure we need this, as this could just be replaced with a bool.
    // However, I think it makes the code a bit easier to read to have a named thing so I did it anyways.
    public enum LogType
    {
        Info,
        Error
    }

    /// <summary>
    ///   Used to know which log we need to write this to
    /// </summary>
    public LogType Type { get; init; } = LogType.Info;

    /// <summary>
    ///   Log message to be written
    /// </summary>
    public string LogMessage { get; init; } = string.Empty;
}