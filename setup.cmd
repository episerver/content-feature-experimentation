@echo off
setlocal

SET Samples=samples\QuickSilver\EPiServer.Reference.Commerce.Site

IF EXIST %Samples%\App_Data\Blobs (
    ECHO Remove all files from the app data blobs folder
    RMDIR %Samples%\App_Data\blobs /S /Q || Exit /B 1
)

IF EXIST %Samples%\App_Data\Quicksilver (
    ECHO Remove all files from the app data index folder
    RMDIR %Samples%\App_Data\Quicksilver /S /Q || Exit /B 1
)

cd samples\QuickSilver\
SetupDatabases.cmd