using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Threading;
using MessageBox.Avalonia.BaseWindows.Base;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.ContentScanning;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Views;


namespace UnitystationLauncher.ViewModels;

public class Testing
{
    private static NamedPipeServerStream serverPipe;
    private static StreamReader reader;
    private static StreamWriter writer;

    public Testing()
    {
        serverPipe = new NamedPipeServerStream("Unitystation_Hub_Build_Communication", PipeDirection.InOut, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
    }

    private enum ClientRequest
    {
        URL = 1,
        API_URL = 2,
        Host_Trust_Mode = 3,
    }

    public async void Do()
    {
        await serverPipe.WaitForConnectionAsync();
        reader = new StreamReader(serverPipe);
        writer = new StreamWriter(serverPipe);

        while (true)
        {
            string? request = await reader.ReadLineAsync();
            if (request == null)
            {
                await serverPipe.WaitForConnectionAsync();
                reader = new StreamReader(serverPipe);
                writer = new StreamWriter(serverPipe);
                continue;
            }

            var requests = request.Split(",");
            Console.WriteLine($"Server: Received request: {request}");

            if (ClientRequest.URL.ToString() == requests[0])
            {
                RxApp.MainThreadScheduler.ScheduleAsync(async  (_, _) =>
                {
                    IMsBoxWindow<string> msgBox = MessageBoxBuilder.CreateMessageBox(
                        MessageBoxButtons.YesNo,
                        string.Empty,
                        $"would you like to add this Domain to The allowed domains to be opened In your browser, {requests[1]} " + @"
Justification given by the Fork : " + requests[2]); 
                    
                    string response = await msgBox.Show();

                    await writer.WriteLineAsync(response == "No" ? false.ToString() : true.ToString());
                    await writer.FlushAsync();
                    //data.Wait();
                    //var AAAAA = data;
                    return Task.CompletedTask;
                });
            }
            else if (ClientRequest.API_URL.ToString() == requests[0])
            {
                RxApp.MainThreadScheduler.ScheduleAsync(async  (_, _) =>
                {
                    IMsBoxWindow<string> msgBox = MessageBoxBuilder.CreateMessageBox(
                        MessageBoxButtons.YesNo,
                        string.Empty,
                        $"The build would like to send an API request to, {requests[1]} " + @"
do you allow this fork to now on access this domain
Justification given by the Fork : " + requests[2]); 
                    

                    string response = await msgBox.Show();

                    await writer.WriteLineAsync(response == "No" ? false.ToString() : true.ToString());
                    await writer.FlushAsync();
                    //data.Wait();
                    //var AAAAA = data;
                    return Task.CompletedTask;
                });

            }
            else if (ClientRequest.Host_Trust_Mode.ToString() == requests[0])
            {
                RxApp.MainThreadScheduler.ScheduleAsync(async  (_, _) =>
                {
                    IMsBoxWindow<string> msgBox = MessageBoxBuilder.CreateMessageBox(
                        MessageBoxButtons.YesNo,
                        string.Empty,
                         @" Trusted mode automatically allows every API and open URL action to happen without prompt, this also enables the 
Variable viewer ( Application that can modify the games Data ) that Could potentially be used to Perform malicious actions on your PC,
 The main purpose of this Prompt is to allow the Variable viewer (Variable editing), 
What follows is given by the build, we do not control what is written in the Following text So treat with caution and use your brain
 Justification : " + requests[1]); //TODO Add text

                    string response = await msgBox.Show();

                    await writer.WriteLineAsync(response == "No" ? false.ToString() : true.ToString());
                    await writer.FlushAsync();
                    //data.Wait();
                    //var AAAAA = data;
                    return Task.CompletedTask;
                });
                
            }
        }
    }
}

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
    //Linking with build to handle URL/APi allowed Host check By user


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
    //Big old try catch to catch any naughties, That deletes and cancels the Scan
    //Delete scanning before doing process
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
    // Sprite
    //Decimal
    // to!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //remove DEBUG Exceptions ; 
    // "Assets"
    // "SerializableDictionary"
    // "Chemistry"
    // "Shared"
    //debug why logger Is not being added


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
    //Profiling.Profiler -> .GetMonoUsedSizeLong(), EndSample(), .BeginSample(string);
    //PlayerPrefs?
    //Application
    //UnityEngine.Resources
    //Addressables
    //Unity.Collections.NativeArray
    // Resources.Load?

    //TO PULL out hehhe
    //Verbal viewer
    //Synth
    //BuildPreferences == If editor
    //NetworkManagerExtensions

    public void OnSetUpPipe()
    {
        var data = new Testing();
        new Task(data.Do).Start();
    }

    public void OnScan()
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


        DirectoryInfo GoodFileCopy = new DirectoryInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
            "GoodFiles", "VDev", "Managed"));
        var GoodFiles = GoodFileCopy.GetFiles().Select(x => x.Name).ToList();

        CopyFilesRecursively(GoodFileCopy.ToString(), DLLDirectory.ToString());

        var Files = DLLDirectory.GetFiles();

        List<FileInfo> ToDelete = new List<FileInfo>();

        List<string> MultiAssemblyReference = new List<string>();

        foreach (var File in Files)
        {
            if (GoodFiles.Contains(File.Name)) continue;
            MultiAssemblyReference.Add(Path.GetFileNameWithoutExtension(File.Name));
        }


        foreach (var File in Files)
        {
            if (GoodFiles.Contains(File.Name)) continue;
            //DO Scan
            Log.Information("TODO Scanning " + File);
            try
            {
                var Listy = MultiAssemblyReference.ToList();
                Listy.Remove(Path.GetFileNameWithoutExtension(File.Name));
                if (MakeTypeChecker().CheckAssembly(File, DLLDirectory, Listy) == false)
                {
                    ToDelete.Add(File);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //TODO Explode
            }
        }


        foreach (var File in ToDelete)
        {
            File.Delete();
        }

        Files = DLLDirectory.GetFiles();

        var ToRemove =
            Files.Where(x => GoodFiles.Contains(x.Name)); //TODO Is temp, just to see what past And as provided

        foreach (var File in ToRemove)
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