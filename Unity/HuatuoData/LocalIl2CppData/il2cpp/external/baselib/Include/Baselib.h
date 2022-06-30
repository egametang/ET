#pragma once

#include "Internal/PlatformDetection.h"
#include "Internal/ArchitectureDetection.h"
#include "Internal/PlatformEnvironment.h"


#ifdef BASELIB_INLINE_NAMESPACE
    #ifndef __cplusplus
        #error "BASELIB_INLINE_NAMESPACE is not available when compiling C code"
    #endif

    #define BASELIB_CPP_INTERFACE     inline namespace BASELIB_INLINE_NAMESPACE
    #define BASELIB_C_INTERFACE       BASELIB_CPP_INTERFACE
#else
    #define BASELIB_CPP_INTERFACE     extern "C++"
    #define BASELIB_C_INTERFACE       extern "C"
#endif

#if defined(BASELIB_USE_DYNAMICLIBRARY)
    #define BASELIB_API     IMPORTED_SYMBOL
#elif defined(BASELIB_DYNAMICLIBRARY)
    #define BASELIB_API     EXPORTED_SYMBOL
#else
    #define BASELIB_API
#endif

// BASELIB_BINDING_GENERATION is set by the bindings generator and by BindingsExposedInlineImplementations.cpp
// in order to selectively provide symbols bindings can link to for some our our inline implementations.
#ifdef BASELIB_BINDING_GENERATION
    #define BASELIB_INLINE_API        BASELIB_API
    #define BASELIB_FORCEINLINE_API   BASELIB_API
#else
    #define BASELIB_INLINE_API        static inline
    #define BASELIB_FORCEINLINE_API   static COMPILER_FORCEINLINE
#endif


#include "Internal/BasicTypes.h"
#include "Internal/CoreMacros.h"
#include "Internal/Assert.h"
