# SyncCodesService
ET里自动刷新Codes的工具，目前仅适用于ET6.0的目录结构。

需要用命令行bat文件打开，参考命令行：

@set WORKSPACE=D:\ET

@set SERVICE=dotnet  %WORKSPACE%\Tools\SyncCodesService\SyncCodesService\output\Debug\net6.0\SyncCodesService.dll

@%SERVICE% %WORKSPACE%\Unity