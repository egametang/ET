#pragma once

// Baselib_SystemFutex

// In computing, a futex (short for "fast userspace mutex") is a kernel system call that programmers can use to implement basic locking, or as a building block
// for higher-level locking abstractions such as semaphores and POSIX mutexes or condition variables.
//
// A futex consists of a kernelspace wait queue that is attached to an atomic integer in userspace. Multiple processes or threads operate on the integer
// entirely in userspace (using atomic operations to avoid interfering with one another), and only resort to relatively expensive system calls to request
// operations on the wait queue (for example to wake up waiting processes, or to put the current process on the wait queue). A properly programmed futex-based
// lock will not use system calls except when the lock is contended; since most operations do not require arbitration between processes, this will not happen
// in most cases.
//
// "Futex", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Futex&oldid=850172014

#include "Baselib_WakeupFallbackStrategy.h"

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Determines if the platform has access to a kernel level futex api
//
// If native support is not present the futex will fallback to an emulated futex setup.
//
// Notes on the emulation:
// * It uses a single synchronization primitive to multiplex all potential addresses. This means there will be
//   additional contention as well as spurious wakeups compared to a native implementation.
// * While the fallback implementation is not something that should be used in production it can still provide value
//   when bringing up new platforms or to test features built on top of the futex api.
BASELIB_INLINE_API bool Baselib_SystemFutex_NativeSupport(void) { return PLATFORM_FUTEX_NATIVE_SUPPORT == 1; }

// Wait for notification.
//
// Address will be checked atomically against expected before entering wait. This can be used to guarantee there are no lost wakeups.
// Note: When notified the thread always wake up regardless if the expectation match the value at address or not.
//
// | Problem this solves
// | Thread 1: checks condition and determine we should enter wait
// | Thread 2: change condition and notify waiting threads
// | Thread 1: enters waiting state
// |
// | With a futex the two Thread 1 operations become a single op.
//
// Spurious Wakeup - This function is subject to spurious wakeups.
//
// \param address                   Any address that can be read from both user and kernel space.
// \param expected                  What address points to will be checked against this value. If the values don't match thread will not enter a waiting state.
// \param timeoutInMilliseconds     A timeout indicating to the kernel when to wake the thread. Regardless of being notified or not.
BASELIB_API void Baselib_SystemFutex_Wait(int32_t* address, int32_t expected, uint32_t timeoutInMilliseconds);

// Notify threads waiting on a specific address.
//
// \param address                   Any address that can be read from both user and kernel space
// \param count                     Number of waiting threads to wakeup.
// \param wakeupFallbackStrategy    Platforms that don't support waking up a specific number of threads will use this strategy.
BASELIB_API void Baselib_SystemFutex_Notify(int32_t* address, uint32_t count, Baselib_WakeupFallbackStrategy wakeupFallbackStrategy);

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
