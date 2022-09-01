#!/bin/bash

export HUATUO_IL2CPP_SOURCE_DIR=$(pushd ../LocalIl2CppData/il2cpp > /dev/null && pwd && popd > /dev/null)
export IPHONESIMULATOR_VERSION=

rm -rf build

mkdir build
cd build
cmake ..
make -j4

if [ -f "libil2cpp.a" ]
then
	echo 'build succ'
else
    echo "build fail"
    exit 1
fi
