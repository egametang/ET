#pragma once

#include <stdint.h>
#include <memory.h>
#include "il2cpp-api-types.h"

// unaligned safe reading
namespace il2cpp
{
namespace utils
{
    static inline Il2CppChar ReadChar(const char* p) { Il2CppChar val; memcpy(&val, p, sizeof(Il2CppChar)); return val; }

    static inline uint16_t Read16(const char* p) { uint16_t val; memcpy(&val, p, sizeof(uint16_t)); return val; }
    static inline uint32_t Read32(const char* p) { uint32_t val; memcpy(&val, p, sizeof(uint32_t)); return val; }
    static inline uint64_t Read64(const char* p) { uint64_t val; memcpy(&val, p, sizeof(uint64_t)); return val; }
    static inline float ReadFloat(const char* p) { float val; memcpy(&val, p, sizeof(float)); return val; }
    static inline double ReadDouble(const char* p) { double val; memcpy(&val, p, sizeof(double)); return val; }

    static inline Il2CppChar ReadChar(const char** p) { Il2CppChar val; memcpy(&val, *p, sizeof(Il2CppChar)); *p += sizeof(Il2CppChar); return val; }
    static inline uint8_t Read8(const char** p) { uint8_t val; memcpy(&val, *p, sizeof(uint8_t)); *p += sizeof(uint8_t); return val; }
    static inline uint16_t Read16(const char** p) { uint16_t val; memcpy(&val, *p, sizeof(uint16_t)); *p += sizeof(uint16_t); return val; }
    static inline uint32_t Read32(const char** p) { uint32_t val; memcpy(&val, *p, sizeof(uint32_t)); *p += sizeof(uint32_t); return val; }
    static inline uint64_t Read64(const char** p) { uint64_t val; memcpy(&val, *p, sizeof(uint64_t)); *p += sizeof(uint64_t); return val; }
    static inline float ReadFloat(const char** p) { float val; memcpy(&val, *p, sizeof(float)); *p += sizeof(float); return val; }
    static inline double ReadDouble(const char** p) { double val; memcpy(&val, *p, sizeof(double)); *p += sizeof(double); return val; }

    uint32_t ReadCompressedUInt32(const char** p);
    int32_t ReadCompressedInt32(const char** p);
} /* utils */
} /* il2cpp */
