@echo off

set PATH=%PATH%;%WINDIR%\system32

rem set default branch
set IL2CPP_BRANCH=%~1

if exist hybridclr_repo rd /s /q hybridclr_repo
rem git clone https://github.com/focus-creative-games/hybridclr
git clone --depth=1 https://gitee.com/focus-creative-games/hybridclr hybridclr_repo

if exist il2cpp_plus_repo rd /s /q il2cpp_plus_repo
rem git clone https://github.com/focus-creative-games/il2cpp_hybridclr
git clone --depth=1 -b %IL2CPP_BRANCH% https://gitee.com/focus-creative-games/il2cpp_plus il2cpp_plus_repo


rem replace with right Unity Editor Install path
set IL2CPP_PATH=%~2

if not exist "%IL2CPP_PATH%" (
    echo "please set correct IL2CPP_PATH value"
    goto EXIT
)

set LOCAL_IL2CPP_DATA=LocalIl2CppData

if not exist %LOCAL_IL2CPP_DATA% (
    mkdir %LOCAL_IL2CPP_DATA%
)

rem need copdy MonoBleedingEdge
set MBE=%LOCAL_IL2CPP_DATA%\MonoBleedingEdge
if exist %MBE% (
    rd /s /q %MBE%
)
xcopy /q /i /e "%IL2CPP_PATH%\..\MonoBleedingEdge" %MBE%

rem copy il2cpp
set IL2CPP=%LOCAL_IL2CPP_DATA%\il2cpp
if exist %IL2CPP% (
    rd /s /q %IL2CPP%
)
xcopy /q /i /e "%IL2CPP_PATH%" %IL2CPP%

set HYBRIDCLR_REPO_DIR=hybridclr_repo

set IL2CPP_PLUS_REPO_DIR=il2cpp_plus_repo

set LIBIL2CPP_PATH=%LOCAL_IL2CPP_DATA%\il2cpp\libil2cpp
rd /s /q %LIBIL2CPP_PATH%

xcopy /q /i /e %IL2CPP_PLUS_REPO_DIR%\libil2cpp %LIBIL2CPP_PATH%
xcopy /q /i /e %HYBRIDCLR_REPO_DIR%\hybridclr %LIBIL2CPP_PATH%\hybridclr

rem clean il2cpp build cache
set IL2CPP_CACHE=..\Library\Il2cppBuildCache
echo clean %IL2CPP_CACHE%
if exist "%IL2CPP_CACHE%" rd /s /q "%IL2CPP_CACHE%"

echo succ

:EXIT

timeout /t 10
