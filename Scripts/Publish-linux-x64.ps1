dotnet publish -r linux-x64 --no-self-contained --no-dependencies -c Release
$path = ".\Publish\linux-x64"
Remove-Item $path\Bin\ -Recurse -ErrorAction Ignore
Copy-Item .\Bin\linux-x64\publish -Destination $path\Bin -Recurse -Force
Remove-Item $path\Config -Recurse -ErrorAction Ignore
Copy-Item .\Config -Destination $path\Config  -Recurse -Force