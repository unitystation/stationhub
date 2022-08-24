#!/bin/bash

APP_NAME="/Users/runner/publish/StationHub.app"
PUBLISH_OUTPUT_DIRECTORY="/Users/runner/publish-macos/."
INFO_PLIST="/Users/runner/work/stationhub/stationhub/tools/Info.plist"
ICON_FILE="/Users/runner/work/stationhub/stationhub/UnitystationLauncher/Assets/Ian.icns"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir "/Users/runner/publish"
mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/Ian.icns"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"
