#pragma once

#include "il2cpp-config.h"

#if RUNTIME_TINY
typedef TinyMethod VmMethod;
#else // Assume the libil2cpp runtime
typedef MethodInfo VmMethod;
#endif
