@echo off
set source=Library\ETCompileRecord\ScriptVersion.etrec
set destination=Library\ETCompileRecord\GeneratedDllVersion.etrec

if exist "%source%" (
    copy /Y "%source%" "%destination%"
)