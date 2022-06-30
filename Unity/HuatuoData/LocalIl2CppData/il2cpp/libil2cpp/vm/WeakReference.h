#pragma once

#include "il2cpp-object-internals.h"

#include "Baselib.h"
#include "Cpp/Atomic.h"

namespace il2cpp
{
namespace vm
{
    struct WeakReference IL2CPP_FINAL : Il2CppIWeakReference
    {
        static il2cpp_hresult_t Create(Il2CppObject* managedObject, Il2CppIWeakReference** result);

        WeakReference(Il2CppObject * managedObject);

        virtual il2cpp_hresult_t STDCALL QueryInterface(const Il2CppGuid& iid, void** object) IL2CPP_FINAL IL2CPP_OVERRIDE;
        virtual uint32_t STDCALL AddRef() IL2CPP_FINAL IL2CPP_OVERRIDE;
        virtual uint32_t STDCALL Release() IL2CPP_FINAL IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL Resolve(const Il2CppGuid& iid, Il2CppIInspectable** object) IL2CPP_FINAL IL2CPP_OVERRIDE;

    private:
        uint32_t m_GCHandle;
        baselib::atomic<uint32_t> m_RefCount;
    };
}
}
