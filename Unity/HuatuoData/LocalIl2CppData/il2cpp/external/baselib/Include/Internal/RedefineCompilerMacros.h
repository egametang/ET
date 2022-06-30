// DO NOT PUT #pragma once or include guard check here
// This header is designed to be able to be included multiple times

// This header is used to redefine compiler macros after they were temporary undefined by UndefineCompilerMacros.h
// Please make sure to always use this paired with the UndefineCompilerMacros.h header.
//
// ex.
//
// #include "UndefineCompilerMacros.h"
// #include "Some3rdParty.h"
// #include "RedefineCompilerMacros.h"

#ifndef DETAIL__COMPILERMACROS_HAD_BEEN_UNDEFINED_BY_UNDEFINECOMPILER_H
    #error "RedefineCompilerMacros.h can only be used after UndefinePlatforms.h got included before."
#endif

#undef DETAIL__COMPILERMACROS_HAD_BEEN_UNDEFINED_BY_UNDEFINECOMPILER_H

#undef COMPILER_GCC
#if defined(DETAIL__TEMP_COMPILER_GCC_WAS_1)
    #undef DETAIL__TEMP_COMPILER_GCC_WAS_1
    #define COMPILER_GCC 1
#else
    #define COMPILER_GCC 0
#endif

#undef COMPILER_CLANG
#if defined(DETAIL__TEMP_COMPILER_CLANG_WAS_1)
    #undef DETAIL__TEMP_COMPILER_CLANG_WAS_1
    #define COMPILER_CLANG 1
#else
    #define COMPILER_CLANG 0
#endif

#undef COMPILER_MSVC
#if defined(DETAIL__TEMP_COMPILER_MSVC_WAS_1)
    #undef DETAIL__TEMP_COMPILER_MSVC_WAS_1
    #define COMPILER_MSVC 1
#else
    #define COMPILER_MSVC 0
#endif
