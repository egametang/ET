@echo off  
call %1
MSBuild %2 /t:Build /p:Platform=AnyCPU /p:Configuration=Debug