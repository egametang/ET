#pragma once

#include "os/WindowsRuntime.h"
#include "vm/ComObjectBase.h"
#include "vm/Exception.h"
#include "utils/Memory.h"
#include "utils/TemplateUtils.h"

#include "Baselib.h"
#include "Cpp/Atomic.h"

#include <new>

namespace il2cpp
{
namespace vm
{
    template<typename TDerived>
    struct NOVTABLE ActivationFactoryBase : public ComObjectBase, Il2CppIActivationFactory
    {
    private:
        baselib::atomic<uint32_t> m_RefCount;

    public:
        ActivationFactoryBase() :
            m_RefCount(1) // We start with a ref count of 1
        {
            Il2CppStaticAssert(utils::TemplateUtils::IsBaseOf<ActivationFactoryBase<TDerived>, TDerived>::value);
        }

        virtual il2cpp_hresult_t STDCALL ActivateInstance(Il2CppIInspectable** instance) IL2CPP_OVERRIDE
        {
            return IL2CPP_E_NOTIMPL;
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

        IL2CPP_FORCE_INLINE il2cpp_hresult_t GetRuntimeClassNameImpl(Il2CppHString* className)
        {
            utils::StringView<Il2CppNativeChar> classNameView(IL2CPP_NATIVE_STRING("System.Runtime.InteropServices.WindowsRuntime.IActivationFactory"));
            return os::WindowsRuntime::CreateHString(classNameView, className);
        }

        IL2CPP_FORCE_INLINE static TDerived* __CreateInstance()
        {
            void* memory = utils::Memory::Malloc(sizeof(TDerived));
            if (memory == NULL)
                Exception::RaiseOutOfMemoryException();
            return new(memory) TDerived;
        }

    private:
        IL2CPP_FORCE_INLINE void Destroy()
        {
            IL2CPP_ASSERT(m_RefCount == 0);

            TDerived* instance = static_cast<TDerived*>(this);
            instance->~TDerived();
            utils::Memory::Free(instance);
        }
    };
}
}
