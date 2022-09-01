#!/bin/bash

# 设置默认分支为2020.3.33，避免很多人忘了切分支
IL2CPP_BRANCH=$1

rm -rf hybridclr_repo
# clone hybridclr仓库,国内推荐用 gitee
# git clone https://github.com/focus-creative-games/hybridclr
git clone --depth=1 https://gitee.com/focus-creative-games/hybridclr hybridclr_repo

rm -rf il2cpp_plus_repo
# git clone https://github.com/focus-creative-games/il2cpp_plus
git clone --depth=1 -b $IL2CPP_BRANCH https://gitee.com/focus-creative-games/il2cpp_plus il2cpp_plus_repo


# 请修改为你所用Unity的il2cpp目录
# 一般像这样 
# C:\Program Files\Unity\Hub\Editor\2020.3.33f1c2\Editor\Data\il2cpp
# /Applications/Unity/Hub/Editor/2020.3.33f1/Unity.app/Contents/il2cpp
IL2CPP_PATH=$2

if [ ! -d "$IL2CPP_PATH" ] ; then
    echo "你未指定正确的il2cpp路径"
    exit 1
fi

LOCAL_IL2CPP_DATA=LocalIl2CppData

if [ ! -d "$LOCAL_IL2CPP_DATA" ]; then
    mkdir $LOCAL_IL2CPP_DATA
fi


# Unity 打包时允许使用环境变量UNITY_IL2CPP_PATH自定义%IL2CPP_PATH%的位置
# 但同时又要求同级目录包含MonoBleedingEdge，因此需要拷贝这两个目录

# 拷贝 MonoBleedingEdge 目录
MBE=$LOCAL_IL2CPP_DATA/MonoBleedingEdge
rm -rf "$MBE"
cp -r "$IL2CPP_PATH/../MonoBleedingEdge" $MBE


# 拷贝il2cpp目录
IL2CPP=$LOCAL_IL2CPP_DATA/il2cpp
rm -rf "$IL2CPP"
cp -r "$IL2CPP_PATH" "$IL2CPP"


# 接下来替换 il2cpp目录下的libil2cpp为 hybridclr修改后的版本
# 需要使用 {https://gitee.com/focus-creative-games/il2cpp_plus}/libil2cpp 替换 il2cpp/libil2cpp目录
# 需要使用 {https://gitee.com/focus-creative-games/hybridclr}/hybridclr 添加到 il2cpp/libil2cpp目录下，即il2cpp/libil2cpp/hybridclr

HYBRIDCLR_REPO_DIR=hybridclr_repo
IL2CPP_PLUS_REPO_DIR=il2cpp_plus_repo

LIBIL2CPP_PATH=$LOCAL_IL2CPP_DATA/il2cpp/libil2cpp
rm -rf "$LIBIL2CPP_PATH"

cp -r "$IL2CPP_PLUS_REPO_DIR/libil2cpp" "$LIBIL2CPP_PATH"
cp -r "$HYBRIDCLR_REPO_DIR/hybridclr" "$LIBIL2CPP_PATH/hybridclr"

# 务必清除缓存，不然build仍然使用旧版本。
# 只影响直接build的情况，不影响导出工程的情形。

echo 清除 Library/Il2cppBuildCache 缓存目录
rm -rf ../Library/Il2cppBuildCache

echo 初始化成功

read -p "Press enter to continue" -t 10
