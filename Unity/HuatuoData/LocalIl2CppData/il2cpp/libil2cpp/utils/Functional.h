#pragma once

#include "utils/NonCopyable.h"

namespace il2cpp
{
namespace utils
{
namespace functional
{
    struct TrueFilter
    {
        template<typename T>
        inline bool operator()(const T& item) const
        {
            return true;
        }
    };

    template<typename ItemType, typename Comparer>
    struct Filter : NonCopyable
    {
    private:
        ItemType m_Item;
        Comparer m_Comparer;

    public:
        Filter(ItemType item, Comparer comparer = Comparer()) :
            m_Item(item), m_Comparer(comparer)
        {
        }

        template<typename T>
        inline bool operator()(const T& item) const
        {
            return m_Comparer(m_Item, item);
        }
    };
}   // functional
}   // utils
}   // il2cpp
