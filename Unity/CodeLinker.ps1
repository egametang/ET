$packages = ("Packages", "Library\PackageCache")
$dirs = ("Model~", "Hotfix~", "ModelView~", "HotfixView~")
$client_server_dir = ("Client", "Server", "Share")


function Link($targetPath, $linker, $subPath, $moudle)
{
    $path = $linker + $subPath
    foreach($c in Get-ChildItem $path)
    {
        if ($c -is [System.IO.FileInfo])
        {
            $to = $targetPath + "/" + $moudle + $subPath + $c.Name

            Write-Host "link:", $to, $c.FullName

            New-Item -ItemType HardLink -Path $to -Value $c.FullName -Force
            continue
        }
        elseif ($c -is [System.IO.DirectoryInfo])
        {
            $newSubPath = $subPath + $c.Name + "/"
            Link $targetPath $linker $newSubPath $moudle
        }
    }
}


foreach($package in $packages)
{
    foreach($a in Get-ChildItem $package) 
    {
        # name = core
        $name = $a.Name
        if (!$name.StartsWith("com.et.")) 
        {
            continue
        }
        $name = $name.Substring(7, $name.Length - 7);

        $newDir = $a.FullName + "/Scripts"

        # $b model~ dir
        foreach($b in Get-ChildItem $newDir)
        {
            $name2 = $b.Name
            if (!$dirs.Contains($name2))
            {
                continue
            }

            $path = $b.FullName

            # name3  Hotfix  name2 Hotfix~
            $name3 = $name2.Substring(0, $name2.Length - 1)

            foreach($c in Get-ChildItem $path)
            {
                $name4 = $c.Name
                if (!$client_server_dir.Contains($name4))
                {
                    continue
                }

                # Assets/Scripts/Hotfix/Client/
                $targetPath = "Assets/Scripts/$name3/$name4"
                Link $targetPath $c.FullName "/" $name
            }
        }
    }
}