#pragma once

#include "Baselib_Timer.h"
#include "Baselib_ErrorState.h"

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Unique thread id that can be used to compare different threads or stored for bookkeeping etc..
typedef intptr_t Baselib_Thread_Id;

// Baselib_Thread_Id that is guaranteed not to represent a thread
static const Baselib_Thread_Id Baselib_Thread_InvalidId = 0;

// Yields the execution context of the current thread to other threads, potentially causing a context switch.
//
// The operating system may decide to not switch to any other thread.
BASELIB_API void Baselib_Thread_YieldExecution(void);

// Return the thread id of the current thread, i.e. the thread that is calling this function
BASELIB_API Baselib_Thread_Id Baselib_Thread_GetCurrentThreadId(void);


// We currently do not allow creating threads from C# bindings,
// since there is right now no way accessible way to inform the garbage collector about new baselib threads.
// I.e. any managed allocation on a baselib thread created from C# would never be garbage collected!
#ifndef BASELIB_BINDING_GENERATION

// The minimum guaranteed number of max concurrent threads that works on all platforms.
//
// This only applies if all the threads are created with Baselib.
// In practice, it might not be possible to create this many threads either. If memory is exhausted,
// by for example creating threads with very large stacks, that might translate to a lower limit in practice.
// Note that on many platforms the actual limit is way higher.
static const int Baselib_Thread_MinGuaranteedMaxConcurrentThreads = 64;

typedef struct Baselib_Thread Baselib_Thread;

typedef void (*Baselib_Thread_EntryPointFunction)(void* arg);

typedef struct Baselib_Thread_Config
{
    // Nullterminated name of the created thread (optional)
    // Useful exclusively for debugging - which tooling it is shown by and how it can be queried is platform dependent.
    const char* name;

    // The minimum size in bytes to allocate for the thread stack. (optional)
    // If not set, a platform/system specific default stack size will be used.
    // If the value set does not conform to platform specific minimum values or alignment requirements,
    // the actual stack size used will be bigger than what was requested.
    uint64_t stackSize;

    // Required, this is set by calling Baselib_Thread_ConfigCreate with a valid entry point function.
    Baselib_Thread_EntryPointFunction entryPoint;

    // Argument to the entry point function, does only need to be set if entryPoint takes an argument.
    void* entryPointArgument;
} Baselib_Thread_Config;

// Creates and starts a new thread.
//
// On some platforms the thread name is not set until the thread has begun executing, which is not guaranteed
// to have happened when the creation function returns. There is typically a platform specific limit on the length of
// the thread name. If config.name is longer than this limit, the name will be automatically truncated.
//
// \param config        A pointer to a config object. entryPoint needs to be a valid function pointer, all other properties can be zero/null.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument:            config.entryPoint is null
// - Baselib_ErrorCode_OutOfSystemResources:       there is not enough memory to create a thread with that stack size or the system limit of number of concurrent threads has been reached
BASELIB_API Baselib_Thread* Baselib_Thread_Create(Baselib_Thread_Config config, Baselib_ErrorState* errorState);


// Waits until a thread has finished its execution.
//
// Also frees its resources.
// If called and completed successfully, no Baselib_Thread function can be called again on the same Baselib_Thread.
//
// \param thread                 A pointer to a thread object.
// \param timeoutInMilliseconds  Time to wait for the thread to finish
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument:       thread is null
// - Baselib_ErrorCode_ThreadCannotJoinSelf:  the thread parameter points to the current thread, i.e. the thread that is calling this function
// - Baselib_ErrorCode_Timeout:               timeout is reached before the thread has finished
BASELIB_API void Baselib_Thread_Join(Baselib_Thread* thread, uint32_t timeoutInMilliseconds, Baselib_ErrorState* errorState);

// Return the thread id of the thread given as argument
//
// \param thread        A pointer to a thread object.
BASELIB_API Baselib_Thread_Id Baselib_Thread_GetId(Baselib_Thread* thread);

// Returns true if there is support in baselib for threads on this platform, otherwise false.
BASELIB_API bool Baselib_Thread_SupportsThreads(void);

#endif // !BASELIB_BINDING_GENERATION

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
