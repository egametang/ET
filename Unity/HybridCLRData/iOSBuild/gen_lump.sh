echo '====================================================================='
echo 'gen lump'
echo '$HUATUO_IL2CPP_SOURCE_DIR='${HUATUO_IL2CPP_SOURCE_DIR}   #/Applications/Unity/Unity.app/Contents/il2cpp/

GEN_SOURCE_DIR=$1
BASE_DIR=${HUATUO_IL2CPP_SOURCE_DIR}/libil2cpp
echo base dir: ${BASE_DIR}
echo " "
#BASE_DIR=${HUATUO_IL2CPP_SOURCE_DIR}/libil2cpp
function SearchCppFile()
{
    for f in $(ls $1)
    do
        SUB_DIR=$1/$f
        if [ -d ${SUB_DIR} ]; then
            SearchCppFile ${SUB_DIR}
        fi
    done

    CPP_FILE_NUM=`ls -l $1/ | grep "\.cpp$"|wc -l`
    if (( ${CPP_FILE_NUM} > 0 ))
    then
        for f in $1/*.cpp
        do
            echo "#include \""$f"\"" >> ${OUTPUT_FILE_NAME}
        done
    fi

    MM_FILE_NUM=`ls -l $1/ | grep "\.mm$"|wc -l`
    if (( ${MM_FILE_NUM} > 0 ))
    then
        for f in $1/*.mm
        do
            echo "#include \""$f"\"" >> ${OBJECTIVE_FILE_NAME}
        done
    fi
}

rm -rf ${GEN_SOURCE_DIR}/lump_cpp
rm -rf ${GEN_SOURCE_DIR}/lump_mm
mkdir ${GEN_SOURCE_DIR}/lump_cpp
mkdir ${GEN_SOURCE_DIR}/lump_mm

OBJECTIVE_FILE_NAME=${GEN_SOURCE_DIR}/lump_mm/lump_libil2cpp_ojective.mm
echo "#include \"${BASE_DIR}/il2cpp-config.h\"" > ${OBJECTIVE_FILE_NAME}
echo gen file: ${OBJECTIVE_FILE_NAME}

for FOLDER in hybridclr vm pch utils vm-utils codegen metadata os debugger mono gc icalls
do
    OUTPUT_FILE_NAME=${GEN_SOURCE_DIR}/lump_cpp/lump_libil2cpp_${FOLDER}.cpp
    echo "#include \"${BASE_DIR}/il2cpp-config.h\"" > ${OUTPUT_FILE_NAME}
    if  [ $FOLDER = hybridclr ] || [ $FOLDER = vm ]
    then
        echo "#include \"${BASE_DIR}/codegen/il2cpp-codegen.h\"" >> ${OUTPUT_FILE_NAME}
    fi
    SearchCppFile ${BASE_DIR}/${FOLDER}
    echo gen file: ${OUTPUT_FILE_NAME}
done

echo gen done.
echo '====================================================================='
echo " "


