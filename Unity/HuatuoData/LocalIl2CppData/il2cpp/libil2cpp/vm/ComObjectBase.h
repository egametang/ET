#pragma once

#include "il2cpp-object-internals.h"

namespace il2cpp
{
namespace vm
{
    struct LIBIL2CPP_CODEGEN_API NOVTABLE ComObjectBase : Il2CppIInspectable, Il2CppIMarshal
    {
    private:
        Il2CppIMarshal* m_FreeThreadedMarshaler;

    public:
        inline ComObjectBase() :
            m_FreeThreadedMarshaler(NULL)
        {
        }

        inline ~ComObjectBase()
        {
            if (m_FreeThreadedMarshaler)
                m_FreeThreadedMarshaler->Release();
        }

        virtual il2cpp_hresult_t STDCALL GetIids(uint32_t* iidCount, Il2CppGuid** iids) IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL GetRuntimeClassName(Il2CppHString* className) IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL GetTrustLevel(int32_t* trustLevel) IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL GetUnmarshalClass(const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags, Il2CppGuid* clsid) IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL GetMarshalSizeMax(const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags, uint32_t* size) IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL MarshalInterface(Il2CppIStream* stream, const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags) IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL UnmarshalInterface(Il2CppIStream* stream, const Il2CppGuid& iid, void** object) IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL ReleaseMarshalData(Il2CppIStream* stream) IL2CPP_OVERRIDE;
        virtual il2cpp_hresult_t STDCALL DisconnectObject(uint32_t reserved) IL2CPP_OVERRIDE;

    protected:
        IL2CPP_FORCE_INLINE Il2CppIInspectable* GetIdentity()
        {
            return this;
        }

        il2cpp_hresult_t GetRuntimeClassNameImpl(Il2CppHString* className);

    private:
        ComObjectBase(const ComObjectBase&);
        ComObjectBase& operator=(const ComObjectBase&);

        il2cpp_hresult_t GetFreeThreadedMarshalerNoAddRef(Il2CppIMarshal** destination);
    };
} /* namespace vm */
} /* namespace il2cpp */
