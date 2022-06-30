#pragma once

#include "Baselib_ErrorCode.h"

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

BASELIB_API COMPILER_NORETURN void Baselib_Process_Abort(Baselib_ErrorCode error);

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
