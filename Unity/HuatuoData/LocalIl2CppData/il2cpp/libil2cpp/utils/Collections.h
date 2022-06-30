#pragma once

#include <algorithm>
#include <vector>
#include <functional>
#include "NonCopyable.h"

namespace il2cpp
{
namespace utils
{
namespace collections
{
// Memory compact, map-like data structure that stores values and
// is a able to compute the key from the value with the provided converter
// This data structure is perfect to storing values that will not be changed
// for the duration of program (like type metadata) and need fast querying
//
// It is able to store multiple values associated with a single key, and query them through find()
// find_first() is a special case find() which improves performance for cases where we don't store multiple values for each key
    template<typename TKey, typename TValue, typename TValueToKeyConverter, typename TKeyLess = std::less<TKey>, typename TKeyEquals = std::equal_to<TKey> >
    class ArrayValueMap : NonCopyable
    {
    public:
        typedef ArrayValueMap<TKey, TValue, TValueToKeyConverter, TKeyLess, TKeyEquals> map_type;
        typedef const TValue* iterator;

    private:
        const TValue* m_Values;
        const size_t m_ValueCount;
        bool m_OwnStorage;
        const TValueToKeyConverter m_ValueToKeyConverter;
        const TKeyLess m_KeyLessComparer;
        const TKeyEquals m_KeyEqualsComparer;

        struct SortComparer
        {
        private:
            const TValueToKeyConverter m_ValueToKeyConverter;
            const TKeyLess m_KeyComparer;

        public:
            SortComparer(TValueToKeyConverter valueToKeyConverter, TKeyLess keyComparer) :
                m_ValueToKeyConverter(valueToKeyConverter), m_KeyComparer(keyComparer)
            {
            }

            inline bool operator()(const TValue& left, const TValue& right) const
            {
                return m_KeyComparer(m_ValueToKeyConverter(left), m_ValueToKeyConverter(right));
            }
        };

        struct LowerBoundFindComparer
        {
        private:
            const TValueToKeyConverter m_ValueToKeyConverter;
            const TKeyLess m_KeyComparer;

        public:
            LowerBoundFindComparer(TValueToKeyConverter valueToKeyConverter, TKeyLess keyComparer) :
                m_ValueToKeyConverter(valueToKeyConverter), m_KeyComparer(keyComparer)
            {
            }

            inline bool operator()(const TValue& left, const TKey& right) const
            {
                return m_KeyComparer(m_ValueToKeyConverter(left), right);
            }
        };

        struct UpperBoundFindComparer
        {
        private:
            const TValueToKeyConverter m_ValueToKeyConverter;
            const TKeyLess m_KeyComparer;

        public:
            UpperBoundFindComparer(TValueToKeyConverter valueToKeyConverter, TKeyLess keyComparer) :
                m_ValueToKeyConverter(valueToKeyConverter), m_KeyComparer(keyComparer)
            {
            }

            inline bool operator()(const TKey& left, const TValue& right) const
            {
                return m_KeyComparer(left, m_ValueToKeyConverter(right));
            }
        };

        inline static TValue* InitializeInPlace(TValue* values, size_t valueCount, TValueToKeyConverter valueToKeyConverter, TKeyLess keyLessComparer)
        {
            std::sort(values, values + valueCount, SortComparer(valueToKeyConverter, keyLessComparer));
            return values;
        }

        inline static TValue* AllocateAndInitialize(const TValue* originalValues, size_t valueCount, TValueToKeyConverter valueToKeyConverter, TKeyLess keyLessComparer)
        {
            TValue* values = new TValue[valueCount];
            memcpy(values, originalValues, valueCount * sizeof(TValue));

            return InitializeInPlace(values, valueCount, valueToKeyConverter, keyLessComparer);
        }

    public:
        inline ArrayValueMap() :
            m_Values(NULL),
            m_ValueCount(0),
            m_OwnStorage(false),
            m_ValueToKeyConverter(TValueToKeyConverter()),
            m_KeyLessComparer(TKeyLess()),
            m_KeyEqualsComparer(TKeyEquals())
        {
        }

        // Non-allocating constructor. It will take a pointer and will not allocate any storage
        // It WILL sort existing values
        inline ArrayValueMap(TValue* values, size_t valueCount, TValueToKeyConverter valueToKeyConverter = TValueToKeyConverter(),
                             TKeyLess keyLessComparer = TKeyLess(), TKeyEquals keyEqualsComparer = TKeyEquals()) :
            m_Values(InitializeInPlace(values, valueCount, valueToKeyConverter, keyLessComparer)),
            m_ValueCount(valueCount),
            m_ValueToKeyConverter(valueToKeyConverter),
            m_KeyLessComparer(keyLessComparer),
            m_KeyEqualsComparer(keyEqualsComparer),
            m_OwnStorage(false)
        {
        }

