
set WORKSPACE=..\..

set GEN_CLIENT=Luban.ClientServer\Luban.ClientServer.exe
set CONF_ROOT=%WORKSPACE%\Unity\Assets\Config\Excel
set OUTPUT_CODE_DIR=%WORKSPACE%\Unity\Assets\Scripts\Codes\Model\Generate
set OUTPUT_DATA_DIR=%WORKSPACE%\Config\Excel
set OUTPUT_JSON_DIR=%WORKSPACE%\Config\Json
set CONFIG_FOLDER=%1

echo ======================= Server GameConfig ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_data_dir %OUTPUT_DATA_DIR%\s\GameConfig ^
 --output:exclude_tables StartMachineConfigCategory,StartProcessConfigCategory,StartSceneConfigCategory,StartZoneConfigCategory ^
 --output:exclude_tags c ^
 --gen_types data_bin ^
 -s server
 
if %ERRORLEVEL% NEQ 0 exit

echo ======================= Server StartConfig %CONFIG_FOLDER% ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas\StartConfig\%CONFIG_FOLDER% ^
 --output_data_dir %OUTPUT_DATA_DIR%\s\StartConfig\%CONFIG_FOLDER% ^
 --gen_types data_bin ^
 -s server
 
if %ERRORLEVEL% NEQ 0 exit

echo ======================= Server Code ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir %OUTPUT_CODE_DIR%\Server\Config ^
 --gen_types code_cs_bin ^
 -s server

if %ERRORLEVEL% NEQ 0 exit

echo ======================= Server GameConfig Json ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_data_dir %OUTPUT_JSON_DIR%\s\GameConfig ^
 --output:exclude_tables StartMachineConfigCategory,StartProcessConfigCategory,StartSceneConfigCategory,StartZoneConfigCategory ^
 --output:exclude_tags c ^
 --gen_types data_json ^
 -s server
 
if %ERRORLEVEL% NEQ 0 exit

echo ======================= Server StartConfig %CONFIG_FOLDER% Json ==========================
%GEN_CLIENT% --template_search_path CustomTemplate -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas\StartConfig\Localhost ^
 --output_data_dir %OUTPUT_JSON_DIR%\s\StartConfig\Localhost ^
 --gen_types data_json ^
 -s server

if %ERRORLEVEL% NEQ 0 exit
