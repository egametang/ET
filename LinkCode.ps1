$src = @(
    'Codes\Hotfix',
    'Codes\Model',
    'Codes\ModelView',
    'Codes\HotfixView',
    'Codes\Generate\Server'
)

$tar = @(
    'Unity\Assets\Scripts\Codes\Link\Hotfix',
    'Unity\Assets\Scripts\Codes\Link\Model',
    'Unity\Assets\Scripts\Codes\Link\ModelView',
    'Unity\Assets\Scripts\Codes\Link\HotfixView',
    'Unity\Assets\Scripts\Codes\Link\Generate\Server'
)

for($i = 0; $i -lt $src.count; $i++)
{
    $from = $src[$i]
    $to = $tar[$i]
    
    $cmd = "cmd /c rd " + $to

    Write-Host $cmd
    Invoke-Expression $cmd

    Write-Host $from  $to
    Remove-Item $to -ErrorAction Ignore -Recurse
    mkdir -p $to
    Remove-Item $to -ErrorAction Ignore -Recurse
    
    $cmd = "cmd /c mklink /j " + $to + " " + $from

    Write-Host $cmd
    Invoke-Expression $cmd
}

"Any key to exit"  ;
Read-Host | Out-Null ;
Exit