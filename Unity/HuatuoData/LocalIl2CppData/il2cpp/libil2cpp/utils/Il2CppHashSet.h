#pragma once

// Mono code also has define for GROUP_SIZE, so we need to wrap its usage here
#pragma push_macro("GROUP_SIZE")
#undef GROUP_SIZE
#if IL2CPP_USE_SPARSEHASH
#include "../../external/google/sparsehash/sparse_hash_set.h"
#else
#include "../../external/google/sparsehash/dense_hash_set.h"
#endif
#pragma pop_macro("GROUP_SIZE")

#include "KeyWrapper.h"

template<class Value,
         class HashFcn = SPARSEHASH_HASH<Value>,
         class EqualKey = std::equal_to<Value>,
         class Alloc = GOOGLE_NAMESPACE::libc_allocator_with_realloc<KeyWrapper<Value> > >
#if IL2CPP_USE_SPARSEHASH
class Il2CppHashSet : public GOOGLE_NAMESPACE::sparse_hash_set<KeyWrapper<Value>, HashFcn, typename KeyWrapper<Value>::template EqualsComparer<EqualKey>, Alloc>
#else
class Il2CppHashSet : public GOOGLE_NAMESPACE::dense_hash_set<KeyWrapper<Value>, HashFcn, typename KeyWrapper<Value>::template EqualsComparer<EqualKey>, Alloc>
#endif
{
private:
#if IL2CPP_USE_SPARSEHASH
    typedef GOOGLE_NAMESPACE::sparse_hash_set<KeyWrapper<Value>, HashFcn, typename KeyWrapper<Value>::template EqualsComparer<EqualKey>, Alloc> Base;
#else
    typedef GOOGLE_NAMESPACE::dense_hash_set<KeyWrapper<Value>, HashFcn, typename KeyWrapper<Value>::template EqualsComparer<EqualKey>, Alloc> Base;
#endif

public:
    typedef typename Base::size_type size_type;
    typedef typename Base::hasher hasher;
    typedef typename Base::key_equal key_equal;
    typedef typename Base::key_type key_type;

    explicit Il2CppHashSet(size_type n = 0,
                           const hasher& hf = hasher(),
                           const EqualKey& eql = EqualKey()) :
        Base(n, hf, key_equal(eql))
    {
        Base::set_deleted_key(key_type(key_type::KeyType_Deleted));
#if !IL2CPP_USE_SPARSEHASH
        Base::set_empty_key(key_type(key_type::KeyType_Empty));
#endif
    }

    template<class InputIterator>
    Il2CppHashSet(InputIterator f, InputIterator l,
                  size_type n = 0,
                  const hasher& hf = hasher(),
                  const EqualKey& eql = EqualKey()) :
        Base(f, l, n, hf, key_equal(eql))
    {
        Base::set_deleted_key(key_type(key_type::KeyType_Deleted));
#if !IL2CPP_USE_SPARSEHASH
        Base::set_empty_key(key_type(key_type::KeyType_Empty));
#endif
    }
};
