mkdir build64 & pushd build64
cmake -G "Visual Studio 16 2019" -A x64 ..
popd
cmake --build build64 --config Release
md Plugins\x86_64
copy /Y build64\Release\RecastDll.dll Plugins\x86_64\RecastDll.dll
rmdir /S /Q build64
pause
