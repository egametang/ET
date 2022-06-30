#pragma once

#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>

// Default for PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT if not specified by platform.
#ifndef PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT
    #define PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT COMPILER_ALIGN_OF(max_align_t)
#endif

// Custom type suitable for representing a UTF-16 codepoint crossplatform.
// Because char16_t is not available on all platforms,
// uint16_t is chosen as a type that inflicts the same behavior across platforms,
// as is requiring a cast from platform specific UTF-16 representation.
typedef uint16_t baselib_char16_t;
