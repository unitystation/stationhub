using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class CodeScanService : ICodeScanService
{
    private readonly IAssemblyTypeCheckerService _assemblyTypeCheckerService;
    private readonly IEnvironmentService _environmentService;
    private readonly ICodeScanConfigService _codeScanConfigService;
    private readonly IPreferencesService _preferencesService;



    public CodeScanService(IAssemblyTypeCheckerService assemblyTypeCheckerService, IEnvironmentService environmentService, ICodeScanConfigService codeScanConfigService,
        IPreferencesService preferencesService)
    {
        _assemblyTypeCheckerService = assemblyTypeCheckerService;
        _environmentService = environmentService;
        _codeScanConfigService = codeScanConfigService;
        _preferencesService = preferencesService;
    }

    public async Task<bool> OnScanAsync(ZipArchive archive, string targetDirectory, string goodFileVersion, Action<string> info, Action<string> errors)
    {
        // TODO: Enable extraction cancelling
        DirectoryInfo root = new(_preferencesService.GetPreferences().InstallationPath);

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
                DeleteFilesWithExtension(processingDirectory.ToString(), ".bundle", exceptionDirectory: Path.Combine(processingDirectory.ToString(), @"Unitystation.app/Contents/Resources/Data/StreamingAssets"));
            }



            DirectoryInfo? stagingManaged = null;
            if (_environmentService.GetCurrentEnvironment() != CurrentEnvironment.MacOsStandalone)
            {
                // Get all files in the directory
                DirectoryInfo[] directories = processingDirectory.GetDirectories();
                // Loop through each file
                foreach (DirectoryInfo directorie in directories)
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
                stagingManaged = stagingDirectory.CreateSubdirectory(Path.Combine(dataPath.Name, FolderNames.Managed));
            }
            else
            {
                //MAC
                dataPath = new(Path.Combine(processingDirectory.ToString(), @"Unitystation.app/Contents/Resources/Data"));
                stagingManaged = stagingDirectory.CreateSubdirectory(Path.Combine(@"Unitystation.app/Contents/Resources/Data", FolderNames.Managed));
            }



            DirectoryInfo dllDirectory = dataPath.CreateSubdirectory(FolderNames.Managed);

            CopyFilesRecursively(stagingManaged.ToString(), dllDirectory.ToString());

            (string goodFilePath, bool booly) = await _codeScanConfigService.GetGoodFileVersionAsync(goodFileVersion);

            if (booly == false)
            {
                DeleteContentsOfDirectory(processingDirectory);
                DeleteContentsOfDirectory(stagingDirectory);
                return false;
            }

            DirectoryInfo goodFileCopy = new(GetManagedOnOS(goodFilePath));

            info.Invoke("Proceeding to scan folder");
            if (await ScanFolderAsync(dllDirectory, goodFileCopy, info, errors) == false)
            {
                try
                {
                    DeleteContentsOfDirectory(processingDirectory);
                    DeleteContentsOfDirectory(stagingDirectory);
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
                return false;
            }



            CopyFilesRecursively(goodFilePath, processingDirectory.ToString());
            if (dataPath.Name != FolderNames.UnitystationData && _environmentService.GetCurrentEnvironment() != CurrentEnvironment.MacOsStandalone) //I know Cases and to file systems but F  
            {
                string oldPath = Path.Combine(processingDirectory.ToString(), FolderNames.UnitystationData);
                CopyFilesRecursively(oldPath, dataPath.ToString());
                Directory.Delete(oldPath, true);
            }


            switch (_environmentService.GetCurrentEnvironment())
            {
                case CurrentEnvironment.WindowsStandalone:
                    FileInfo? exeRename = processingDirectory.GetFiles()
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
                    FileInfo? executableRename = processingDirectory.GetFiles()
                        .FirstOrDefault(x => x.Extension == "");

                    if (executableRename == null || executableRename.Directory == null)
                    {
                        errors.Invoke("no Executable found ");
                        DeleteContentsOfDirectory(processingDirectory);
                        DeleteContentsOfDirectory(stagingDirectory);
                        return false;
                    }
                    info.Invoke($"Found ExecutableRename {executableRename}");
                    executableRename.MoveTo(Path.Combine(executableRename.Directory.ToString(), dataPath.Name.Replace("_Data", "") + ""));
                    break;
            }


            DirectoryInfo targetDirectoryInfo = new(targetDirectory);
            if (targetDirectoryInfo.Exists)
            {
                DeleteContentsOfDirectory(targetDirectoryInfo);
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

    private string GetManagedOnOS(string goodFiles)
    {
        CurrentEnvironment os = _environmentService.GetCurrentEnvironment();
        switch (os)
        {
            case CurrentEnvironment.WindowsStandalone:
                return Path.Combine(goodFiles, FolderNames.UnitystationData, FolderNames.Managed);
            case CurrentEnvironment.LinuxFlatpak:
            case CurrentEnvironment.LinuxStandalone:
                return Path.Combine(goodFiles, FolderNames.UnitystationData, FolderNames.Managed);
            case CurrentEnvironment.MacOsStandalone:
                return Path.Combine(goodFiles, @"Unitystation.app/Contents/Resources/Data", FolderNames.Managed);
            default:
                throw new($"Unable to determine OS Version {os}");
        }
    }



    private async Task<bool> ScanFolderAsync(DirectoryInfo @unsafe, DirectoryInfo saveFiles, Action<string> info, Action<string> errors)
    {
        List<string> goodFiles = saveFiles.GetFiles().Select(x => x.Name).ToList();

        info.Invoke("Provided files " + string.Join(",", goodFiles));

        CopyFilesRecursively(saveFiles.ToString(), @unsafe.ToString());

        FileInfo[] files = @unsafe.GetFiles();


        List<string> multiAssemblyReference = new();

        foreach (FileInfo file in files)
        {
            if (goodFiles.Contains(file.Name)) continue;
            multiAssemblyReference.Add(Path.GetFileNameWithoutExtension(file.Name));
        }


        foreach (FileInfo file in files)
        {
            if (goodFiles.Contains(file.Name)) continue;

            info.Invoke("Scanning " + file.Name);

            try
            {
                List<string> listy = multiAssemblyReference.ToList();
                listy.Remove(Path.GetFileNameWithoutExtension(file.Name));
                if (await _assemblyTypeCheckerService.CheckAssemblyTypesAsync(file, @unsafe, listy, info, errors) == false)
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
        FileInfo[] files = directory.GetFiles();
        foreach (FileInfo file in files)
        {
            file.Delete();
        }

        // Delete subdirectories within the folder
        DirectoryInfo[] subdirectories = directory.GetDirectories();
        foreach (DirectoryInfo subdirectory in subdirectories)
        {
            subdirectory.Delete(true);
        }
    }

    static void DeleteFilesWithExtension(string directoryPath, string fileExtension, bool recursive = true, string? exceptionDirectory = null)
    {
        DirectoryInfo directory = new(directoryPath);

        // Check if the directory exists
        if (!directory.Exists)
        {
            Log.Error("Directory not found: " + directoryPath);
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
            Log.Debug("Deleted file: " + file.FullName);
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

            DirectoryInfo source = new(sourceDirectory);

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