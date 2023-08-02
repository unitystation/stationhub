using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Serilog;
using UnitystationLauncher.ContentScanning;


namespace UnitystationLauncher.ViewModels;

public class SecurityPanelViewModel : PanelBase
{
    public override string Name => "Security";
    public override bool IsEnabled => true;

    public SecurityPanelViewModel()
    {
    }


    public override void Refresh()
    {
    }

    //TODO
    // in-memory virtual file system
    //UnityPlayer.dll?? look in to
    //Unitystation_Data\Plugins
    //MonoBleedingEdge
    //the delete any DLLs outside of the folder Just to be safe
    //Replace exes with Good ones
    //Scan files in 
    //Cleanup files end
    //  Directory.CreateDirectory(destinationDirectory) keep in Directory
    //LoadConfig(ISawmill sawmill)
    //private const string SystemAssemblyName = "mscorlib"; //TODO check security Not trusted Need to input Safe DLL
    // if (string.Equals(fileName, assemblyName.Name, StringComparison.OrdinalIgnoreCase))  Potential security+
    //Generic maybe not allowed???:
    //return true; //RRT 
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
    // Sprite
    //Decimal
    // to!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //remove DEBUG Exceptions ; 
    // "Assets"
    // "SerializableDictionary"
    // "Chemistry"
    // "Shared"
    //debug why logger Is not being added
    //TO PULL out hehhe
    //Verbal viewer
    //Synth
    
    //BAD Look out for
    //System.Diagnostics.Process
    //System.Xml.XmlTextReader
    //UnityEngine.Windows.File
    
    //GOODs 
    //var path = Path.Combine(streamingAssetsPath, fileName);
    //StringReader 
    
    
    //TO Patch
    //UnityEngine.Events.UnityEvent, GetValidMethodInfo
    
    //Manually set 
    //Texture2D
    //ParticleSystem
    //Input
    //GUIUtility
    //GUI?
    //ImageConversion 
    
    //TODO Stuff with annoying file access
    //profiler + messages to delete and add
    //BuildPreferences == If editor
    //admin/PlayerList So many file watches ;n;
    //Application.OpenURL()

    public void OnClickCommand()
    {
        //TODO remove exes


        // Create a DirectoryInfo object for the directory
        DirectoryInfo directory = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);

        DirectoryInfo Stagingdirectory = directory.CreateSubdirectory("staging"); //TODO temp
        DirectoryInfo Processingdirectory = directory.CreateSubdirectory("processing");
        deleteContentsOfDirectory(Processingdirectory);
        CopyFilesRecursively(Stagingdirectory.ToString(), Processingdirectory.ToString());
        DeleteFilesWithExtension(Processingdirectory.ToString(), "exe");

        // Get all files in the directory
        var Directories = Processingdirectory.GetDirectories();

        DirectoryInfo Datapath = null;

        // Loop through each file
        foreach (var Directorie in Directories)
        {
            if (Directorie.Name.Contains("_Data"))
            {
                if (Datapath != null)
                {
                    Log.Error("oh God 2 Datapaths Exiting!!!");
                    return;
                }

                Datapath = Directorie;
            }
        }

        if (Datapath == null)
        {
            Log.Error("oh God NO Datapath Exiting!!!");
            return;
        }

        Log.Error("Datapath > " + Datapath);

        var DLLDirectory = Datapath.CreateSubdirectory("Managed");


        var Files = DLLDirectory.GetFiles();

        List<FileInfo> ToDelete = new List<FileInfo>();

        foreach (var File in Files)
        {
            if (File.Name == "mscorlib.dll") continue; //TODO TEMP Causes an error If it's passed by itself
             //DO Scan
            Log.Information("TODO Scanning " + File);
            try
            {
                if (MakeTypeChecker().CheckAssembly(File, DLLDirectory) == false)
                { 
                    ToDelete.Add(File);
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        foreach (var File in ToDelete)
        {
            File.Delete();
        }
        
        //TODO 
        //Drop-in good exes and rename
        //move to "finished" folder

        DirectoryInfo GoodBuilddirectory = directory.CreateSubdirectory("GoodBuild");
        deleteContentsOfDirectory(GoodBuilddirectory);
        CopyFilesRecursively(Processingdirectory.ToString(), GoodBuilddirectory.ToString());
    }

    #region Scanning

    private AssemblyTypeChecker MakeTypeChecker()
    {
        return new()
        {
            VerifyIL = true,
            DisableTypeCheck = false,
        };
    }

    #endregion

    #region Utilities

    public void deleteContentsOfDirectory(DirectoryInfo directory)
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

    static void DeleteFilesWithExtension(string directoryPath, string fileExtension)
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

        // Recursively process subdirectories
        DirectoryInfo[] subdirectories = directory.GetDirectories();
        foreach (DirectoryInfo subdirectory in subdirectories)
        {
            DeleteFilesWithExtension(subdirectory.FullName, fileExtension);
        }
    }

    static void CopyFilesRecursively(string sourceDirectory, string destinationDirectory)
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

    #endregion
}