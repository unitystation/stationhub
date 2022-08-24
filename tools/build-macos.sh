#!/bin/bash

APP_NAME="/Users/runner/publish/StationHub.app"
PUBLISH_OUTPUT_DIRECTORY="/Users/runner/work/stationhub/stationhub/UnitystationLauncher/bin/Release/netcoreapp3.1/osx-x64/."
INFO_PLIST="/Users/runner/work/stationhub/stationhub/UnitystationLauncher/Assets/Info.plist"
ICON_FILE="/Users/runner/work/stationhub/stationhub/UnitystationLauncher/Assets/ian.ico"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/ian.ico"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"
