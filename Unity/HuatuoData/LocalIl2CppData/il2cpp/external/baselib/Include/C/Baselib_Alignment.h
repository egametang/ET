#pragma once

#ifndef BASELIB_ALIGN_OF
    #if defined(__cplusplus) // We assume C++11 support (also, note that Mscv has correct version numbers on this attribute as opt-in)
        #define BASELIB_ALIGN_OF(TYPE_) alignof(TYPE_)

// As of gcc8+clang 8, alignof and _Alignof return the ABI alignment of a type, as opposed to the preferred alignment.
// __alignof still returns the preferred alignment.
// Also see:
// https://gcc.gnu.org/gcc-8/porting_to.html#alignof
// https://releases.llvm.org/8.0.0/tools/clang/docs/ReleaseNotes.html#modified-compiler-flags
    #elif STDC_VERSION >= 201112L
        #define BASELIB_ALIGN_OF(TYPE_) _Alignof(TYPE_)
    #else
        #define BASELIB_ALIGN_OF(TYPE_) COMPILER_ALIGN_OF(TYPE_)
    #endif
#endif

#ifndef BASELIB_ALIGN_AS
    #define BASELIB_ALIGN_AS(ALIGNMENT_) COMPILER_ALIGN_AS(ALIGNMENT_)
#endif
