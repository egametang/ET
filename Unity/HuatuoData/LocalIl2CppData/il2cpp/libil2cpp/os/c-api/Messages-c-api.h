#pragma once

#include "os/c-api/Error-c-api.h"
#include "os/c-api/Messages-c-api.h"


#if defined(__cplusplus)
extern "C"
{
#endif

char* UnityPalMessagesFromCode(UnityPalErrorCode code);

#if defined(__cplusplus)
}
#endif
