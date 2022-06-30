#pragma once

#include "il2cpp-api-types.h"
#include "il2cpp-metadata.h"

#if IL2CPP_ENABLE_NATIVE_STACKTRACES
struct MethodDefinitionKey
{
    Il2CppMethodPointer method;
#if IL2CPP_TINY_DEBUG_METADATA && !IL2CPP_TINY_DEBUGGER
    int32_t methodIndex;
#else
    Il2CppMetadataMethodDefinitionHandle methodHandle;
#endif
};
#endif
