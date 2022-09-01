# file: CMakeList.txt
cmake_minimum_required(VERSION 3.0)
set(CMAKE_OSX_DEPLOYMENT_TARGET iOS)
project(il2cpp)

set(IL2CPP_SOURCE_DIR $ENV{HUATUO_IL2CPP_SOURCE_DIR})
set(SDK_VERSION $ENV{IPHONESIMULATOR_VERSION})

message(STATUS "objective project, binary dir: " ${PROJECT_BINARY_DIR})
message(STATUS "objective project, il2cpp source dir: " ${PROJECT_SOURCE_DIR})


SET_PROPERTY(GLOBAL PROPERTY USE_FOLDERS ON)
find_program(CLANG_EXECUTABLE NAMES clang clang-7 clang-8 clang-9 clang-10)
if (NOT CLANG_EXECUTABLE)
    message(FATAL_ERROR "Cannot find any clang executable.")
endif()


set(CMAKE_OSX_SYSROOT /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator${SDK_VERSION}.sdk)

set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_CXX_COMPILER clang++)
set(CMAKE_C_COMPILER clang)
#add_compile_options(-x objective-c++)

# 以下命令为复制的Xcode生成中的命令
set(CMAKE_CXX_FLAGS "-target arm64-apple-ios11.0 -fmessage-length=0 -fdiagnostics-show-note-include-stack -fmacro-backtrace-limit=0 -std=gnu++1z -fmodules -fmodules-prune-interval=86400 -fmodules-prune-after=345600 -fmodules-ignore-macro=IL2CPP_TARGET_IOS=1 -fembed-bitcode -fmodules-ignore-macro=BASELIB_DYNAMICLIBRARY=1 -fmodules-ignore-macro=BASELIB_INLINE_NAMESPACE=il2cpp_baselib -fmodules-ignore-macro=AARCH64 -fmodules-ignore-macro=__aarch64__ -fmodules-ignore-macro=IL2CPP_SUPPORT_THREADS -fmodules-ignore-macro=IL2CPP_THREADS_PTHREAD -Wnon-modular-include-in-framework-module -Werror=non-modular-include-in-framework-module -Wno-trigraphs -fpascal-strings -Os -fno-common -Wno-missing-field-initializers -Wno-missing-prototypes -Werror=return-type -Wdocumentation -Wunreachable-code -Wquoted-include-in-framework-header -Werror=deprecated-objc-isa-usage -Werror=objc-root-class -Wno-non-virtual-dtor -Wno-overloaded-virtual -Wno-exit-time-destructors -Wno-missing-braces -Wparentheses -Wswitch -Wno-unused-function -Wno-unused-label -Wno-unused-parameter -Wno-unused-variable -Wunused-value -Wempty-body -Wuninitialized -Wconditional-uninitialized -Wno-unknown-pragmas -Wno-shadow -Wno-four-char-constants -Wno-conversion -Wconstant-conversion -Wint-conversion -Wbool-conversion -Wenum-conversion -Wno-float-conversion -Wnon-literal-null-conversion -Wobjc-literal-conversion -Wshorten-64-to-32 -Wno-newline-eof -Wno-c++11-extensions -DIL2CPP_TARGET_IOS=1 -isysroot ${SDK_VERSION} -fasm-blocks -fstrict-aliasing -Wdeprecated-declarations -Winvalid-offsetof -g -Wno-sign-conversion -Winfinite-recursion -Wmove -Wcomma -Wblock-capture-autoreleasing -Wstrict-prototypes -Wrange-loop-analysis -Wno-semicolon-before-method-body -Wunguarded-availability ")


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


include_directories(${IL2CPP_SOURCE_DIR}/libil2cpp)
include_directories(${IL2CPP_SOURCE_DIR}/external)
include_directories(${IL2CPP_SOURCE_DIR}/external/baselib/Include/)
include_directories(${IL2CPP_SOURCE_DIR}/external/bdwgc/include/)
include_directories(${IL2CPP_SOURCE_DIR}/external/baselib/Platforms/OSX/Include/)

# 修改为本机对应SDK路径
include_directories(SYSTEM /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk/usr/include/c++/v1)
include_directories(SYSTEM /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk/usr/include)

get_filename_component(PARENT_PATH ${PROJECT_BINARY_DIR} PATH)
aux_source_directory(${PARENT_PATH}/lump_mm LUMP_SOURCE_LIST)

#find_library(zlib ${PROJECT_SOURCE_DIR}/external)
add_library(objective STATIC ${LUMP_SOURCE_LIST})

