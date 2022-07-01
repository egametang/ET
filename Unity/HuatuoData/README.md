# 使用说明

这个目录主要包含

- huatuo相关仓库
- 本地il2cpp目录
- 其他生成的目录

## 安装huatuo

正常情况下，安装huatuo需要替换Unity安装目录下libil2cpp目录为huatuo版本的实现，但Unity允许使用环境变量UNITY_IL2CPP_PATH自定义%IL2CPP_PATH%的位置。
因此我们不修改原始的il2cpp目录，直接在本地创建huatuo版本的il2cpp，并让环境变量指向它

安装流程

- 运行 init_huatuo_repos.bat 或 .sh 文件从远程接取 huatuo_il2cpp和huatuo仓库
- 根据你的版本，运行相应的 set_version_xxx.bat 或.sh 文件将 huatuo_il2cpp切换到合适的分支
- 修改 init_local_il2cpp_data.bat(或.sh)文件中语句 `set IL2CPP_PATH=<你的Unity editor的il2cpp目录的路径>`
- 运行 init_local_il2cpp_data.bat 或.sh 文件 创建本地il2cpp目录，即 LocalIl2CppData 目录。

如果看到初始化成功，表示运行成功。否则请参照文档，对应 .bat或.sh文件，自己查找错误原因。

