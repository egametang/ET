param($packageName, $version)

Write-Host $packageName, $version

$from = "Library/PackageCache/$packageName@" + $version
Move-Item $from "Packages/$packageName"

Write-Host "move finish!" $packageName $version
#Read-Host -Prompt "Press Enter to exit"