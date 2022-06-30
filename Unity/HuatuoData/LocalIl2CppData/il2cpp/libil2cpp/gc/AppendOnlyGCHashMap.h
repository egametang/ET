#pragma once

#include "utils/Il2CppHashMap.h"
#include "utils/NonCopyable.h"
#include "GarbageCollector.h"

namespace il2cpp
{
namespace gc
{
    template<class Key, class T,
             class HashFcn,
             class EqualKey = std::equal_to<Key> >
    class AppendOnlyGCHashMap : public il2cpp::utils::NonCopyable
    {
        typedef Il2CppHashMap<Key, size_t, HashFcn, EqualKey> hash_map_type;
        typedef typename Il2CppHashMap<Key, size_t, HashFcn, EqualKey>::const_iterator ConstIterator;
    public:

        typedef typename hash_map_type::key_type key_type;
        typedef T data_type;
        typedef typename hash_map_type::size_type size_type;
        typedef typename hash_map_type::hasher hasher;
        typedef typename hash_map_type::key_equal key_equal;

        AppendOnlyGCHashMap() :
            m_Data(NULL),
            m_Size(0)
        {
        }

        ~AppendOnlyGCHashMap()
        {
            if (m_Data)
                il2cpp::gc::GarbageCollector::FreeFixed(m_Data);
        }

        bool Contains(const Key& k)
        {
            return m_Map.find(k) != m_Map.end();
        }

        // Returns true if value was added. False if it already existed
        bool Add(const Key& k, T value)
        {
            if (m_Map.find(k) != m_Map.end())
                return false;

            if (m_Size == 0)
            {
                m_Size = 8;
                m_Data = (T*)il2cpp::gc::GarbageCollector::AllocateFixed(m_Size * sizeof(T), NULL);
                IL2CPP_ASSERT(m_Data);
            }
            else if (m_Map.size() == m_Size)
            {
                size_t newSize = 2 * m_Size;
                T* newData = (T*)il2cpp::gc::GarbageCollector::AllocateFixed(newSize * sizeof(T), NULL);

                MemCpyData memCpyData = { newData, m_Data, m_Size * sizeof(T) };
                // perform memcpy with GC lock held so GC doesn't see torn pointer values.I think this is less of an issue with Boehm than other GCs, but being safe.
                il2cpp::gc::GarbageCollector::CallWithAllocLockHeld(&CopyValues, &memCpyData);

                il2cpp::gc::GarbageCollector::FreeFixed(m_Data);

                GarbageCollector::SetWriteBarrier((void**)newData, m_Size * sizeof(T));

                m_Size = newSize;
                m_Data = newData;
                IL2CPP_ASSERT(m_Data);
            }

            size_t index = m_Map.size();
            m_Map.insert(std::make_pair(k, index));
            m_Data[index] = value;

            GarbageCollector::SetWriteBarrier((void**)(m_Data + index));

            IL2CPP_ASSERT(m_Map.size() <= m_Size);
            return true;
        }

        bool TryGetValue(const Key& k, T* value)
        {
            ConstIterator iter = m_Map.find(k);
            if (iter == m_Map.end())
                return false;

            size_t index = iter->second;
            IL2CPP_ASSERT(index <= m_Map.size());
            *value = m_Data[index];
            return true;
        }

    private:
        struct MemCpyData
        {
            void* dst;
            const void* src;
            size_t size;
        };

        static void* CopyValues(void* arg)
        {
            MemCpyData* thisPtr = (MemCpyData*)arg;
            memcpy(thisPtr->dst, thisPtr->src, thisPtr->size);
            return NULL;
        }

        Il2CppHashMap<Key, size_t, HashFcn, EqualKey> m_Map;
        T* m_Data;
        size_t m_Size;
    };
}
}
