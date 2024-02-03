using System;
using System.IO.Compression;
using System.Threading.Tasks;
using UnitystationLauncher.Models.ContentScanning;

namespace UnitystationLauncher.Services.Interface;

public interface ICodeScanService
{
    public Task<bool> OnScanAsync(ZipArchive archive, string targetDirectory, string goodFileVersion, Action<ScanLog> scanLog);
}