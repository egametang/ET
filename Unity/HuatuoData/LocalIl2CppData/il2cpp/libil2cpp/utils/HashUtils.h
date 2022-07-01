#pragma once

namespace il2cpp
{
namespace utils
{
    class HashUtils
    {
        static const size_t Seed = 486187739;
    public:
        static inline size_t Combine(size_t hash1, size_t hash2)
        {
            return hash1 * Seed + hash2;
        }

        static inline size_t AlignedPointerHash(void* ptr)
        {
            return ((uintptr_t)ptr) >> 3;
        }
    };

    template<class T>
    struct PointerHash
    {
        size_t operator()(const T* value) const
        {
            return (size_t)value;
        }
    };

    template<class T>
    struct PassThroughHash
    {
        size_t operator()(T value) const
        {
            return (size_t)value;
        }
    };
} /* namespace vm */
} /* namespace il2cpp */
