#pragma once

// This should match MonoW32HandleWaitRet in the Mono code.

#define MAXIMUM_WAIT_OBJECTS 64

typedef enum
{
    kWaitStatusSuccess = 0,
    kWaitStatusAbandoned = kWaitStatusSuccess + MAXIMUM_WAIT_OBJECTS,
    kWaitStatusAlerted = -1,
    kWaitStatusTimeout = -2,
    kWaitStatusFailure = -3,
} UnityPalWaitStatus;
