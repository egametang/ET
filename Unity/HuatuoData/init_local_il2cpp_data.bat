

rem replace with right Unity Editor Install path
set IL2CPP_PATH=D:\Study\Unity\UnityEditor\2021.3.5f1c1\Editor\Data\il2cpp

if not exist "%IL2CPP_PATH%" (
    echo "please set correct %IL2CPP_PATH% value"
    goto EXIT
)

set LOCAL_IL2CPP_DATA=LocalIl2CppData

if not exist %LOCAL_IL2CPP_DATA% (
    mkdir %LOCAL_IL2CPP_DATA%
)

rem need copdy MonoBleedingEdge
set MBE=%LOCAL_IL2CPP_DATA%\MonoBleedingEdge
if not exist %MBE% (
    xcopy /q /i /e "%IL2CPP_PATH%\..\MonoBleedingEdge" %MBE%
)


rem copy il2cpp
set IL2CPP=%LOCAL_IL2CPP_DATA%\il2cpp
if not exist %IL2CPP% (
    xcopy /q /i /e "%IL2CPP_PATH%" %IL2CPP%
)

set HUATUO_REPO=huatuo_repo

if not exist %HUATUO_REPO% (
    echo not install huatuo https://gitee.com/focus-creative-games/huatuo, please run init_huatuo_repos.bat or init_huatuo_repos.sh
    goto EXIT 
)

set IL2CPP_HUATUO_REPO=il2cpp_huatuo_repo
if not exist %IL2CPP_HUATUO_REPO% (
    echo not install il2cpp_huatuo https://gitee.com/juvenior/il2cpp_huatuo , please run init_huatuo_repos.bat or init_huatuo_repos.sh
    goto EXIT 
)

set LIBIL2CPP_PATH=%LOCAL_IL2CPP_DATA%\il2cpp\libil2cpp
rd /s /q %LIBIL2CPP_PATH%

xcopy /q /i /e %IL2CPP_HUATUO_REPO%\libil2cpp %LIBIL2CPP_PATH%
xcopy /q /i /e %HUATUO_REPO%\huatuo %LIBIL2CPP_PATH%\huatuo

rem clean il2cpp build cache
set IL2CPP_CACHE=..\Library\Il2cppBuildCache
echo clean %IL2CPP_CACHE%
if exist "%IL2CPP_CACHE%" rd /s /q "%IL2CPP_CACHE%"

echo succ

:EXIT

PAUSE