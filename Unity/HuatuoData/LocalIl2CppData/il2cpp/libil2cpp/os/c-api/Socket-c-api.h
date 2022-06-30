#pragma once

#include <stdint.h>
#include "WaitStatus-c-api.h"

#if defined(__cplusplus)
#include "os/Socket.h"


#else


#endif //__cplusplus

#if defined(__cplusplus)
extern "C"
{
#endif

UnityPalWaitStatus UnityPalGetHostByName(const char* host, char** name, int32_t* family, char*** aliases, void*** address_list, int32_t* address_size);

#if defined(__cplusplus)
}
#endif
