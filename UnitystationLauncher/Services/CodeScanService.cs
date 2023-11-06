using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Serilog;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class CodeScanService : ICodeScanService
{
    private readonly IAssemblyChecker _IAssemblyChecker;
    private readonly IEnvironmentService _environmentService;
    private readonly IGoodFileService _iGoodFileService;


    private const string Managed = "Managed";
    private const string Plugins = "Plugins";
    private const string Unitystation_Data = "Unitystation_Data";





    public CodeScanService(IAssemblyChecker assemblyChecker, IEnvironmentService environmentService, IGoodFileService iGoodFileService)
    {
        _IAssemblyChecker = assemblyChecker;
        _environmentService = environmentService;
        _iGoodFileService = iGoodFileService;
    }



    class FileInfoComparer : IEqualityComparer<FileInfo>
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

    public async Task<bool> OnScan(ZipArchive archive, string targetDirectory, string goodFileVersion, Action<string> info, Action<string> errors)
    {
        // TODO: Enable extraction cancelling
        var root = new DirectoryInfo(_environmentService.GetUserdataDirectory());

        DirectoryInfo stagingDirectory = root.CreateSubdirectory("UnsafeBuildZipDirectory");
        DirectoryInfo processingDirectory = root.CreateSubdirectory("UnsafeBuildProcessing");
        DirectoryInfo? dataPath = null;
        archive.ExtractToDirectory(stagingDirectory.ToString(), true);
        try
        {
            DeleteContentsOfDirectory(processingDirectory);
            info.Invoke("Copying files");
            CopyFilesRecursively(stagingDirectory.ToString(), processingDirectory.ToString());

            info.Invoke("Cleaning out Dlls and Executables");
            DeleteFilesWithExtension(processingDirectory.ToString(), ".exe");
            DeleteFilesWithExtension(processingDirectory.ToString(), ".dll");
            DeleteFilesWithExtension(processingDirectory.ToString(), ".so");
            DeleteFilesWithExtension(processingDirectory.ToString(), ".dylib");
            DeleteFilesWithExtension(processingDirectory.ToString(), "", false);

            if (_environmentService.GetCurrentEnvironment() == CurrentEnvironment.MacOsStandalone)
            {
                DeleteFilesWithExtension(processingDirectory.ToString(), ".bundle", exceptionDirectory: Path.Combine(processingDirectory.ToString(), @"Contents/Resources/Data/StreamingAssets"));
            }



            DirectoryInfo stagingManaged = null;
            if (_environmentService.GetCurrentEnvironment() != CurrentEnvironment.MacOsStandalone)
            {
                // Get all files in the directory
                var directories = processingDirectory.GetDirectories();
                // Loop through each file
                foreach (var directorie in directories)
                {
                    if (directorie.Name.Contains("_Data"))
                    {
                        if (dataPath != null)
                        {
                            errors.Invoke("oh God 2 Datapaths Exiting!!!");
                            return false;
                        }

                        dataPath = directorie;
                    }
                }

                if (dataPath == null)
                {
                    errors.Invoke("oh God NO Datapath Exiting!!!");
                    return false;
                }
                stagingManaged = stagingDirectory.CreateSubdirectory(Path.Combine(dataPath.Name, Managed));
            }
            else
            {
                //MAC
                dataPath = new DirectoryInfo(Path.Combine(processingDirectory.ToString(), @"Contents/Resources/Data"));
                stagingManaged = stagingDirectory.CreateSubdirectory(Path.Combine(@"Contents/Resources/Data", Managed));
            }



            var dllDirectory = dataPath.CreateSubdirectory(Managed);

            CopyFilesRecursively(stagingManaged.ToString(), dllDirectory.ToString());

            var (goodFilePath, booly) = await _iGoodFileService.GetGoodFileVersion(goodFileVersion);

            if (booly == false)
            {
                DeleteContentsOfDirectory(processingDirectory);
                DeleteContentsOfDirectory(stagingDirectory);
                return false;
            }

            DirectoryInfo goodFileCopy = new DirectoryInfo(GetManagedOnOS(goodFilePath));

            info.Invoke("Proceeding to scan folder");
            if (ScanFolder(dllDirectory, goodFileCopy, info, errors) == false)
            {
                DeleteContentsOfDirectory(processingDirectory);
                DeleteContentsOfDirectory(stagingDirectory);
                return false;
            }



            CopyFilesRecursively(goodFilePath, processingDirectory.ToString());
            if (dataPath.Name != Unitystation_Data && _environmentService.GetCurrentEnvironment() != CurrentEnvironment.MacOsStandalone) //I know Cases and to file systems but F  
            {
                var oldPath = Path.Combine(processingDirectory.ToString(), Unitystation_Data);
                CopyFilesRecursively(oldPath, dataPath.ToString());
                Directory.Delete(oldPath, true);
            }


            switch (_environmentService.GetCurrentEnvironment())
            {
                case CurrentEnvironment.WindowsStandalone:
                    var exeRename = processingDirectory.GetFiles()
                        .FirstOrDefault(x => x.Extension == ".exe" && x.Name != "UnityCrashHandler64.exe"); //TODO OS


                    if (exeRename == null || exeRename.Directory == null)
                    {
                        errors.Invoke("no Executable found ");
                        DeleteContentsOfDirectory(processingDirectory);
                        DeleteContentsOfDirectory(stagingDirectory);
                        return false;
                    }
                    info.Invoke($"Found exeRename {exeRename}");
                    exeRename.MoveTo(Path.Combine(exeRename.Directory.ToString(), dataPath.Name.Replace("_Data", "") + ".exe"));
                    break;
                case CurrentEnvironment.LinuxFlatpak:
                case CurrentEnvironment.LinuxStandalone:
                    var ExecutableRename = processingDirectory.GetFiles()
                        .FirstOrDefault(x => x.Extension == "");

                    if (ExecutableRename == null || ExecutableRename.Directory == null)
                    {
                        errors.Invoke("no Executable found ");
                        DeleteContentsOfDirectory(processingDirectory);
                        DeleteContentsOfDirectory(stagingDirectory);
                        return false;
                    }
                    info.Invoke($"Found ExecutableRename {ExecutableRename}");
                    ExecutableRename.MoveTo(Path.Combine(ExecutableRename.Directory.ToString(), dataPath.Name.Replace("_Data", "") + ""));
                    break;
            }


            var targetDirectoryinfo = new DirectoryInfo(targetDirectory);
            if (targetDirectoryinfo.Exists)
            {
                DeleteContentsOfDirectory(targetDirectoryinfo);
            }

            CopyFilesRecursively(processingDirectory.ToString(), targetDirectory.ToString());
            DeleteContentsOfDirectory(processingDirectory);
            DeleteContentsOfDirectory(stagingDirectory);
        }
        catch (Exception e)
        {
            errors.Invoke(" an Error happened > " + e);
            DeleteContentsOfDirectory(processingDirectory);
            DeleteContentsOfDirectory(stagingDirectory);
            return false;
        }

        return true;
    }

    public string GetManagedOnOS(string GoodFiles)
    {
        var OS = _environmentService.GetCurrentEnvironment();
        switch (OS)
        {
            case CurrentEnvironment.WindowsStandalone:
                return Path.Combine(GoodFiles, Unitystation_Data, Managed);
            case CurrentEnvironment.LinuxFlatpak:
            case CurrentEnvironment.LinuxStandalone:
                return Path.Combine(GoodFiles, Unitystation_Data, Managed);
            case CurrentEnvironment.MacOsStandalone:
                return Path.Combine(GoodFiles, @"Contents/Resources/Data", Managed);
            default:
                throw new Exception($"Unable to determine OS Version {OS}");
        }
    }



    public bool ScanFolder(DirectoryInfo @unsafe, DirectoryInfo saveFiles, Action<string> info, Action<string> errors)
    {
        var goodFiles = saveFiles.GetFiles().Select(x => x.Name).ToList();

        info.Invoke("Provided files " + string.Join(",", goodFiles));

        CopyFilesRecursively(saveFiles.ToString(), @unsafe.ToString());

        var files = @unsafe.GetFiles();


        List<string> multiAssemblyReference = new List<string>();

        foreach (var file in files)
        {
            if (goodFiles.Contains(file.Name)) continue;
            multiAssemblyReference.Add(Path.GetFileNameWithoutExtension(file.Name));
        }


        foreach (var file in files)
        {
            if (goodFiles.Contains(file.Name)) continue;

            info.Invoke("Scanning " + file.Name);

            try
            {
                var listy = multiAssemblyReference.ToList();
                listy.Remove(Path.GetFileNameWithoutExtension(file.Name));
                if (_IAssemblyChecker.CheckAssembly(file, @unsafe, listy, info, errors) == false)
                {
                    errors.Invoke($"{file.Name} Failed scanning Cancelling");
                    return false;
                }
            }
            catch (Exception e)
            {
                errors.Invoke(" Failed scan due to error of " + e);
                return false;
            }
        }


        return true;
    }


    #region Utilities

    public static void DeleteContentsOfDirectory(DirectoryInfo directory)
    {
        // Delete files within the folder
        var files = directory.GetFiles();
        foreach (var file in files)
        {
            file.Delete();
        }

        // Delete subdirectories within the folder
        var subdirectories = directory.GetDirectories();
        foreach (var subdirectory in subdirectories)
        {
            subdirectory.Delete(true);
        }
    }

    static void DeleteFilesWithExtension(string directoryPath, string fileExtension, bool recursive = true, string? exceptionDirectory = null)
    {
        DirectoryInfo directory = new DirectoryInfo(directoryPath);

        // Check if the directory exists
        if (!directory.Exists)
        {
            Console.WriteLine("Directory not found: " + directoryPath);
            return;
        }

        // Process all the files in the directory
        FileInfo[] files = directory.GetFiles("*" + fileExtension);
        foreach (FileInfo file in files)
        {
            if (exceptionDirectory != null)
            {
                if (file.Directory?.ToString().Contains(exceptionDirectory) == true)
                {
                    continue;
                }
            }
            // Delete the file
            file.Delete();
            Console.WriteLine("Deleted file: " + file.FullName);
        }

        if (recursive)
        {
            // Recursively process subdirectories
            DirectoryInfo[] subdirectories = directory.GetDirectories();
            foreach (DirectoryInfo subdirectory in subdirectories)
            {
                DeleteFilesWithExtension(subdirectory.FullName, fileExtension, exceptionDirectory: exceptionDirectory);
            }
        }
    }

    static void CopyFilesRecursively(string sourceDirectory, string destinationDirectory)
    {
        try
        {
            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationDirectory);

            DirectoryInfo source = new DirectoryInfo(sourceDirectory);

            // Get the files from the source directory
            FileInfo[] files = source.GetFiles();

            foreach (FileInfo file in files)
            {
                // Get the destination file path by combining the destination directory with the file name
                string destinationFile = Path.Combine(destinationDirectory, file.Name);

                // Copy the file to the destination
                file.CopyTo(destinationFile, true);
            }

            // Get the directories from the source directory
            DirectoryInfo[] directories = source.GetDirectories();

            foreach (DirectoryInfo directory in directories)
            {
                // Get the destination directory path by combining the destination directory with the directory name
                string destinationSubdirectory = Path.Combine(destinationDirectory, directory.Name);

                // Copy the files from the subdirectory recursively
                CopyFilesRecursively(directory.FullName, destinationSubdirectory);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
        }
    }

    #endregion
}