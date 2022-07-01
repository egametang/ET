#pragma once

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

// Default for PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT if not specified by platform.
#ifndef PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT
    #define PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT COMPILER_ALIGN_OF(max_align_t)
#endif
