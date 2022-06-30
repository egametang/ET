#pragma once

#if defined(__cplusplus)
#include "os/Mutex.h"
#include "os/WaitStatus.h"
typedef il2cpp::os::Mutex UnityPalMutex;
typedef il2cpp::os::MutexHandle UnityPalMutexHandle;
typedef il2cpp::os::FastMutex UnityPalFastMutex;
typedef il2cpp::os::FastMutexImpl UnityPalFastMutexImpl;
#else
typedef struct UnityPalMutex UnityPalMutex;
typedef struct UnityPalMutexHandle UnityPalMutexHandle;
typedef struct UnityPalFastMutex UnityPalFastMutex;
typedef struct UnityPalFastMutexImpl UnityPalFastMutexImpl;
#endif


#if defined(__cplusplus)
extern "C"
{
#endif
UnityPalMutex* UnityPalMutexNew(int32_t initiallyOwned);
void UnityPalMutexDelete(UnityPalMutex* mutex);
void UnityPalMutexLock(UnityPalMutex* mutex, int32_t interruptible);
int32_t UnityPalMutexTryLock(UnityPalMutex* mutex, uint32_t milliseconds, int32_t interruptible);
void UnityPalMutexUnlock(UnityPalMutex* mutex);

UnityPalMutexHandle* UnityPalMutexHandleNew(UnityPalMutex* mutex);
void UnityPalMutexHandleDelete(UnityPalMutexHandle* mutex);
int32_t UnityPalMutexHandleWait(UnityPalMutexHandle* handle);
int32_t UnityPalMutexHandleWaitMs(UnityPalMutexHandle* handle, uint32_t ms);
void UnityPalMutexHandleSignal(UnityPalMutexHandle* handle);
UnityPalMutex* UnityPalMutexHandleGet(UnityPalMutexHandle* handle);

UnityPalFastMutex* UnityPalFastMutexNew();
void UnityPalFastMutexDelete(UnityPalFastMutex* fastMutex);
void UnityPalFastMutexLock(UnityPalFastMutex* fastMutex);
void UnityPalFastMutexUnlock(UnityPalFastMutex* fastMutex);
UnityPalFastMutexImpl* UnityPalFastMutexGetImpl(UnityPalFastMutex* fastMutex);


#if defined(__cplusplus)
}
#endif
