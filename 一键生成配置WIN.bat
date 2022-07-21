set WORKSPACE=.

set GEN_CLIENT=dotnet %WORKSPACE%\Tools\Luban.ClientServer\Luban.ClientServer.dll

set CONF_ROOT=%WORKSPACE%\Config

@ECHO =======================SERVER==========================
%GEN_CLIENT% --template_search_path %CONF_ROOT%\CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %WORKSPACE%\Server\Model\Generate\Config ^
 --output_data_dir %WORKSPACE%\Server\ConfigBin ^
 --gen_types code_cs_bin,data_bin ^
 --external:selectors dotnet_cs ^
 -s server


@ECHO =======================CLIENT==========================
%GEN_CLIENT% --template_search_path %CONF_ROOT%\CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %WORKSPACE%\Unity\Codes\Model\Generate\Config ^
 --output_data_dir %WORKSPACE%\Unity\Assets\Bundles\ConfigBin ^
 --gen_types code_cs_bin,data_bin ^
 --external:selectors dotnet_cs,unity_cs ^
 -s client

%GEN_CLIENT% -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_data_dir %CONF_ROOT%\Output_Json\Server ^
 --gen_types data_json ^
 -s server

%GEN_CLIENT% -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_data_dir %CONF_ROOT%\Output_Json\Client ^
 --gen_types data_json ^
 -s client

pause