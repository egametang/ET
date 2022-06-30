#pragma once

#include "gc/GCHandle.h"
#include "vm/CCWBase.h"
#include "vm/Exception.h"
#include "utils/TemplateUtils.h"
#include "utils/Memory.h"

#include "Baselib.h"
#include "Cpp/Atomic.h"

namespace il2cpp
{
namespace vm
{
    template<typename TDerived>
    struct NOVTABLE NonCachedCCWBase : CCWBase
    {
    private:
        baselib::atomic<uint32_t> m_RefCount;
        uint32_t m_GCHandle;

    public:
        inline NonCachedCCWBase(Il2CppObject* obj) :
            CCWBase(obj),
            m_RefCount(1) // We start with a ref count of 1
        {
            m_GCHandle = gc::GCHandle::New(GetManagedObjectInline(), false);
            IL2CPP_ASSERT(m_GCHandle != 0);
            Il2CppStaticAssert(utils::TemplateUtils::IsBaseOf<NonCachedCCWBase<TDerived>, TDerived>::value);
        }

        inline ~NonCachedCCWBase()
        {
            IL2CPP_ASSERT(m_GCHandle != 0);
            gc::GCHandle::Free(m_GCHandle);
            m_GCHandle = 0;
        }

        IL2CPP_FORCE_INLINE uint32_t AddRefImpl()
        {
            return ++m_RefCount;
        }

        IL2CPP_FORCE_INLINE uint32_t ReleaseImpl()
        {
            const uint32_t count = --m_RefCount;
            if (count == 0)
                Destroy();

            return count;
        }

        IL2CPP_FORCE_INLINE static TDerived* __CreateInstance(Il2CppObject* obj)
        {
            void* memory = utils::Memory::Malloc(sizeof(TDerived));
            if (memory == NULL)
                Exception::RaiseOutOfMemoryException();
            return new(memory) TDerived(obj);
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
