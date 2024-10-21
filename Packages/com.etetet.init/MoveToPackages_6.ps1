param($packageName, $version)

Write-Host $packageName

$from = "Library/PackageCache/$packageName" 
Move-Item $from "Packages/$packageName"

Write-Host "move finish!" $packageName 
#Read-Host -Prompt "Press Enter to exit"