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
    
}