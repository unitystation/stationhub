#!/bin/bash

APP_NAME="/Users/runner/publish/StationHub.app"
PUBLISH_OUTPUT_DIRECTORY="/Users/runner/publish-macos/."
INFO_PLIST="/Users/runner/work/stationhub/stationhub/tools/Info.plist"
ENTITLEMENTS="/Users/runner/work/stationhub/stationhub/tools/MyAppEntitlements.entitlements"
ICON_FILE="/Users/runner/work/stationhub/stationhub/UnitystationLauncher/Assets/Ian.icns"
SIGNING_IDENTITY="Developer ID: MyCompanyName" # matches Keychain Access certificate name

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
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "/Users/runner/publish/"

#find "$APP_NAME/Contents/MacOS/"|while read fname; do
#    if [[ -f $fname ]]; then
#        echo "[INFO] Signing $fname"
#        codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$fname"
#    fi
#done

#echo "[INFO] Signing app file"

#codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$APP_NAME"
