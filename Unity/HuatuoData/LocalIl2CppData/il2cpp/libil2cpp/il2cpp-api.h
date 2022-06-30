#pragma once

#include <stdint.h>

#include "il2cpp-config-api.h"

#if IL2CPP_API_DYNAMIC_NO_DLSYM
#if defined(__cplusplus)
extern "C"
{
#endif // __cplusplus
IL2CPP_EXPORT void il2cpp_api_register_symbols(void);
IL2CPP_EXPORT void* il2cpp_api_lookup_symbol(const char* name);
#if defined(__cplusplus)
}
#endif // __cplusplus
#endif

#if defined(__cplusplus)
extern "C"
{
#endif // __cplusplus
#define DO_API(r, n, p)             IL2CPP_EXPORT r n p;
#define DO_API_NO_RETURN(r, n, p)   IL2CPP_EXPORT NORETURN r n p;
#include "il2cpp-api-functions.h"
#undef DO_API
#undef DO_API_NORETURN
#if defined(__cplusplus)
}
#endif // __cplusplus
