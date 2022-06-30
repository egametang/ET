#pragma once

#include "../Baselib_StaticAssert.h"

#define BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(_enumType)  \
    BASELIB_STATIC_ASSERT(sizeof(_enumType) == 4,         \
        "Baselib assumes that sizeof any enum type is exactly 4 bytes, there might be ABI compatibility problems if violated");
