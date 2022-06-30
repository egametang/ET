#!/bin/bash

# clone huatuo仓库,国内推荐用 gitee
# git clone https://github.com/focus-creative-games/huatuo
git clone https://gitee.com/focus-creative-games/huatuo huatuo_repo

# git clone https://github.com/pirunxi/il2cpp_huatuo
git clone https://gitee.com/juvenior/il2cpp_huatuo il2cpp_huatuo_repo


# 设置默认分支为2020.3.33，避免很多人忘了切分支
DEFAULT_VERSION=2020.3.33

cd il2cpp_huatuo_repo

git switch $DEFAULT_VERSION