        // Allocating constructor
        // Will copy values to newly allocated storage
        inline ArrayValueMap(const std::vector<TValue>& values, TValueToKeyConverter valueToKeyConverter = TValueToKeyConverter(),
                             TKeyLess keyLessComparer = TKeyLess(), TKeyEquals keyEqualsComparer = TKeyEquals()) :
            m_Values(AllocateAndInitialize(values.data(), values.size(), valueToKeyConverter, keyLessComparer)),
            m_ValueCount(values.size()),
            m_ValueToKeyConverter(valueToKeyConverter),
            m_KeyLessComparer(keyLessComparer),
            m_KeyEqualsComparer(keyEqualsComparer),
            m_OwnStorage(true)
        {
        }

        ~ArrayValueMap()
        {
            if (m_OwnStorage)
            {
                delete[] m_Values;
            }
        }

        inline void assign_external(TValue* values, size_t valueCount, TValueToKeyConverter valueToKeyConverter = TValueToKeyConverter(),
            TKeyLess keyLessComparer = TKeyLess(), TKeyEquals keyEqualsComparer = TKeyEquals())
        {
            this->~ArrayValueMap();
            new(this) map_type(values, valueCount, valueToKeyConverter, keyLessComparer, keyEqualsComparer);
        }

        // Constructs map that contains pointers to original array
        inline void assign_addresses(const TValue& valueArray, size_t valueCount, TValueToKeyConverter valueToKeyConverter = TValueToKeyConverter(),
            TKeyLess keyLessComparer = TKeyLess(), TKeyEquals keyEqualsComparer = TKeyEquals())
        {
            this->~ArrayValueMap();

            TValue* storage = NULL;

            if (valueCount > 0)
            {
                storage = new TValue[valueCount];

                for (size_t i = 0; i < valueCount; i++)
                {
                    storage[i] = &valueArray[i];
                }
            }

            new(this) map_type(storage, valueCount, valueToKeyConverter, keyLessComparer, keyEqualsComparer);
            m_OwnStorage = true;
        }

        inline void assign(const std::vector<TValue>& values, TValueToKeyConverter valueToKeyConverter = TValueToKeyConverter(),
            TKeyLess keyLessComparer = TKeyLess(), TKeyEquals keyEqualsComparer = TKeyEquals())
        {
            this->~ArrayValueMap();
            new(this) map_type(values, valueToKeyConverter, keyLessComparer, keyEqualsComparer);
        }

        inline iterator begin() const
        {
            return m_Values;
        }

        inline iterator end() const
        {
            return m_Values + m_ValueCount;
        }

        template<typename EqualsPredicate>
        inline iterator find(const TKey& key, const EqualsPredicate& equalsPredicate) const
        {
            iterator dataStart = begin();
            iterator dataEnd = end();
            iterator ptr = std::lower_bound(dataStart, dataEnd, key, LowerBoundFindComparer(m_ValueToKeyConverter, m_KeyLessComparer));

            for (; ptr != dataEnd && m_KeyEqualsComparer(m_ValueToKeyConverter(*ptr), key); ptr++)
            {
                if (equalsPredicate(*ptr))
                    return ptr;
            }

            return dataEnd;
        }

        inline iterator find_first(const TKey& key) const
        {
            iterator dataStart = begin();
            iterator dataEnd = end();
            iterator ptr = std::lower_bound(dataStart, dataEnd, key, LowerBoundFindComparer(m_ValueToKeyConverter, m_KeyLessComparer));

            if (ptr != dataEnd && m_KeyEqualsComparer(m_ValueToKeyConverter(*ptr), key))
                return ptr;

            return dataEnd;
        }

        inline iterator lower_bound(const TKey& key) const
        {
            return std::lower_bound(begin(), end(), key, LowerBoundFindComparer(m_ValueToKeyConverter, m_KeyLessComparer));
        }

        inline iterator upper_bound(const TKey& key) const
        {
            return std::upper_bound(begin(), end(), key, UpperBoundFindComparer(m_ValueToKeyConverter, m_KeyLessComparer));
        }

        inline size_t size() const
        {
            return m_ValueCount;
        }

        inline const TValue& operator[](size_t i) const
        {
            return m_Values[i];
        }

        template<typename Mutator>
        inline void mutate(Mutator& mutator)
        {
            size_t count = m_ValueCount;
            const TValue* values = m_Values;
            for (size_t i = 0; i < count; i++)
                mutator(const_cast<TValue*>(values + i));

            m_Values = InitializeInPlace(const_cast<TValue*>(values), count, m_ValueToKeyConverter, m_KeyLessComparer);
        }
    };
}
}   // namespace utils
}   // namespace il2cpp
