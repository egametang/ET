#!/bin/bash

# 更新 huatuo主仓库. 所有il2cpp版本共享一个分支
cd huatuo_repo
git pull
cd ..

# 更新 il2cpp_huatuo 仓库，必须精确对应版本
cd il2cpp_huatuo_repo
git pull


