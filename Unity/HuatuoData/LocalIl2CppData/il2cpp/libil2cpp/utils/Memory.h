#pragma once

#include "il2cpp-config.h"

namespace il2cpp
{
namespace utils
{
    struct LIBIL2CPP_CODEGEN_API Memory
    {
        static void SetMemoryCallbacks(Il2CppMemoryCallbacks* callbacks);

        static void* Malloc(size_t size);
        static void* AlignedMalloc(size_t size, size_t alignment);
        static void Free(void* memory);
        static void AlignedFree(void* memory);
        static void* Calloc(size_t count, size_t size);
        static void* Realloc(void* memory, size_t newSize);
        static void* AlignedRealloc(void* memory, size_t newSize, size_t alignment);
    };
} /* namespace utils */
} /* namespace il2cpp */

#define IL2CPP_MALLOC(size) il2cpp::utils::Memory::Malloc(size)
#define IL2CPP_MALLOC_ALIGNED(size, alignment) il2cpp::utils::Memory::AlignedMalloc(size, alignment)
#define IL2CPP_MALLOC_ZERO(size) il2cpp::utils::Memory::Calloc(1,size)
#define IL2CPP_FREE(memory) il2cpp::utils::Memory::Free(memory)
#define IL2CPP_FREE_ALIGNED(memory) il2cpp::utils::Memory::AlignedFree(memory)
#define IL2CPP_CALLOC(count, size) il2cpp::utils::Memory::Calloc(count,size)
#define IL2CPP_REALLOC(memory, newSize) il2cpp::utils::Memory::Realloc(memory,newSize)
#define IL2CPP_REALLOC_ALIGNED(memory, newSize, alignment) il2cpp::utils::Memory::AlignedRealloc(memory, newSize, alignment)
