#pragma once

#include <processthreadsapi.h>
#include <errhandlingapi.h>

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

BASELIB_FORCEINLINE_API void Baselib_TLS_Set(Baselib_TLS_Handle handle, uintptr_t value)
{
    BOOL success = TlsSetValue((DWORD)handle, (void*)value);
    BaselibAssert(success != 0);
}

BASELIB_FORCEINLINE_API uintptr_t Baselib_TLS_Get(Baselib_TLS_Handle handle)
{
    void* result = TlsGetValue((DWORD)handle);
    BaselibAssert((result != NULL) || (GetLastError() == 0L));
    return (uintptr_t)result;
}

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
