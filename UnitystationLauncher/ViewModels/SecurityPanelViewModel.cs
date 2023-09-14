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
using UnitystationLauncher.Services.Interface;
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
                try
                {
                    await serverPipe.WaitForConnectionAsync();
                }
                catch (System.IO.IOException e)
                {
                    serverPipe.Close();
                    serverPipe = new NamedPipeServerStream("Unitystation_Hub_Build_Communication", PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    await serverPipe.WaitForConnectionAsync();
                }

                reader = new StreamReader(serverPipe);
                writer = new StreamWriter(serverPipe);
                continue;
            }

            var requests = request.Split(",");
            Console.WriteLine($"Server: Received request: {request}");

            if (ClientRequest.URL.ToString() == requests[0])
            {
                RxApp.MainThreadScheduler.ScheduleAsync(async (_, _) =>
                {
                    IMsBoxWindow<string> msgBox = MessageBoxBuilder.CreateMessageBox(
                        MessageBoxButtons.YesNo,
                        string.Empty,
                        $"would you like to add this Domain to The allowed domains to be opened In your browser, {requests[1]} " +
                        @"
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
                RxApp.MainThreadScheduler.ScheduleAsync(async (_, _) =>
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
                RxApp.MainThreadScheduler.ScheduleAsync(async (_, _) =>
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

    private readonly IAssemblyChecker _IAssemblyChecker;
    
    public SecurityPanelViewModel(IAssemblyChecker assemblyChecker)
    {
        _IAssemblyChecker = assemblyChecker;
    }


    public override void Refresh()
    {
    }

    //TODO 
    //Automatic start-up pipe


    //TODO
    // in-memory virtual file system

    //UnityPlayer.dll?? look in to
    //Unitystation.exe

    //Cleanup files end 

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


    public void OnSetUpPipe()
    {
        var data = new Testing();
        new Task(data.Do).Start();
    }

    public void OnUpdateSafe()
    {
        DirectoryInfo directory = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);

        DirectoryInfo Stagingdirectory = directory.CreateSubdirectory("staging");

        Stagingdirectory = Stagingdirectory.CreateSubdirectory("unitystation_Data");
        Stagingdirectory = Stagingdirectory.CreateSubdirectory("Managed");

        DirectoryInfo GoodDirectory = directory.CreateSubdirectory("GoodFiles");
        GoodDirectory = GoodDirectory.CreateSubdirectory("VDev");
        GoodDirectory = GoodDirectory.CreateSubdirectory("Managed");


        DirectoryInfo sourceDirInfo = Stagingdirectory;
        DirectoryInfo targetDirInfo = GoodDirectory;

        // Get the list of files that are present in both directories
        FileInfo[] sourceFiles = sourceDirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        FileInfo[] targetFiles = targetDirInfo.GetFiles("*.*", SearchOption.AllDirectories);

        var commonFiles = sourceFiles.Intersect(targetFiles, new FileInfoComparer());

        // Do something with the common files
        foreach (FileInfo file in commonFiles)
        {
            File.Copy(sourceDirInfo.GetFiles().FirstOrDefault(x => x.Name == file.Name).FullName,
                targetDirInfo.GetFiles().FirstOrDefault(x => x.Name == file.Name).FullName, true);
            Console.WriteLine($"Common file: {file.FullName}");
        }

        Console.WriteLine("Common files listed.");
    }


    class FileInfoComparer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo x, FileInfo y)
        {
            return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(FileInfo obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    public void OnScan()
    {
        Action<string> Info = new Action<string>((string log) => { });
        Action<string> Errors = new Action<string>((string log) => { });
        
        // Create a DirectoryInfo object for the directory
        DirectoryInfo directory = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);

        DirectoryInfo Stagingdirectory = directory.CreateSubdirectory("staging"); //TODO temp
        DirectoryInfo Processingdirectory = directory.CreateSubdirectory("processing");
        DirectoryInfo Datapath = null;

        try
        {
            deleteContentsOfDirectory(Processingdirectory);
            Info.Invoke("Copying files");
            CopyFilesRecursively(Stagingdirectory.ToString(), Processingdirectory.ToString());

            Info.Invoke("Cleaning out Dlls and Executables");
            DeleteFilesWithExtension(Processingdirectory.ToString(), "exe");
            DeleteFilesWithExtension(Processingdirectory.ToString(), "dll");

            // Get all files in the directory
            var Directories = Processingdirectory.GetDirectories();


            // Loop through each file
            foreach (var Directorie in Directories)
            {
                if (Directorie.Name.Contains("_Data"))
                {
                    if (Datapath != null)
                    {
                        Errors.Invoke("oh God 2 Datapaths Exiting!!!");
                        return;
                    }

                    Datapath = Directorie;
                }
            }

            if (Datapath == null)
            {
                Errors.Invoke("oh God NO Datapath Exiting!!!");
                return;
            }

            
            var StagingManaged =
                Processingdirectory.CreateSubdirectory(Path.Combine(Datapath.Name, "Managed"));

            var DLLDirectory = Datapath.CreateSubdirectory("Managed");

            CopyFilesRecursively(StagingManaged.ToString(), DLLDirectory.ToString());


            DirectoryInfo GoodFileCopy = new DirectoryInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                "GoodFiles", "VDev", "Managed"));

            Info.Invoke("Proceeding to scan folder");
            if (ScanFolder(DLLDirectory, GoodFileCopy, Info, Errors) == false)
            {
                deleteContentsOfDirectory(Processingdirectory);
                return;
            }

            var PluginsFolder = Datapath.CreateSubdirectory("Plugins");
            PluginsFolder.Delete(true);

            PluginsFolder = Datapath.CreateSubdirectory("Plugins");

            DirectoryInfo GoodPluginsCopy = new DirectoryInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                "GoodFiles", "VDev", "Plugins"));

            //TODO OS Switch?
            CopyFilesRecursively(GoodPluginsCopy.ToString(), PluginsFolder.ToString());


            var MonoBleedingEdge = Processingdirectory.CreateSubdirectory("MonoBleedingEdge");

            MonoBleedingEdge.Delete(true);


            var GOODRoot = new DirectoryInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                "GoodFiles", "VDev", "Root"));

            try
            {
                CopyFilesRecursively(GOODRoot.ToString(), Processingdirectory.ToString());
            }
            catch (Exception e)
            {
                Errors.Invoke("e > " + e);
                deleteContentsOfDirectory(Processingdirectory);
                return;
            }


            var exeRename = Processingdirectory.GetFiles()
                .FirstOrDefault(x => x.Extension == ".exe" && x.Name != "UnityCrashHandler64.exe"); //TODO OS

            if (exeRename == null)
            {
                Errors.Invoke("no Executable found ");
                deleteContentsOfDirectory(Processingdirectory);
                return;
            }

            exeRename.MoveTo(Path.Combine(exeRename.Directory.ToString(), Datapath.Name.Replace("_Data", "") + ".exe"));


            DirectoryInfo GoodBuilddirectory = directory.CreateSubdirectory("GoodBuild");
            deleteContentsOfDirectory(GoodBuilddirectory);
            CopyFilesRecursively(Processingdirectory.ToString(), GoodBuilddirectory.ToString());
            deleteContentsOfDirectory(Processingdirectory);
            //deleteContentsOfDirectory(Stagingdirectory); TODO
        }
        catch (Exception e)
        {
            Errors.Invoke( " an Error happened > " + e.ToString());
            deleteContentsOfDirectory(Processingdirectory);
            //deleteContentsOfDirectory(Stagingdirectory); TODO
            return;
        }
    }

    public bool ScanFolder(DirectoryInfo Unsafe, DirectoryInfo SaveFiles, Action<string> Info, Action<string> Errors)
    {
        var GoodFiles = SaveFiles.GetFiles().Select(x => x.Name).ToList();

        Info.Invoke("Provided files " + string.Join(",", GoodFiles));
        
        CopyFilesRecursively(SaveFiles.ToString(), Unsafe.ToString());

        var Files = Unsafe.GetFiles();

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

            Info.Invoke("Scanning " + File.Name);

            try
            {
                var Listy = MultiAssemblyReference.ToList();
                Listy.Remove(Path.GetFileNameWithoutExtension(File.Name));
                if (_IAssemblyChecker.CheckAssembly(File, Unsafe, Listy, Errors) == false)
                {
                    Errors.Invoke($"{File.Name} Failed scanning Cancelling");
                    return false;
                }
            }
            catch (Exception e)
            {
                Errors.Invoke(" Failed scan due to error of " + e.ToString());
                return false;
            }
        }


        return true;
    }
    

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

    static void DeleteFilesWithExtension(string directoryPath, string fileExtension, bool Recursive = true)
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

        if (Recursive)
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