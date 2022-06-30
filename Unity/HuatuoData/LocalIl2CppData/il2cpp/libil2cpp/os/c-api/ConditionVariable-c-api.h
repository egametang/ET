#pragma once

#include <stdint.h>
#include "Mutex-c-api.h"

#if defined(__cplusplus)
#include "os/ConditionVariable.h"
typedef il2cpp::os::ConditionVariable UnityPalConditionVariable;
#else
typedef struct UnityPalConditionVariable UnityPalConditionVariable;
#endif

#if defined(__cplusplus)
extern "C"
{
#endif

UnityPalConditionVariable* UnityPalConditionVariableNew();
void UnityPalConditionVariableDelete(UnityPalConditionVariable* object);

int UnityPalConditionVariableWait(UnityPalConditionVariable* object, UnityPalFastMutex* lock);
int UnityPalConditionVariableTimedWait(UnityPalConditionVariable* object, UnityPalFastMutex* lock, uint32_t timeout_ms);
void UnityPalConditionVariableBroadcast(UnityPalConditionVariable* object);
void UnityPalConditionVariableSignal(UnityPalConditionVariable* object);

#if defined(__cplusplus)
}
#endif
