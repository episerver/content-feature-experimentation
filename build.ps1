param ([string]$mode = "Release")

Write-Output "Running script in mode: $mode"
yarn --cwd src/EPiServer.Marketing.KPI.Commerce/clientResources install
yarn --cwd src/EPiServer.Marketing.KPI.Commerce/clientResources build
yarn --cwd src/EPiServer.Marketing.Testing.Web/clientResources/config install
yarn --cwd src/EPiServer.Marketing.Testing.Web/clientResources/config build
dotnet build -c $mode
./build/pack.ps1 -configuration $mode 
./build/repack.ps1