#!/bin/bash

APP_NAME="/Users/runner/publish/StationHub.app"
INFO_PLIST="/Users/runner/work/stationhub/stationhub/UnitystationLauncher/Assets/Info.plist"
ICON_FILE="/Users/runner/work/stationhub/stationhub/UnitystationLauncher/Assets/ian.ico"

mkdir "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/ian.ico"
