@echo off
setlocal
SET PATH=.\.ci\tools\;%PATH%

IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)
ECHO Building in %Configuration%

REM Install KPI.Commerce dependencies
CALL yarn --cwd src\EPiServer.Marketing.KPI.Commerce\clientResources install
IF %errorlevel% NEQ 0 exit /B %errorlevel%

IF "%1"=="Release" (CALL yarn --cwd src\EPiServer.Marketing.KPI.Commerce\clientResources build) ELSE (CALL yarn --cwd src\Episerver.Marketing.KPI.Commerce\clientResources dev)
IF %errorlevel% NEQ 0 exit /B %errorlevel%

REM Install Testing.Web dependencies
CALL yarn --cwd src\EPiServer.Marketing.Testing.Web\ClientResources\Config install
IF %errorlevel% NEQ 0 exit /B %errorlevel%

CALL yarn --cwd src\EPiServer.Marketing.Testing.Web\ClientResources\Config run build
IF %errorlevel% NEQ 0 exit /B %errorlevel%

dotnet build EPiServer.Marketing.Testing.sln -c %Configuration%