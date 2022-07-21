@set WORKSPACE=.

@set ADJUST=dotnet  %WORKSPACE%\Tools\SyncCodesService\SyncCodesService\output\Debug\net6.0\SyncCodesService.dll

@%ADJUST% %WORKSPACE%\Unity
pause