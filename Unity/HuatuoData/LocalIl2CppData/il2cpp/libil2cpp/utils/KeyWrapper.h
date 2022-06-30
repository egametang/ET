#pragma once

template<class T>
struct KeyWrapper
{
    typedef T wrapped_type;
    typedef KeyWrapper<T> self_type;

    enum KeyTypeEnum { KeyType_Normal, KeyType_Empty, KeyType_Deleted };

    KeyTypeEnum type;
    T key;

    KeyWrapper() : type(KeyType_Normal), key(T()) {}
    KeyWrapper(KeyTypeEnum type_) : type(type_), key(T()) {}
    KeyWrapper(const T& key_) : key(key_), type(KeyType_Normal) {}
    KeyWrapper(const self_type& other) : type(other.type), key(other.key) {}

    operator const T&() const { return key; }
    bool isNormal() const { return (type == KeyType_Normal); }

    template<typename KeyComparer>
    struct EqualsComparer
    {
        EqualsComparer(KeyComparer keyComparer) :
            m_KeyComparer(keyComparer)
        {
        }

        bool operator()(const KeyWrapper<T>& left, const KeyWrapper<T>& right) const
        {
            if (left.type != right.type)
                return false;

            if (!left.isNormal())
                return true;

            return m_KeyComparer(left.key, right.key);
        }

    private:
        KeyComparer m_KeyComparer;
    };
};
