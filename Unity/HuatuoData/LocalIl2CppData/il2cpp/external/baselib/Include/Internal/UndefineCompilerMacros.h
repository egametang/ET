// DO NOT PUT #pragma once or include guard check here
// This header is designed to be able to be included multiple times

// This header is used to temporary undefine all compiler macros in case there is a naming conflict with
// 3rd party code. Please make sure to always use this paired with the RedefineCompilerMacros.h header.
//
// ex.
//
// #include "UndefineCompilerMacros.h"
// #include "Some3rdParty.h"
// #include "RedefineCompilerMacros.h"

#ifdef DETAIL__COMPILERMACROS_HAD_BEEN_UNDEFINED_BY_UNDEFINECOMPILER_H
    #error "UndefineCompilerMacros.h has been included more than once or RedefineCompilerMacros.h is missing."
#endif

#if COMPILER_GCC
    #define DETAIL__TEMP_COMPILER_GCC_WAS_1
#endif
#undef COMPILER_GCC

#if COMPILER_CLANG
    #define DETAIL__TEMP_COMPILER_CLANG_WAS_1
#endif
#undef COMPILER_CLANG

#if COMPILER_MSVC
    #define DETAIL__TEMP_COMPILER_MSVC_WAS_1
#endif
#undef COMPILER_MSVC

#define DETAIL__COMPILERMACROS_HAD_BEEN_UNDEFINED_BY_UNDEFINECOMPILER_H
