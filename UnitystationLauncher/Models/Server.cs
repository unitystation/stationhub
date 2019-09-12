namespace UnitystationLauncher.Models
{
    public class Server
    {
        public string? ServerName { get; set;}
        public string? ForkName { get; set;}
        public int? BuildVersion { get; set;}
        public string? CurrentMap { get; set;}
        public string? GameMode { get; set;}
        public string? IngameTime { get; set;}
        public int? PlayerCount { get; set;}
        public string? ServerIP { get; set;}
        public int? ServerPort { get; set;}
        public string? WinDownload { get; set;}
        public string? OSXDownload { get; set;}
        public string? LinuxDownload { get; set;}
    }
}