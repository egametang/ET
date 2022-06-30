#!/bin/bash

# 请修改为你所用Unity的il2cpp目录
# 一般像这样 C:\Program Files\Unity\Hub\Editor\2020.3.33f1c2\Editor\Data\il2cpp

IL2CPP_PATH='/c/Program Files/Unity/Hub/Editor/2020.3.33f1c2/Editor/Data/il2cpp'

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
if [ ! -d "$MBE" ]; then
    cp -r "$IL2CPP_PATH/../MonoBleedingEdge" $MBE
fi


# 拷贝il2cpp目录
IL2CPP=$LOCAL_IL2CPP_DATA/il2cpp
if [ ! -d "$IL2CPP" ]; then
    cp -r "$IL2CPP_PATH" "$IL2CPP"
fi

# 接下来替换 il2cpp目录下的libil2cpp为 huatuo修改后的版本
# 需要使用 {https://gitee.com/juvenior/il2cpp_huatuo}/libil2cpp 替换 il2cpp/libil2cpp目录
# 需要使用 {https://gitee.com/focus-creative-games/huatuo}/huatuo 添加到 il2cpp/libil2cpp目录下，即il2cpp/libil2cpp/huatuo

HUATUO_REPO=huatuo_repo

if [ ! -d "$HUATUO_REPO" ] ; then
    echo 未安装huatuo https://gitee.com/focus-creative-games/huatuo,请运行 init_huatuo_repos.bat or init_huatuo_repos.sh
    exit 1
fi

IL2CPP_HUATUO_REPO=il2cpp_huatuo_repo
if [ ! -d "$IL2CPP_HUATUO_REPO" ]; then
    echo 未安装il2cpp_huatuo https://gitee.com/juvenior/il2cpp_huatuo ,请运行 init_huatuo_repos.bat or init_huatuo_repos.sh
    exit 1
fi

LIBIL2CPP_PATH=$LOCAL_IL2CPP_DATA/il2cpp/libil2cpp
rm -rf "$LIBIL2CPP_PATH"

cp -r "$IL2CPP_HUATUO_REPO/libil2cpp" "$LIBIL2CPP_PATH"
cp -r "$HUATUO_REPO/huatuo" "$LIBIL2CPP_PATH/huatuo"

# 务必清除缓存，不然build仍然使用旧版本。
# 只影响直接build的情况，不影响导出工程的情形。

echo 清除 Library/Il2cppBuildCache 缓存目录
rm -rf ../Library/Il2cppBuildCache

echo 初始化成功
