@echo off
rem "%windir%\Microsoft.NET\Framework\v4.0.30319\regasm.exe" /nologo /unregister "%1"

rem "%ProgramFiles%\Microsoft SDKs\Windows\v7.0A\Bin\gacutil.exe" /nologo /f /i "%1"
rem "%windir%\Microsoft.NET\Framework\v4.0.30319\regasm.exe" /nologo /CodeBase "%1"

goto BuildEventOK

:BuildEventFailed
echo POSTBUILDSTEP FAILED
exit 1

:BuildEventOK
echo POSTBUILDSTEP COMPLETED OK
exit 0