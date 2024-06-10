foreach($dir in Get-ChildItem "Library/PackageCache")
{
    if (!($dir -is [System.IO.DirectoryInfo]))
    {
        continue
    }

    if ($dir.Name.StartsWith("cn.etetet"))
    {
        $baseName = $dir.Name.Substring(0, $dir.Name.indexOf("@"))
             
        Move-Item "Library/PackageCache/$dir" "Packages/$baseName"
        Write-Host "move Library/PackageCache/$dir to Packages/$baseName"
    }  
}

Write-Host "move finish!"
#Read-Host -Prompt "Press Enter to exit"