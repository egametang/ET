#pragma once

#include "../C/Baselib_DynamicLibrary.h"

// alias for Baselib_DynamicLibrary_OpenUtf8
static inline Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_Open(
    const char* pathnameUtf8,
    Baselib_ErrorState* errorState
)
{
    return Baselib_DynamicLibrary_OpenUtf8(pathnameUtf8, errorState);
}

// alias for Baselib_DynamicLibrary_OpenUtf16
static inline Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_Open(
    const baselib_char16_t* pathnameUtf16,
    Baselib_ErrorState* errorState
)
{
    return Baselib_DynamicLibrary_OpenUtf16(pathnameUtf16, errorState);
}

static inline bool operator==(const Baselib_DynamicLibrary_Handle& a, const Baselib_DynamicLibrary_Handle& b)
{
    return a.handle == b.handle;
}

static inline bool operator!=(const Baselib_DynamicLibrary_Handle& a, const Baselib_DynamicLibrary_Handle& b)
{
    return a.handle != b.handle;
}
