# 使用说明

这个目录主要包含

- huatuo相关仓库
- 本地il2cpp目录
- 其他生成的目录

## 安装huatuo

正常情况下，安装huatuo需要替换Unity安装目录下libil2cpp目录为huatuo版本的实现，但Unity允许使用环境变量UNITY_IL2CPP_PATH自定义%IL2CPP_PATH%的位置。
因此我们不修改原始的il2cpp目录，直接在本地创建huatuo版本的il2cpp，并让环境变量指向它

安装流程

- 酌情修改 init_local_il2cpp_data.bat(或.sh)文件中代码
  - `set IL2CPP_BRANCH=2020.3.33` 改成你的版本（目前只有2020.3.33或2021.3.1）
  - `set IL2CPP_PATH=<你的Unity editor的il2cpp目录的路径>` 改成你的Unity安装目录
- 运行 init_local_il2cpp_data.bat 或.sh 文件 创建本地il2cpp目录，即 LocalIl2CppData 目录。

如果看到初始化成功，表示运行成功。否则请参照文档，对应 .bat或.sh文件，自己查找错误原因。

