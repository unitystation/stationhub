# Stationhub

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/unitystation/stationhub/dotnetcore.yml?style=flat-square)](https://github.com/unitystation/stationhub/actions/workflows/dotnetcore.yml)
[![Codacy grade](https://img.shields.io/codacy/grade/b6c9615ab3ba47f091efb0ff28e24798?style=flat-square)](https://app.codacy.com/gh/unitystation/stationhub)
[![AUR version](https://img.shields.io/aur/version/stationhub?style=flat-square)](https://aur.archlinux.org/packages/stationhub)
[![Flathub](https://img.shields.io/flathub/v/org.unitystation.StationHub?style=flat-square)](https://flathub.org/apps/details/org.unitystation.StationHub)
[![Discord](https://img.shields.io/discord/273774715741667329?style=flat-square)](https://discord.com/invite/tFcTpBp)

StationHub is the official launcher for Unitystation, it handles downloading, updating, and joining servers.

## Tech-stack

It is cross-platform using .NET 8 as the runtime and [Avalonia MVVM](https://docs.avaloniaui.net/guides/basics/mvvm) for the UI.

## Building

You'll need [git](https://git-scm.com) and the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed.

To install the Microsoft .NET SDK version 8 using Windows Package Manager (winget), run the following command in terminal or command prompt:

```
winget install Microsoft.DotNet.SDK.8
```

To check out the repo you can run the following in the directory you'd like to save the repo locally:

```
git clone https://github.com/unitystation/stationhub.git
```

Once checked out you should be able to run the following in the directory with the `UnitystationLauncher.sln` file:

```
dotnet build
```

Dependencies should be automatically restored by NuGet during the build process.

To test the build you just ran, you can do the following:

```
dotnet run --project ./UnitystationLauncher/UnitystationLauncher.csproj
```

## Contributing

Before opening your pull request please ensure there are no compile warnings for any new code that you've added.

.NET format is also enforced by the build pipeline, so before pushing your code you can run the following to make sure your code is formatted in the standard way:

```
dotnet format
```

Unit Tests are a work in progress currently, however once they are included you'll be able to run them with:

```
dotnet test
```

You'll want to ensure that existing tests pass, and make changes and additional tests where needed for your change.
Also make sure to check out the [Code Style Guide](https://github.com/unitystation/stationhub/blob/develop/docs/code-style-guide.md) for the project.
