using System;
using System.IO.Compression;

namespace UnitystationLauncher.Services.Interface;

public interface ICodeScanService
{
    public bool OnScan(ZipArchive archive, string targetDirectory, string goodFileVersion, Action<string> info, Action<string> errors);
}