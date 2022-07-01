#pragma once

// Baselib_SystemSemaphore

// In computer science, a semaphore is a variable or abstract data type used to control access to a common resource by multiple processes in a concurrent
// system such as a multitasking operating system. A semaphore is simply a variable. This variable is used to solve critical section problems and to achieve
// process synchronization in the multi processing environment. A trivial semaphore is a plain variable that is changed (for example, incremented or
// decremented, or toggled) depending on programmer-defined conditions.
//
// A useful way to think of a semaphore as used in the real-world system is as a record of how many units of a particular resource are available, coupled with
// operations to adjust that record safely (i.e. to avoid race conditions) as units are required or become free, and, if necessary, wait until a unit of the
// resource becomes available.
//
// "Semaphore (programming)", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Semaphore_(programming)&oldid=872408126

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

typedef struct Baselib_SystemSemaphore_Handle { void* handle; } Baselib_SystemSemaphore_Handle;

// This is the maximum number of tokens that can be made available on a semaphore
enum { Baselib_SystemSemaphore_MaxCount = INT32_MAX };

// Creates a counting semaphore synchronization primitive.
//
// If there are not enough system resources to create a semaphore, process abort is triggered.
//
// \returns          A handle to a semaphore instance. Use Baselib_SystemSemaphore_Free to free the semaphore.
BASELIB_API Baselib_SystemSemaphore_Handle Baselib_SystemSemaphore_Create(void);

// Wait for semaphore token to become available
//
BASELIB_API void Baselib_SystemSemaphore_Acquire(Baselib_SystemSemaphore_Handle semaphore);

// Try to consume a token and return immediately.
//
// \returns          true if token was consumed. false if not.
BASELIB_API bool Baselib_SystemSemaphore_TryAcquire(Baselib_SystemSemaphore_Handle semaphore);

// Wait for semaphore token to become available
//
// Timeout passed to this function may be subject to system clock resolution.
// If the system clock has a resolution of e.g. 16ms that means this function may exit with a timeout error 16ms earlier than originally scheduled.
//
// \param timeout   Time to wait for token to become available.
//
// \returns          true if token was consumed. false if timeout was reached.
BASELIB_API bool Baselib_SystemSemaphore_TryTimedAcquire(Baselib_SystemSemaphore_Handle semaphore, uint32_t timeoutInMilliseconds);

// Submit tokens to the semaphore.
//
// Increase the number of available tokens on the semaphore by `count`. Any waiting threads will be notified there are new tokens available.
// If count reach `Baselib_SystemSemaphore_MaxCount` this function silently discard any overflow.
// Note that hitting max count may inflict a heavy performance penalty.
BASELIB_API void Baselib_SystemSemaphore_Release(Baselib_SystemSemaphore_Handle semaphore, uint32_t count);

// Reclaim resources and memory held by the semaphore.
//
// If threads are waiting on the semaphore, calling free may cause process abort.
BASELIB_API void Baselib_SystemSemaphore_Free(Baselib_SystemSemaphore_Handle semaphore);

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
