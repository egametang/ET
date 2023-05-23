#!/bin/zsh
WORKSPACE=../..

GEN_CLIENT=Luban.ClientServer/Luban.ClientServer.dll
CONF_ROOT=${WORKSPACE}/Unity/Assets/Config/Excel
OUTPUT_CODE_DIR=${WORKSPACE}/Unity/Assets/Scripts/Codes/Model/Generate
OUTPUT_DATA_DIR=${WORKSPACE}/Config/Excel
OUTPUT_JSON_DIR=${WORKSPACE}/Config/Json
CONFIG_FOLDER=$1
  
#ClientServer
echo =====================================================================================================
echo ======================= ClientServer GameConfig ==========================
/usr/local/share/dotnet/dotnet ${GEN_CLIENT} --template_search_path CustomTemplate -j cfg --\
 -d ${CONF_ROOT}/Defines/__root__.xml \
 --input_data_dir ${CONF_ROOT}/Datas \
 --output_data_dir ${OUTPUT_DATA_DIR}/cs/GameConfig \
 --output:exclude_tables StartMachineConfigCategory,StartProcessConfigCategory,StartSceneConfigCategory,StartZoneConfigCategory \
 --gen_types data_bin \
 -s all

if [ $? -eq 1 ]; then
    exit 1
fi
   
echo ======================= ClientServer StartConfig ${CONFIG_FOLDER} ==========================
/usr/local/share/dotnet/dotnet ${GEN_CLIENT} --template_search_path CustomTemplate -j cfg --\
 -d ${CONF_ROOT}/Defines/__root__.xml \
 --input_data_dir ${CONF_ROOT}/Datas/StartConfig/${CONFIG_FOLDER} \
 --output_data_dir ${OUTPUT_DATA_DIR}/cs/StartConfig/${CONFIG_FOLDER} \
 --gen_types data_bin \
 -s all

if [ $? -eq 1 ]; then
    exit 1
fi
   
echo ======================= ClientServer Code ==========================
/usr/local/share/dotnet/dotnet ${GEN_CLIENT} --template_search_path CustomTemplate -j cfg --\
 -d ${CONF_ROOT}/Defines/__root__.xml \
 --input_data_dir ${CONF_ROOT}/Datas \
 --output_code_dir ${OUTPUT_CODE_DIR}/ClientServer/Config \
 --gen_types code_cs_bin \
 -s all

if [ $? -eq 1 ]; then
    exit 1
fi
   
#ClientServer Json
echo =====================================================================================================
echo ======================= ClientServer GameConfig Json ==========================
/usr/local/share/dotnet/dotnet ${GEN_CLIENT} --template_search_path CustomTemplate -j cfg --\
 -d ${CONF_ROOT}/Defines/__root__.xml \
 --input_data_dir ${CONF_ROOT}/Datas \
 --output_data_dir ${OUTPUT_JSON_DIR}/cs/GameConfig \
 --output:exclude_tables StartMachineConfigCategory,StartProcessConfigCategory,StartSceneConfigCategory,StartZoneConfigCategory \
 --gen_types data_json \
 -s all
   
if [ $? -eq 1 ]; then
    exit 1
fi
   
echo ======================= ClientServer StartConfig ${CONFIG_FOLDER} Json ==========================
/usr/local/share/dotnet/dotnet ${GEN_CLIENT} --template_search_path CustomTemplate -j cfg --\
 -d ${CONF_ROOT}/Defines/__root__.xml \
 --input_data_dir ${CONF_ROOT}/Datas/StartConfig/${CONFIG_FOLDER} \
 --output_data_dir ${OUTPUT_JSON_DIR}/cs/StartConfig/${CONFIG_FOLDER} \
 --gen_types data_json \
 -s all

if [ $? -eq 1 ]; then
    exit 1
fi