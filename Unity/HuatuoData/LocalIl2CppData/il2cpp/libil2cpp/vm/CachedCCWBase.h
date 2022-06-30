#pragma once

#include "gc/GCHandle.h"
#include "os/Atomic.h"
#include "vm/CCWBase.h"
#include "utils/Memory.h"
#include "utils/TemplateUtils.h"

#include "Baselib.h"
#include "Cpp/Atomic.h"

namespace il2cpp
{
namespace vm
{
// Alright, so the lifetime of this guy is pretty weird
// For a single managed object, the IUnknown of its COM Callable Wrapper must always be the same
// That means that we have to keep the same COM Callable Wrapper alive for an object once we create it
// They are cached in il2cpp::vm::g_CCWCache, which is managed by il2cpp::vm::CCW class
//
// Here comes the tricky part: when a native object has a reference to the COM Callable Wrapper,
// the managed object is not supposed to be garbage collected. However, when no native objects are referencing
// it, it should not prevent the GC from collecting the managed object. We implement this by keeping a GC handle
// on the managed object if our reference count is 1 or more. We acquire it when it gets increased from 0 (this
// is safe because such AddRef can only come when this object is retrieved from CCW Cache) and release the GC
// handle when our reference count gets decreased to 0. Here's a kicker: we don't destroy the COM Callable Wrapper
// when the reference count reaches 0; we instead rely on GC finalizer of the managed object to both remove it from
// CCW cache and also destroy it.
    template<typename TDerived>
    struct NOVTABLE CachedCCWBase : CCWBase
    {
    private:
        baselib::atomic<uint32_t> m_RefCount;
        uint32_t m_GCHandle;

    public:
        inline CachedCCWBase(Il2CppObject* obj) :
            CCWBase(obj),
            m_RefCount(0), // We do not hold any references upon its creation
            m_GCHandle(0)
        {
            Il2CppStaticAssert(utils::TemplateUtils::IsBaseOf<CachedCCWBase<TDerived>, TDerived>::value);
        }

        virtual uint32_t STDCALL AddRef() IL2CPP_OVERRIDE
        {
            return AddRefImpl();
        }

        virtual uint32_t STDCALL Release() IL2CPP_OVERRIDE
        {
            return ReleaseImpl();
        }

        // AddRef can be called at any time whatsoever, as it's called when
        // managed objects are passed to native code
        IL2CPP_NO_INLINE uint32_t AddRefImpl()
        {
            const uint32_t refCount = ++m_RefCount;

            if (refCount == 1)
            {
                // Since AddRef can be called at any time, it's possible that
                // at this point we're in middle of ReleaseImpl call just after
                // it decrements the gccount to 0 but hasn't released m_GCHandle
                // yet. We spin until it is released.
                uint32_t gcHandle = gc::GCHandle::New(GetManagedObjectInline(), false);
                while (os::Atomic::CompareExchange(&m_GCHandle, gcHandle, 0) != 0) {}
            }

            return refCount;
        }

        // Release can be called only if m_RefCount is greater than 0,
        // and the AddRef call that has increased the ref count above 0 has returned
        IL2CPP_NO_INLINE uint32_t ReleaseImpl()
        {
            const uint32_t count = --m_RefCount;
            if (count == 0)
            {
                // We decreased the ref count to 0, so we are responsible
                // for freeing the handle. Only one ReleaseImpl that reduced
                // ref count to 0 will ever be in flight at the same time
                // because AddRefImpl that takes us out of this state halts until
                // we set m_GCHandle to zero.
                uint32_t gcHandle = os::Atomic::Exchange(&m_GCHandle, 0);
                IL2CPP_ASSERT(gcHandle != 0);
                gc::GCHandle::Free(gcHandle);
            }

            return count;
        }

        virtual void STDCALL Destroy() IL2CPP_FINAL IL2CPP_OVERRIDE
        {
            IL2CPP_ASSERT(m_RefCount == 0);

            TDerived* instance = static_cast<TDerived*>(this);
            instance->~TDerived();
            utils::Memory::Free(instance);
        }
    };
}
}
