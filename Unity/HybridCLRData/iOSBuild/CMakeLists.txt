# file: CMakeList.txt
cmake_minimum_required(VERSION 3.0)
set(CMAKE_OSX_DEPLOYMENT_TARGET iOS)

set(TMP $ENV{HUATUO_IL2CPP_SOURCE_DIR})
if ( NOT TMP )
    message(FATAL_ERROR "需要设置环境变量: HUATUO_IL2CPP_SOURCE_DIR")
else()
    message(STATUS "unity il2cpp 路径为: ${TMP}")
endif()

set(SDK_VERSION $ENV{IPHONESIMULATOR_VERSION})
if ( SDK_VERSION )
    message(STATUS "使用iPhoneSimulator版本:" ${SDK_VERSION})
else()
    message(STATUS "当前使用默认版本的iPhoneSimulator，可以通过设置环境变量IPHONESIMULATOR_VERSION，指定版本")
endif()

project(il2cpp)
execute_process(COMMAND sh gen_lump.sh ${PROJECT_BINARY_DIR} WORKING_DIRECTORY ${CMAKE_SOURCE_DIR})

add_subdirectory(external)
add_subdirectory(objective)


set(IL2CPP_SOURCE_DIR $ENV{HUATUO_IL2CPP_SOURCE_DIR})

message(STATUS "il2cpp project, binary dir: " ${CMAKE_BINARY_DIR})
message(STATUS "il2cpp project, build dir: " ${PROJECT_BINARY_DIR})
message(STATUS "il2cpp project, il2cpp source dir: " ${IL2CPP_SOURCE_DIR})


SET_PROPERTY(GLOBAL PROPERTY USE_FOLDERS ON)
find_program(CLANG_EXECUTABLE NAMES clang clang-7 clang-8 clang-9 clang-10)
if (NOT CLANG_EXECUTABLE)
    message(FATAL_ERROR "Cannot find any clang executable.")
endif()


set(CMAKE_OSX_SYSROOT /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator${SDK_VERSION}.sdk)

#set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_CXX_COMPILER clang++)
set(CMAKE_C_COMPILER clang)
#add_compile_options(-x objective-c++)

# 以下命令为复制的Xcode生成中的命令
set(CMAKE_CXX_FLAGS "-target arm64-apple-ios11.0 -fmessage-length=0 -fdiagnostics-show-note-include-stack -fmacro-backtrace-limit=0 -std=gnu++1z -fmodules -fmodules-prune-interval=86400 -fmodules-prune-after=345600 -fmodules-ignore-macro=IL2CPP_TARGET_IOS=1 -fembed-bitcode -fmodules-ignore-macro=BASELIB_DYNAMICLIBRARY=1 -fmodules-ignore-macro=BASELIB_INLINE_NAMESPACE=il2cpp_baselib -fmodules-ignore-macro=AARCH64 -fmodules-ignore-macro=__aarch64__ -fmodules-ignore-macro=IL2CPP_SUPPORT_THREADS -fmodules-ignore-macro=IL2CPP_THREADS_PTHREAD -Wnon-modular-include-in-framework-module -Werror=non-modular-include-in-framework-module -Wno-trigraphs -fpascal-strings -Os -fno-common -Wno-missing-field-initializers -Wno-missing-prototypes -Werror=return-type -Wdocumentation -Wunreachable-code -Wquoted-include-in-framework-header -Werror=deprecated-objc-isa-usage -Werror=objc-root-class -Wno-non-virtual-dtor -Wno-overloaded-virtual -Wno-exit-time-destructors -Wno-missing-braces -Wparentheses -Wswitch -Wno-unused-function -Wno-unused-label -Wno-unused-parameter -Wno-unused-variable -Wunused-value -Wempty-body -Wuninitialized -Wconditional-uninitialized -Wno-unknown-pragmas -Wno-shadow -Wno-four-char-constants -Wno-conversion -Wconstant-conversion -Wint-conversion -Wbool-conversion -Wenum-conversion -Wno-float-conversion -Wnon-literal-null-conversion -Wobjc-literal-conversion -Wshorten-64-to-32 -Wno-newline-eof -Wno-c++11-extensions -DIL2CPP_TARGET_IOS=1 -isysroot ${CMAKE_OSX_SYSROOT} -fasm-blocks -fstrict-aliasing -Wdeprecated-declarations -Winvalid-offsetof -g -Wno-sign-conversion -Winfinite-recursion -Wmove -Wcomma -Wblock-capture-autoreleasing -Wstrict-prototypes -Wrange-loop-analysis -Wno-semicolon-before-method-body -Wunguarded-availability ")


add_definitions(-DIL2CPP_TARGET_IOS=1)
add_definitions(-DBASELIB_DYNAMICLIBRARY=1)
add_definitions(-DBASELIB_INLINE_NAMESPACE=il2cpp_baselib)
add_definitions(-DAARCH64)
add_definitions(-D__aarch64__)
add_definitions(-DIL2CPP_SUPPORT_THREADS)
add_definitions(-DIL2CPP_THREADS_PTHREAD)
#add_definitions(-DCMAKE_C_COMPILER=/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang)
#add_definitions(-DCMAKE_CXX_COMPILER=/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang++)
#add_definitions(-DIL2CPP_TARGET_DARWIN)
add_definitions(-DIL2CPP_PLATFORM_SUPPORTS_TIMEZONEINFO)

add_definitions(-MMD)
add_definitions(-MT dependencies)


include_directories(${IL2CPP_SOURCE_DIR}/libil2cpp/)
include_directories(${IL2CPP_SOURCE_DIR}/external/)
include_directories(${IL2CPP_SOURCE_DIR}/external/baselib/Include/)
include_directories(${IL2CPP_SOURCE_DIR}/external/bdwgc/include/)
include_directories(${IL2CPP_SOURCE_DIR}/external/baselib/Platforms/OSX/Include/)

# 修改为本机对应SDK路径
include_directories(SYSTEM /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk/usr/include/c++/v1)
include_directories(SYSTEM /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk/usr/include)

aux_source_directory(${PROJECT_BINARY_DIR}/lump_cpp LUMP_SOURCE_LIST)
aux_source_directory(${IL2CPP_SOURCE_DIR}/libil2cpp LIBIL2CPP_SOURCE_LIST)

#find_library(zlib ${PROJECT_SOURCE_DIR}/external)
add_library(il2cpp_original STATIC ${LIBIL2CPP_SOURCE_LIST} ${LUMP_SOURCE_LIST})
add_dependencies(il2cpp_original external objective)

add_custom_command(TARGET il2cpp_original 
    POST_BUILD
    COMMAND xcrun -r libtool -static -o libil2cpp.a libil2cpp_original.a external/libexternal.a objective/libobjective.a
    COMMENT "post build this is command combine libil2cpp_original.a, libojjective.a and libzlib.a into libil2cpp"
    )
