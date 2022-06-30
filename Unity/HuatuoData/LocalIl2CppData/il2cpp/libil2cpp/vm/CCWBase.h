#pragma once

#include "vm/ComObjectBase.h"

namespace il2cpp
{
namespace vm
{
    struct LIBIL2CPP_CODEGEN_API NOVTABLE CCWBase : ComObjectBase, Il2CppIManagedObjectHolder, Il2CppIWeakReferenceSource
    {
    private:
        Il2CppObject* m_ManagedObject;

    public:
        inline CCWBase(Il2CppObject* obj) :
            m_ManagedObject(obj)
        {
            IL2CPP_ASSERT(obj != NULL);
        }

        IL2CPP_FORCE_INLINE Il2CppObject* GetManagedObjectInline() const
        {
            return m_ManagedObject;
        }

        il2cpp_hresult_t GetRuntimeClassNameImpl(Il2CppHString* className);

        virtual Il2CppObject* STDCALL GetManagedObject() IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL GetWeakReference(Il2CppIWeakReference** weakReference) IL2CPP_FINAL IL2CPP_OVERRIDE;
    };
}
}
