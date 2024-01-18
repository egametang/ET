@echo off
set source=Library\ETCompileRecord\ScriptVersion.etrec
set destination=Library\ETCompileRecord\GeneratedDllVersion.etrec
if exist "%source%" (
    copy /Y "%source%" "%destination%"
)

for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%" & set "Min=%dt:~10,2%" & set "Sec=%dt:~12,2%"
set "stamp=%YYYY%-%MM%-%DD% %HH%:%Min%:%Sec%"
echo %stamp% > Library\ETCompileRecord\IDECompileTime.etrec