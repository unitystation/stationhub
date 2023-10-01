using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels;

public class SecurityPanelViewModel : PanelBase
{
    public override string Name => "Security";
    public override bool IsEnabled => false;

    private readonly IAssemblyChecker _IAssemblyChecker;

    public SecurityPanelViewModel(IAssemblyChecker assemblyChecker)
    {
        _IAssemblyChecker = assemblyChecker;
    }


    public override void Refresh()
    {
        //what does this do?!?
    }

    //TODO 
    //Automatic start-up pipe
    //Zip bomb prevention


    //TODO
    // in-memory virtual file system

    //UnityPlayer.dll?? look in to
    //Unitystation.exe
    

    // if (sandboxConfig.WhitelistedAssembliesDEBUG.Contains(
    //(type.ResolutionScope as AssemblyTypeChecker.MResScopeAssembly).Name))

    // if (string.Equals(fileName, assemblyName.Name, StringComparison.OrdinalIgnoreCase))  Potential security+

    //Check the multi-assembly stuff, make sure you can't do any naughty with name matching and stuff
    //tool to Make a PR Auto to add Two white lists, if PR violates White list

    //TODO Double check added stuff
    // UnityEngine.Events
    // UnityEngine.Events.UnityAction TODO Looking 
    //"UnityEngine.UI":{Dropdown"
    // Collider2D
    // LayerMask
    // Physics2D
    // Mathf???
    // UnityEngine.Transform???
    // Random???
    // Time
    // UnityEvent
    // UnityEngine.Plane?
    // UnityEngine.Rect?
    // Ray?
    // Texture2D
    //Manually set 
    //Texture2D
    //ParticleSystem
    //Input
    //GUIUtility
    //GUI?
    //ImageConversion 
    //Application
    //UnityEngine.Resources
    //Addressables


    //TODO Patch
    //UnityEngine.Events.UnityEvent, GetValidMethodInfo


    public void OnUpdateSafe()
    {
        DirectoryInfo directory = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);

        DirectoryInfo stagingDirectory = directory.CreateSubdirectory("staging");

        stagingDirectory = stagingDirectory.CreateSubdirectory("unitystation_Data");
        stagingDirectory = stagingDirectory.CreateSubdirectory("Managed");

        DirectoryInfo goodDirectory = directory.CreateSubdirectory("GoodFiles");
        goodDirectory = goodDirectory.CreateSubdirectory("VDev");
        goodDirectory = goodDirectory.CreateSubdirectory("Managed");


        DirectoryInfo sourceDirInfo = stagingDirectory;
        DirectoryInfo targetDirInfo = goodDirectory;

        // Get the list of files that are present in both directories
        FileInfo[] sourceFiles = sourceDirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        FileInfo[] targetFiles = targetDirInfo.GetFiles("*.*", SearchOption.AllDirectories);

        var commonFiles = sourceFiles.Intersect(targetFiles, new FileInfoComparer());

        // Do something with the common files
        foreach (FileInfo file in commonFiles)
        {
            var sourceFile = sourceDirInfo.GetFiles().FirstOrDefault(x => x.Name == file.Name);
            if (sourceFile == null)
            {
                throw new FileNotFoundException("Common files don't match somehow?!");
            }

            var targetFile = targetDirInfo.GetFiles().FirstOrDefault(x => x.Name == file.Name);
            if (targetFile == null)
            {
                throw new FileNotFoundException("Common files don't match somehow?!");
            }

            File.Copy(sourceFile.FullName,
                targetFile.FullName, true);
            Console.WriteLine($"Common file: {file.FullName}");
        }

        Console.WriteLine("Common files listed.");
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

    public void OnScan()
    {
        Action<string> info = new Action<string>((string log) => { });
        Action<string> errors = new Action<string>((string log) => { });

        // Create a DirectoryInfo object for the directory
        DirectoryInfo directory = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);

        DirectoryInfo stagingDirectory = directory.CreateSubdirectory("staging"); //TODO temp
        DirectoryInfo processingDirectory = directory.CreateSubdirectory("processing");
        DirectoryInfo? dataPath = null;

        try
        {
            DeleteContentsOfDirectory(processingDirectory);
            info.Invoke("Copying files");
            CopyFilesRecursively(stagingDirectory.ToString(), processingDirectory.ToString());

            info.Invoke("Cleaning out Dlls and Executables");
            DeleteFilesWithExtension(processingDirectory.ToString(), "exe");
            DeleteFilesWithExtension(processingDirectory.ToString(), "dll");

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
                        return;
                    }

                    dataPath = directorie;
                }
            }

            if (dataPath == null)
            {
                errors.Invoke("oh God NO Datapath Exiting!!!");
                return;
            }


            var stagingManaged =
                stagingDirectory.CreateSubdirectory(Path.Combine(dataPath.Name, "Managed"));

            var dllDirectory = dataPath.CreateSubdirectory("Managed");

            CopyFilesRecursively(stagingManaged.ToString(), dllDirectory.ToString());


            DirectoryInfo goodFileCopy = new DirectoryInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                "GoodFiles", "VDev", "Managed"));

            info.Invoke("Proceeding to scan folder");
            if (ScanFolder(dllDirectory, goodFileCopy, info, errors) == false)
            {
                DeleteContentsOfDirectory(processingDirectory);
                return;
            }

            var pluginsFolder = dataPath.CreateSubdirectory("Plugins");
            pluginsFolder.Delete(true);

            pluginsFolder = dataPath.CreateSubdirectory("Plugins");

            DirectoryInfo goodPluginsCopy = new DirectoryInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                "GoodFiles", "VDev", "Plugins"));

            //TODO OS Switch?
            CopyFilesRecursively(goodPluginsCopy.ToString(), pluginsFolder.ToString());


            var monoBleedingEdge = processingDirectory.CreateSubdirectory("MonoBleedingEdge");

            monoBleedingEdge.Delete(true);


            var goodRoot = new DirectoryInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                "GoodFiles", "VDev", "Root"));

            try
            {
                CopyFilesRecursively(goodRoot.ToString(), processingDirectory.ToString());
            }
            catch (Exception e)
            {
                errors.Invoke("e > " + e);
                DeleteContentsOfDirectory(processingDirectory);
                return;
            }


            var exeRename = processingDirectory.GetFiles()
                .FirstOrDefault(x => x.Extension == ".exe" && x.Name != "UnityCrashHandler64.exe"); //TODO OS

            if (exeRename == null || exeRename.Directory == null)
            {
                errors.Invoke("no Executable found ");
                DeleteContentsOfDirectory(processingDirectory);
                return;
            }

            exeRename.MoveTo(Path.Combine(exeRename.Directory.ToString(), dataPath.Name.Replace("_Data", "") + ".exe"));


            DirectoryInfo goodBuilddirectory = directory.CreateSubdirectory("GoodBuild");
            DeleteContentsOfDirectory(goodBuilddirectory);
            CopyFilesRecursively(processingDirectory.ToString(), goodBuilddirectory.ToString());
            DeleteContentsOfDirectory(processingDirectory);
            //deleteContentsOfDirectory(Stagingdirectory); TODO
        }
        catch (Exception e)
        {
            errors.Invoke(" an Error happened > " + e);
            DeleteContentsOfDirectory(processingDirectory);
            //deleteContentsOfDirectory(Stagingdirectory); TODO
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

    static void DeleteFilesWithExtension(string directoryPath, string fileExtension, bool recursive = true)
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
                DeleteFilesWithExtension(subdirectory.FullName, fileExtension);
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