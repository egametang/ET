function PublishLinux {
    dotnet publish ET.sln -r linux-x64 --no-self-contained --no-dependencies -c Release

    $path = "Publish\linux-x64"
    Remove-Item $path\Bin\ -Recurse -ErrorAction Ignore
    Copy-Item .\Bin\linux-x64\publish -Destination $path\Bin -Recurse -Force

    Remove-Item $path\Packages -Recurse -ErrorAction Ignore

    $matchingPaths = Get-ChildItem -Path "Packages" -Directory

    $matchingPaths | ForEach-Object {
        $relativePath = Join-Path $_ "Config"
        $fullConfigPath = Join-Path "Packages" $relativePath
        if (Test-Path $fullConfigPath -PathType Container) {
            Write-Host "Find Config :"$fullConfigPath
            $targetPath = Join-Path $path $fullConfigPath
            Write-Host "CopyTo :"$targetPath
            Copy-Item $fullConfigPath -Destination $targetPath -Recurse -Force
        }
    }

    pause
}

cd ../

PublishLinux