mkdir -p build_ios && cd build_ios
cmake -DCMAKE_TOOLCHAIN_FILE=../cmake/iOS.cmake  -GXcode ../
cd ..
cmake --build build_ios --config Release
mkdir -p Plugins/iOS/
exist_armv7=`lipo -info build_ios/Release-iphoneos/libkcp.a | grep armv7 | wc -l`
exist_arm64=`lipo -info build_ios/Release-iphoneos/libkcp.a | grep arm64 | wc -l`
if [ $[exist_armv7] -eq 0 ]; then
	echo "** ERROR **: No support for armv7, maybe XCode version is to high, use manual_build_ios instead!"
elif [ $[exist_arm64] -eq 0 ]; then
	echo "** ERROR ** : No support for arm64, maybe XCode version is to high, use manual_build_ios instead!"
else
	cp build_ios/Release-iphoneos/libRecastDll.a Plugins/iOS/libRecastDll.a
    rm -rf build_io
fi
