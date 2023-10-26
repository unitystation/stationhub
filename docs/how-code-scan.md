# How Does The Dode Scanner Work
Brief overview, 
Every build is a normal build of the game, you can extract it and run it directly if you would like,
The code scanning is here to ensure The security of code being run by the hub, 

So the first thing it does Download the build extract to a processing directory, 

Clean out all of the executable files it can find, 
Then copy over the Managed folder That contain all the Custom Dlls + unity Dlls,

Then download the Safe files For the corresponding OS, these are all the Assemblies that are high-risk, We provide them,
They overwrite any That are present in the managed folder and won't be scanned.

so, It loops through every .DLL, and scans it for any violations From the whitelist, 

e.g If your assembly tried accessing system.IO  it's not specified in the White list so it will be blocked and fail the entire Scan
It allows references to other provided assemblies, but those will be scanned to so if they violate It will fail the scan,

Once it has scanned all the assemblies It's copied into the installation directory and is ready to use

