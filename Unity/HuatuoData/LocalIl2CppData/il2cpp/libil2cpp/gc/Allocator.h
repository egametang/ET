#pragma once
#include "GarbageCollector.h"

namespace il2cpp
{
namespace gc
{
    template<typename T>
    class Allocator
    {
    public:
        typedef size_t    size_type;
        typedef ptrdiff_t difference_type;
        typedef T*        pointer;
        typedef const T*  const_pointer;
        typedef T&        reference;
        typedef const T&  const_reference;
        typedef T         value_type;
        typedef Allocator<T> allocator_type;
        Allocator() {}
        Allocator(const Allocator&) {}

        pointer allocate(size_type n, const void * = 0)
        {
            T* t = (T*)GarbageCollector::AllocateFixed(n * sizeof(T), 0);
            return t;
        }

        void deallocate(void* p, size_type)
        {
            if (p)
            {
                GarbageCollector::FreeFixed(p);
            }
        }

        pointer address(reference x) const { return &x; }
        const_pointer address(const_reference x) const { return &x; }
        Allocator<T>& operator=(const Allocator&) { return *this; }
        void construct(pointer p, const T& val) { new((T*)p) T(val); }
        void destroy(pointer p) { p->~T(); }

        size_type max_size() const { return size_t(-1); }

        template<class U>
        struct rebind { typedef Allocator<U> other; };

        template<class U>
        Allocator(const Allocator<U>&) {}

        template<class U>
        Allocator& operator=(const Allocator<U>&) { return *this; }
    };
}
}
