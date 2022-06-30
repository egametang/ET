#pragma once

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Generates breakpoint exception (interrupt) the same way as normal breakpoint would.
//
// If debugger is attached, this will break into the debugger.
// If debugger is not attached, application will crash, unless breakpoint exception is handled.
// Breakpoint exception can be handled on some platforms by using signal(SIGTRAP, ...) or AddVectoredExceptionHandler.
// Platforms can override default compiler implementation by providing BASELIB_DEBUG_TRAP.
#define Baselib_Debug_Break() BASELIB_DEBUG_TRAP()

// \returns true if debugger is attached
BASELIB_API bool Baselib_Debug_IsDebuggerAttached(void);

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
