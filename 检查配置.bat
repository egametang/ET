set WORKSPACE=.

set GEN_CLIENT=dotnet %WORKSPACE%\Tools\Luban.ClientServer\Luban.ClientServer.dll

set CONF_ROOT=%WORKSPACE%\Config

set ADJUST=dotnet  %WORKSPACE%\Tools\AdjustETCsprojs\AdjustETCsprojs.dll

%GEN_CLIENT% -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_data_dir %CONF_ROOT%\Output_Json\Check ^
 --gen_types data_json ^
 -s all

%ADJUST% refresh %WORKSPACE%\Unity

pause