#pragma once

#include <stdint.h>

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

extern "C"
{
    struct ZStream;

    IL2CPP_EXPORT intptr_t CreateZStream(int32_t compress, uint8_t gzip, Il2CppMethodPointer func, intptr_t gchandle);
    IL2CPP_EXPORT int32_t CloseZStream(intptr_t zstream);
    IL2CPP_EXPORT int32_t Flush(intptr_t zstream);
    IL2CPP_EXPORT int32_t ReadZStream(intptr_t zstream, intptr_t buffer, int32_t length);
    IL2CPP_EXPORT int32_t WriteZStream(intptr_t zstream, intptr_t buffer, int32_t length);
}
