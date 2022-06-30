#pragma once

// Baselib_ThreadLocalStorage

// Thread-local storage (TLS) is a computer programming method that uses static or global memory local to a thread.
//
// TLS is used in some places where ordinary, single-threaded programs would use global variables, but where this would be inappropriate
// in multithreaded cases. An example of such situations is where functions use a global variable to set an error condition
// (for example the global variable errno used by many functions of the C library). If errno were a global variable,
// a call of a system function on one thread may overwrite the value previously set by a call of a system function on a different thread,
// possibly before following code on that different thread could check for the error condition. The solution is to have errno be a variable
// that looks like it is global, but in fact exists once per thread—i.e., it lives in thread-local storage. A second use case would be
// multiple threads accumulating information into a global variable. To avoid a race condition, every access to this global variable would
// have to be protected by a mutex. Alternatively, each thread might accumulate into a thread-local variable (that, by definition,
// cannot be read from or written to from other threads, implying that there can be no race conditions). Threads then only have to synchronise
// a final accumulation from their own thread-local variable into a single, truly global variable.
//
// Many systems impose restrictions on the size of the thread-local memory block, in fact often rather tight limits.
// On the other hand, if a system can provide at least a memory address (pointer) sized variable thread-local, then this allows the use of
// arbitrarily sized memory blocks in a thread-local manner, by allocating such a memory block dynamically and storing the memory address of
// that block in the thread-local variable.
//
// "Thread-local storage", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Thread-local_storage&oldid=860347814

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// It's guaranteed that we can allocate at least Baselib_TLS_MinimumGuaranteedSlots values on all platforms.
static const uint32_t Baselib_TLS_MinimumGuaranteedSlots = 100;

// Thread Local Storage slot handle.
typedef uintptr_t Baselib_TLS_Handle;

// Allocates a new Thread Local Storage slot. In case of an error, abort with Baselib_ErrorCode_OutOfSystemResources will be triggered.
// On some platforms this might be fiber local storage.
//
// The value of a newly create Thread Local Storage slot is guaranteed to be zero on all threads.
BASELIB_API Baselib_TLS_Handle Baselib_TLS_Alloc(void);

// Frees provided Thread Local Storage slot.
BASELIB_API void Baselib_TLS_Free(Baselib_TLS_Handle handle);

// Sets value to Thread Local Storage slot.
BASELIB_FORCEINLINE_API void Baselib_TLS_Set(Baselib_TLS_Handle handle, uintptr_t value);

// Gets value from Thread Local Storage slot.
//
// If called on just initialized variable, guaranteed to return 0.
BASELIB_FORCEINLINE_API uintptr_t Baselib_TLS_Get(Baselib_TLS_Handle handle);

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif

#include <C/Baselib_ThreadLocalStorage.inl.h>
