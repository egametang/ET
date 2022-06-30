#pragma once

#include "os/c-api/Error-c-api.h"

#if defined(__cplusplus)
#include "os/ThreadLocalValue.h"
typedef il2cpp::os::ThreadLocalValue UnityPalThreadLocalValue;

#else

typedef struct UnityPalThreadLocalValue UnityPalThreadLocalValue;

#endif

#if defined(__cplusplus)
extern "C"
{
#endif

UnityPalThreadLocalValue* UnityPalThreadLocalValueNew();
void UnityPalThreadLocalValueDelete(UnityPalThreadLocalValue* object);

UnityPalErrorCode UnityPalThreadLocalValueSetValue(UnityPalThreadLocalValue* object, void* value);
UnityPalErrorCode UnityPalThreadLocalValueGetValue(UnityPalThreadLocalValue* object, void** value);

#if defined(__cplusplus)
}
#endif
