# Stationhub
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/unitystation/stationhub/dotnetcore.yml?style=flat-square)](https://github.com/unitystation/stationhub/actions/workflows/dotnetcore.yml)
[![Codacy grade](https://img.shields.io/codacy/grade/b6c9615ab3ba47f091efb0ff28e24798?style=flat-square)](https://app.codacy.com/gh/unitystation/stationhub)
[![AUR version](https://img.shields.io/aur/version/stationhub?style=flat-square)](https://aur.archlinux.org/packages/stationhub)
[![Flathub](https://img.shields.io/flathub/v/org.unitystation.StationHub?style=flat-square)](https://flathub.org/apps/details/org.unitystation.StationHub)
[![Discord](https://img.shields.io/discord/273774715741667329?style=flat-square)](https://discord.com/invite/tFcTpBp)

This is the official launcher for Unitystation, it handles account creation, downloading, updating, and server joining.

## Tech-stack
It is cross-platform using .NET 6 as the runtime and [Avalonia MVVM](https://docs.avaloniaui.net/guides/basics/mvvm) for the UI.

## Building
You'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed.

Once that is installed you should be able to check out the repo and run the following in the directory with the `UnitystationLauncher.sln` file:
```
dotnet build
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
