#pragma once

#ifndef BASELIB_ENABLE_SOURCELOCATION
    #ifdef NDEBUG
        #define BASELIB_ENABLE_SOURCELOCATION 0
    #else
        #define BASELIB_ENABLE_SOURCELOCATION 1
    #endif
#endif

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Human readable about the original location of a piece of source code.
typedef struct Baselib_SourceLocation
{
    const char* file;
    const char* function;
    uint32_t    lineNumber;
} Baselib_SourceLocation;

// Macro to create source location in-place for the current line of code.
#if BASELIB_ENABLE_SOURCELOCATION
    #define BASELIB_SOURCELOCATION Baselib_SourceLocation { __FILE__, __func__, __LINE__ }
#else
    #define BASELIB_SOURCELOCATION Baselib_SourceLocation { NULL, NULL, 0 }
#endif

#ifdef __cplusplus
}
#endif
