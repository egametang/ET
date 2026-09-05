param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$SceneName = '',
    [string]$MainPackage = '',
    [ValidateSet('Client', 'Server', 'ClientServer')]
    [string]$CodeMode = '',
    [switch]$GenerateMainPackageOnly,
    [switch]$SkipLubanBuild,
    [switch]$SkipToolBuild,
    [switch]$SkipCodeMode,
    [switch]$SkipFullBuild,
    [switch]$BuildSolution,
    [string[]]$DotNetSdkVersions = @(),
    [string]$GitExecutable = 'git'
)

$ErrorActionPreference = 'Stop'

function Write-Step {
    param([string]$Message)
    Write-Host "[Initialize] $Message"
}

function Resolve-ProjectPath {
    param([string]$Path)
    return [System.IO.Path]::GetFullPath((Join-Path $ProjectRoot $Path))
}

function Assert-FileExists {
    param(
        [string]$Path,
        [string]$Description
    )

    if (!(Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Description 不存在: $Path"
    }
}

function Assert-DirectoryExists {
    param(
        [string]$Path,
        [string]$Description
    )

    if (!(Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Description 不存在: $Path"
    }
}

function Invoke-CheckedCommand {
    param(
        [string]$FilePath,
        [string[]]$ArgumentList,
        [string]$Description
    )

    Write-Step $Description
    & $FilePath @ArgumentList
    if ($LASTEXITCODE -ne 0) {
        throw "$Description 失败，退出码: $LASTEXITCODE"
    }
}

function Resolve-GitExecutable {
    if (Test-Path -LiteralPath $GitExecutable -PathType Leaf) {
        return (Resolve-Path -LiteralPath $GitExecutable).Path
    }

    $command = Get-Command -Name $GitExecutable -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        throw '未检测到 Git，请先安装 Git 后重新执行初始化脚本。'
    }

    if (![string]::IsNullOrWhiteSpace($command.Source)) {
        return $command.Source
    }

    return $GitExecutable
}

function Get-GlobalConfigValue {
    param(
        [string]$GlobalConfigPath,
        [string]$Name
    )

    if (!(Test-Path -LiteralPath $GlobalConfigPath -PathType Leaf)) {
        return ''
    }

    foreach ($line in Get-Content -LiteralPath $GlobalConfigPath) {
        if ($line -match "^\s*$([regex]::Escape($Name)):\s*(.+?)\s*$") {
            return $Matches[1].Trim()
        }
    }

    return ''
}

function Convert-CodeModeValue {
    param([string]$Value)

    switch ($Value) {
        '1' { return 'Client' }
        '2' { return 'Server' }
        '3' { return 'ClientServer' }
        'Client' { return 'Client' }
        'Server' { return 'Server' }
        'ClientServer' { return 'ClientServer' }
        default { return '' }
    }
}

function Read-PackageJson {
    param([string]$PackageJsonPath)

    Assert-FileExists -Path $PackageJsonPath -Description 'package.json'
    return Get-Content -Raw -LiteralPath $PackageJsonPath | ConvertFrom-Json
}

function Resolve-PackageInfo {
    param([string]$PackageName)

    if ([string]::IsNullOrWhiteSpace($PackageName)) {
        throw '主包名为空'
    }

    if (!$PackageName.StartsWith('cn.etetet.', [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "非法主包名: $PackageName"
    }

    $packagesDir = Resolve-ProjectPath 'Packages'
    Assert-DirectoryExists -Path $packagesDir -Description 'Packages 目录'

    $packageDir = Join-Path $packagesDir $PackageName
    if (!(Test-Path -LiteralPath $packageDir -PathType Container)) {
        $match = Get-ChildItem -LiteralPath $packagesDir -Directory -Filter 'cn.etetet.*' |
            Where-Object { $_.Name -ieq $PackageName } |
            Select-Object -First 1

        if ($null -eq $match) {
            throw "未找到主包目录: $packageDir"
        }

        $packageDir = $match.FullName
    }

    $packageJsonPath = Join-Path $packageDir 'package.json'
    $packageJson = Read-PackageJson -PackageJsonPath $packageJsonPath
    $resolvedName = if (![string]::IsNullOrWhiteSpace($packageJson.name)) { [string]$packageJson.name } else { Split-Path -Leaf $packageDir }

    return [pscustomobject]@{
        Name = $resolvedName
        Directory = $packageDir
        PackageJson = $packageJson
        PackageJsonPath = $packageJsonPath
    }
}

function Get-DirectEtDependencies {
    param(
        [object]$PackageJson,
        [string]$MainPackageName
    )

    $dependencies = New-Object 'System.Collections.Generic.List[string]'
    if ($null -eq $PackageJson.dependencies) {
        return @()
    }

    foreach ($property in $PackageJson.dependencies.PSObject.Properties) {
        $name = [string]$property.Name
        if ($name.StartsWith('cn.etetet.', [System.StringComparison]::OrdinalIgnoreCase) -and
            !$name.Equals($MainPackageName, [System.StringComparison]::OrdinalIgnoreCase)) {
            $dependencies.Add($name)
        }
    }

    return @($dependencies | Sort-Object)
}

function Write-MainPackageFile {
    param(
        [string]$MainPackageName,
        [string[]]$Dependencies
    )

    $mainPackagePath = Resolve-ProjectPath 'MainPackage.txt'
    $lines = @($MainPackageName) + @($Dependencies)
    Set-Content -LiteralPath $mainPackagePath -Value $lines -Encoding utf8
    Write-Step "已生成 MainPackage.txt: $mainPackagePath"
}

function Link-MainPackageSolution {
    param([string]$PackageDirectory)

    $sourceSolutionPath = Join-Path $PackageDirectory 'ET.sln'
    $rootSolutionPath = Resolve-ProjectPath 'ET.sln'
    $tempLinkPath = Resolve-ProjectPath 'ET.sln.mainpackage_link_tmp'

    Assert-FileExists -Path $sourceSolutionPath -Description '主包 ET.sln'

    Remove-Item -LiteralPath $tempLinkPath -Force -ErrorAction SilentlyContinue

    try {
        New-Item -ItemType HardLink -Path $tempLinkPath -Target $sourceSolutionPath | Out-Null
        Remove-Item -LiteralPath $rootSolutionPath -Force -ErrorAction SilentlyContinue
        Move-Item -LiteralPath $tempLinkPath -Destination $rootSolutionPath -Force
        Write-Step "已链接主包 ET.sln: $rootSolutionPath"
    }
    catch {
        Remove-Item -LiteralPath $tempLinkPath -Force -ErrorAction SilentlyContinue
        throw "链接主包 ET.sln 失败: $($_.Exception.Message)"
    }
}

function Initialize-MainPackage {
    param([string]$PackageName)

    $packageInfo = Resolve-PackageInfo -PackageName $PackageName
    $dependencies = Get-DirectEtDependencies -PackageJson $packageInfo.PackageJson -MainPackageName $packageInfo.Name
    Write-Step "生成主包: $($packageInfo.Name)，直接依赖数量: $($dependencies.Count)"
    Write-MainPackageFile -MainPackageName $packageInfo.Name -Dependencies $dependencies
    Link-MainPackageSolution -PackageDirectory $packageInfo.Directory
}

function Resolve-InitializationOptions {
    $globalConfigPath = Resolve-ProjectPath 'Packages/com.etetet.init/Resources/GlobalConfig.asset'

    if ([string]::IsNullOrWhiteSpace($script:SceneName)) {
        $script:SceneName = Get-GlobalConfigValue -GlobalConfigPath $globalConfigPath -Name 'SceneName'
    }

    if ([string]::IsNullOrWhiteSpace($script:MainPackage)) {
        if ([string]::IsNullOrWhiteSpace($script:SceneName)) {
            throw '未指定 SceneName 或 MainPackage，且 GlobalConfig.asset 中没有 SceneName'
        }

        $script:MainPackage = "cn.etetet.$($script:SceneName.ToLowerInvariant())"
    }

    if ([string]::IsNullOrWhiteSpace($script:CodeMode)) {
        $rawCodeMode = Get-GlobalConfigValue -GlobalConfigPath $globalConfigPath -Name 'CodeMode'
        $script:CodeMode = Convert-CodeModeValue -Value $rawCodeMode
    }

    if ([string]::IsNullOrWhiteSpace($script:CodeMode)) {
        $script:CodeMode = 'ClientServer'
    }
}

function Assert-Environment {
    Write-Step '检查初始化环境'

    Assert-DirectoryExists -Path $ProjectRoot -Description '项目根目录'
    Assert-DirectoryExists -Path (Resolve-ProjectPath 'Packages') -Description 'Packages 目录'
    $script:ResolvedGitExecutable = Resolve-GitExecutable

    $dotnetInfo = $DotNetSdkVersions
    if ($dotnetInfo.Count -eq 0) {
        $dotnetInfo = & dotnet --list-sdks
        if ($LASTEXITCODE -ne 0) {
            throw 'dotnet --list-sdks 执行失败'
        }
    }

    $hasCompatibleSdk = $false
    foreach ($line in $dotnetInfo) {
        if ($line -match '^(\d+)\.') {
            $major = [int]$Matches[1]
            if ($major -ge 10) {
                $hasCompatibleSdk = $true
            }
        }
    }

    if (!$hasCompatibleSdk) {
        throw ".NET SDK 版本过低，需要 10 或更高版本。当前 SDK:`n$($dotnetInfo -join [Environment]::NewLine)"
    }

    Assert-FileExists -Path (Resolve-ProjectPath 'Packages/cn.etetet.yiuiluban/DontNet~/luban/src/Luban.sln') -Description 'Luban.sln'
    Assert-FileExists -Path (Resolve-ProjectPath 'Packages/com.etetet.init/DotNet~/ET.CodeMode.csproj') -Description 'ET.CodeMode.csproj'
    Assert-FileExists -Path (Resolve-ProjectPath 'Packages/cn.etetet.sourcegenerator/DotNet~/ET.SourceGenerator/ET.SourceGenerator.csproj') -Description 'ET.SourceGenerator.csproj'
    Assert-FileExists -Path (Resolve-ProjectPath 'ET.sln') -Description 'ET.sln'
}

function Restore-SourceGeneratorMeta {
    $metaRelativePath = 'Packages/cn.etetet.sourcegenerator/ET.SourceGenerator.dll.meta'
    $metaPath = Resolve-ProjectPath $metaRelativePath

    if (!(Test-Path -LiteralPath $metaPath -PathType Leaf)) {
        Write-Warning "未找到 SourceGenerator meta 文件，跳过 git 还原: $metaRelativePath"
        return
    }

    Invoke-CheckedCommand -FilePath $script:ResolvedGitExecutable -ArgumentList @('restore', '--', $metaRelativePath) -Description "还原 SourceGenerator meta 文件: $metaRelativePath"
}

function Invoke-InitializationBuilds {
    $solutionDir = $ProjectRoot
    $separator = [string][System.IO.Path]::DirectorySeparatorChar
    if (!$solutionDir.EndsWith($separator, [System.StringComparison]::Ordinal)) {
        $solutionDir = "$solutionDir$separator"
    }

    if (!$SkipLubanBuild) {
        Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('build', (Resolve-ProjectPath 'Packages/cn.etetet.yiuiluban/DontNet~/luban/src/Luban.sln'), '-p:NoWarn=NU1902%3BNU1903%3BCA2022') -Description '编译 Luban'
    }

    if (!$SkipToolBuild) {
        Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('build', (Resolve-ProjectPath 'Packages/com.etetet.init/DotNet~/ET.CodeMode.csproj'), "-p:SolutionDir=$solutionDir") -Description '编译 ET.CodeMode'
        Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('build', (Resolve-ProjectPath 'Packages/cn.etetet.sourcegenerator/DotNet~/ET.SourceGenerator/ET.SourceGenerator.csproj'), "-p:SolutionDir=$solutionDir") -Description '编译 ET.SourceGenerator'
    }

    if (!$SkipCodeMode) {
        Assert-FileExists -Path (Resolve-ProjectPath 'Bin/ET.CodeMode.dll') -Description 'Bin/ET.CodeMode.dll'
        Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @((Resolve-ProjectPath 'Bin/ET.CodeMode.dll'), "--CodeMode=$CodeMode") -Description "执行 ET.CodeMode: $CodeMode"
    }

    if ($BuildSolution -and !$SkipFullBuild) {
        Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('build', (Resolve-ProjectPath 'ET.sln')) -Description '编译 ET.sln'
    }
}

Push-Location $ProjectRoot
try {
    $ProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
    Resolve-InitializationOptions

    Write-Step "项目根目录: $ProjectRoot"
    Write-Step "主包: $MainPackage"
    Write-Step "CodeMode: $CodeMode"

    Initialize-MainPackage -PackageName $MainPackage

    if ($GenerateMainPackageOnly) {
        Write-Step '已按参数仅生成主包，跳过后续构建'
        exit 0
    }

    Assert-Environment
    Invoke-InitializationBuilds
    Restore-SourceGeneratorMeta

    Write-Step '命令行初始化流程已完成'
    Write-Host ''
    Write-Host '仍需在 Unity/Rider 中确认的人工步骤：'
    Write-Host '- Unity 版本使用 2022.3.62'
    Write-Host '- 已安装 DOTween 和 Odin 插件'
    Write-Host '- 已安装 IL2CPP 模块'
    Write-Host '- Unity External ScriptEditor 选择 Rider，并勾选 Generate .csproj files for 的前两个选项'
    Write-Host '- 打开 Packages/ET.StateSync/Scenes/Init 场景后进入 Play'
}
finally {
    Pop-Location
}
