$v = "3.1.0";
$newV = "0.2.1";
Remove-Item -Path "artifacts/packages/EPiServer.Marketing.Testing.$v" -Recurse -Force -Confirm:$false -ErrorAction Ignore
Expand-Archive -LiteralPath "artifacts/packages/EPiServer.Marketing.Testing.$v.nupkg" -DestinationPath "artifacts/packages/EPiServer.Marketing.Testing.$v"
Remove-Item "artifacts/packages/EPiServer.Marketing.Testing.$v/EPiServer.Marketing.Testing.nuspec"
Copy-Item "build/Research.Marketing.Experimentation.nuspec"  "artifacts/packages/EPiServer.Marketing.Testing.$v/"
Remove-Item "artifacts/packages/Research.Marketing.Experimentation.$newV.nupkg" -Force -Confirm:$false -ErrorAction Ignore
[System.IO.Compression.ZipFile]::CreateFromDirectory("artifacts/packages/EPiServer.Marketing.Testing.$v", "artifacts/packages/Research.Marketing.Experimentation.$newV.nupkg")
Remove-Item -Path "artifacts/packages/EPiServer.Marketing.Testing.$v" -Recurse -Force -Confirm:$false -ErrorAction Ignore
