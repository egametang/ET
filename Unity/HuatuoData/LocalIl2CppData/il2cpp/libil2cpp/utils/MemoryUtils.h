#pragma once
#include <stdint.h>
#include "xxhash.h"

namespace il2cpp
{
namespace utils
{
    class MemoryUtils
    {
    public:
        template<typename T>
        static int32_t MemCmpRef(T* left, T* right)
        {
            return memcmp(left, right, sizeof(T));
        }

#if IL2CPP_TINY
        template<typename T>
        static int32_t MemHashRef(T* val)
        {
            return XXH32(val, sizeof(T), 0x8f37154b);
        }

#endif
    };
#define DECL_MEMCMP_NUM(typ) template<> inline int32_t MemoryUtils::MemCmpRef<typ>(typ* left, typ* right) { return (*right > *left) ? -1 : (*right < *left) ? 1 : 0; }
    DECL_MEMCMP_NUM(int8_t)
    DECL_MEMCMP_NUM(int16_t)
    DECL_MEMCMP_NUM(int32_t)
    DECL_MEMCMP_NUM(int64_t)
    DECL_MEMCMP_NUM(uint8_t)
    DECL_MEMCMP_NUM(uint16_t)
    DECL_MEMCMP_NUM(uint32_t)
    DECL_MEMCMP_NUM(uint64_t)
    // don't think this will give the right result for NaNs and such
    DECL_MEMCMP_NUM(float)
    DECL_MEMCMP_NUM(double)
#undef DECL_MEMCMP_NUM

#define DECL_MEMHASH_NUM(typ) template<> inline int32_t MemoryUtils::MemHashRef(typ* val) { return (int32_t)(*val); }
    DECL_MEMHASH_NUM(int8_t)
    DECL_MEMHASH_NUM(int16_t)
    DECL_MEMHASH_NUM(int32_t)
    DECL_MEMHASH_NUM(uint8_t)
    DECL_MEMHASH_NUM(uint16_t)
    DECL_MEMHASH_NUM(uint32_t)
    DECL_MEMHASH_NUM(float)
#undef DECL_MEMHASH_NUM

    template<> inline int32_t MemoryUtils::MemHashRef(int64_t* val) { int64_t k = *val; return (int32_t)(k & 0xffffffff) ^ (int32_t)((k >> 32) & 0xffffffff); }
    template<> inline int32_t MemoryUtils::MemHashRef(uint64_t* val) { return MemHashRef(reinterpret_cast<int64_t*>(val)); }
    template<> inline int32_t MemoryUtils::MemHashRef(double* val) { return MemHashRef(reinterpret_cast<int64_t*>(val)); }
} // namespace utils
} // namespace il2cpp
