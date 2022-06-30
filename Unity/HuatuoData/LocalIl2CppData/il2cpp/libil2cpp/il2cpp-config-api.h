#pragma once

#include "os/c-api/il2cpp-config-platforms.h"
#include "os/c-api/il2cpp-config-api-platforms.h"
#include "il2cpp-api-types.h"

// If the platform loads il2cpp as a dynamic library but does not have dlsym (or equivalent) then
// define IL2CPP_API_DYNAMIC_NO_DLSYM = 1 to add support for api function registration and symbol
// lookup APIs, see il2cpp-api.cpp
#ifndef IL2CPP_API_DYNAMIC_NO_DLSYM
#define IL2CPP_API_DYNAMIC_NO_DLSYM 0
#endif

/* Profiler */
#ifndef IL2CPP_ENABLE_PROFILER
#define IL2CPP_ENABLE_PROFILER !IL2CPP_TINY
#endif

#if IL2CPP_TARGET_ARMV7
// On ARMv7 with Thumb instructions the lowest bit is always set.
// With Thumb2 the second-to-lowest bit is also set. Mask both of
// them off so that we can do a comparison properly based on the data
// from the linker map file.
#define IL2CPP_POINTER_SPARE_BITS 3
#else
// Some compilers align functions by default (MSVC), some do not (GCC).
// Do not mask bits on platforms that do not absolutely require it.
#define IL2CPP_POINTER_SPARE_BITS 0
#endif

#if IL2CPP_COMPILER_MSVC || defined(__ARMCC_VERSION)
#define NORETURN __declspec(noreturn)
#elif (IL2CPP_POINTER_SPARE_BITS == 0) && (defined(__clang__) || defined(__GNUC__))
#define NORETURN __attribute__ ((noreturn))
#else
#define NORETURN
#endif

#if IL2CPP_TARGET_IOS || IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_DARWIN
#define REAL_NORETURN __attribute__ ((noreturn))
#else
#define REAL_NORETURN NORETURN
#endif
