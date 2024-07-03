foreach($dir in Get-ChildItem "Library/PackageCache")
{
    if (!($dir -is [System.IO.DirectoryInfo]))
    {
        continue
    }

    if ($dir.Name.StartsWith("cn.etetet"))
    {
        $baseName = $dir.Name.Substring(0, $dir.Name.indexOf("@"))
        
        $t = $dir.Name
        Write-Host "move Library/PackageCache/$t to Packages/$baseName"
        Move-Item "Library/PackageCache/$t" "Packages/$baseName"
    }  
}

Write-Host "move finish!"
#Read-Host -Prompt "Press Enter to exit"