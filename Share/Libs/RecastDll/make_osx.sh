mkdir -p build_osx && cd build_osx
cmake -GXcode ../
cd ..
cmake --build build_osx --config Release
mkdir -p Plugins/RecastDll.bundle/Contents/MacOS/
cp build_osx/Release/RecastDll.bundle/Contents/MacOS/RecastDll Plugins/RecastDll.bundle/Contents/MacOS/RecastDll
rm -rf build_osx
