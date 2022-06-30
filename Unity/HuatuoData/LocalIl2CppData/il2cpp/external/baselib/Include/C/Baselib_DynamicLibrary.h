#pragma once

// Baselib Dynamic Library.

// In computing, a dynamic linker is the part of an operating system that loads and links
// the shared libraries needed by an executable when it is executed (at "run time"),
// by copying the content of libraries from persistent storage to RAM, filling jump tables and
// relocating pointers. The specific operating system and executable format determine how
// the dynamic linker functions and how it is implemented.
//
// "Dynamic linker", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Dynamic_linker&oldid=935827444
//
// Platform specific gotchas:
// - On Posix/Darwin based platforms, if executable/library has import entries,
// as for importing functions from .so's/.dylib's at executable/library open time,
// Baselib_DynamicLibrary_GetFunction is able to return them as well.
// This is because of ELF/Mach-O format limitations.
// - On Posix/Darwin based platforms, to be able to query symbols in an executable
// they must be made visible via --external-dynamic and -external_dynamic flags respectively.
// Some linkers have an option to make specific symbols visible.
// - Emscripten limitations are detailed in
// https://github.com/emscripten-core/emscripten/wiki/Linking
// - On some platforms dynamic linker doesn't load downstream dependencies.
// For example if library A imports a symbol from library B,
// and this is passed to the compiler/linker at compilation step,
// on most platforms it will generate load entries inside library A to load library B,
// so if you load library A then library B will be loaded for you by the dynamic linker.
// But on some platforms, you have to load library B first, and then library A.

#include "Baselib_ErrorState.h"

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

typedef struct Baselib_DynamicLibrary_Handle { intptr_t handle; } Baselib_DynamicLibrary_Handle;
// values in range from 0 inclusive to -5 are valid handles on some platforms
static const Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_Handle_Invalid = { -100 };

#include <C/Baselib_DynamicLibrary.inl.h>

// Open a dynamic library.
//
// Dynamic libraries are reference counted, so if the same library is loaded again
// with Baselib_DynamicLibrary_OpenUtf8/Baselib_DynamicLibrary_OpenUtf16, the same file handle is returned.
// It is also possible to load two different libraries containing two different functions that have the same name.
//
// Please note that additional error information should be retrieved via error state explain and be presented to the end user.
// This is needed to improve ergonomics of debugging library loading issues.
//
// \param pathnameUtf8 Library file to be opened.
//                     If relative pathname is provided, platform library search rules are applied (if any).
//                     If nullptr is passed, Baselib_ErrorCode_InvalidArgument will be risen.
//
// Possible error codes:
// - Baselib_ErrorCode_FailedToOpenDynamicLibrary: Unable to open requested dynamic library.
// - Baselib_ErrorCode_NotSupported: This feature is not supported on the current platform.
BASELIB_API Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_OpenUtf8(
    const char* pathnameUtf8,
    Baselib_ErrorState* errorState
);

// Open a dynamic library.
// Functionally identical to Baselib_DynamicLibrary_OpenUtf8, but accepts UTF-16 path instead.
BASELIB_API Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_OpenUtf16(
    const baselib_char16_t* pathnameUtf16,
    Baselib_ErrorState* errorState
);

// Return a handle that can be used to query functions in the program's scope.
// Must be closed via Baselib_DynamicLibrary_Close.
//
// Possible error codes:
// - Baselib_ErrorCode_NotSupported: This feature is not supported on the current platform.
BASELIB_API Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_OpenProgramHandle(
    Baselib_ErrorState* errorState
);

// Convert native handle into baselib handle without changing the dynamic library ref counter.
//
// Provided handle should be closed either via Baselib_DynamicLibrary_Close or other means.
// The caller is responsible for closing the handle once done with it.
// Other corresponding resources should be closed by other means.
//
// \param handle Platform defined native handle.
// \param type   Platform defined native handle type from Baselib_DynamicLibrary_NativeHandleType enum.
//               If unsupported type is passed, will return Baselib_DynamicLibrary_Handle_Invalid.
//
// \returns Baselib_DynamicLibrary_Handle handle.
BASELIB_API Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_FromNativeHandle(
    uint64_t            handle,
    uint32_t            type,
    Baselib_ErrorState* errorState
);

// Lookup a function in a dynamic library.
//
// \param handle       Library handle.
//                     If Baselib_DynamicLibrary_Handle_Invalid is passed, Baselib_ErrorCode_InvalidArgument will be risen.
// \param functionName Function name to look for.
//                     If nullptr is passed, Baselib_ErrorCode_InvalidArgument will be risen.
//
// \returns pointer to the function (can be NULL for symbols mapped to NULL).
//
// Possible error codes:
// - Baselib_ErrorCode_FunctionNotFound: Requested function was not found.
BASELIB_API void* Baselib_DynamicLibrary_GetFunction(
    Baselib_DynamicLibrary_Handle handle,
    const char* functionName,
    Baselib_ErrorState* errorState
);

// Close a dynamic library.
//
// Decreases reference counter, if it becomes zero, closes the library.
// If system api will return an error during this operation, the process will be aborted.
//
// \param handle Library handle.
//               If Baselib_DynamicLibrary_Handle_Invalid is passed, function is no-op.
BASELIB_API void Baselib_DynamicLibrary_Close(
    Baselib_DynamicLibrary_Handle handle
);

#if __cplusplus
}
#endif
