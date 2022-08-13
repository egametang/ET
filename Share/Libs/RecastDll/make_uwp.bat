mkdir build_uwp & pushd build_uwp
cmake -G "Visual Studio 16 2019" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp --config Release
md Plugins\WSA\x86
copy /Y build_uwp\Release\RecastDll.dll Plugins\WSA\x86\RecastDll.dll
rmdir /S /Q build_uwp

mkdir build_uwp64 & pushd build_uwp64
cmake -G "Visual Studio 16 2019 Win64" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp64 --config Release
md Plugins\WSA\x64
copy /Y build_uwp64\Release\RecastDll.dll Plugins\WSA\x64\RecastDll.dll
rmdir /S /Q build_uwp64

mkdir build_uwp_arm & pushd build_uwp_arm
cmake -G "Visual Studio 16 2019 ARM" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp_arm --config Release
md Plugins\WSA\ARM
copy /Y build_uwp_arm\Release\RecastDll.dll Plugins\WSA\ARM\RecastDll.dll
rmdir /S /Q build_uwp_arm

pause
