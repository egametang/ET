#include "il2cpp-config.h"
#include "os/Atomic.h"
#include "vm/COM.h"
#include "vm/ComObjectBase.h"

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::GetIids(uint32_t* iidCount, Il2CppGuid** iids)
{
    *iidCount = 0;
    *iids = NULL;
    return IL2CPP_S_OK;
}

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::GetRuntimeClassName(Il2CppHString* className)
{
    return GetRuntimeClassNameImpl(className);
}

il2cpp_hresult_t il2cpp::vm::ComObjectBase::GetRuntimeClassNameImpl(Il2CppHString* className)
{
    *className = NULL;
    return IL2CPP_S_OK;
}

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::GetTrustLevel(int32_t* trustLevel)
{
    *trustLevel = 0;
    return IL2CPP_S_OK;
}

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::GetUnmarshalClass(const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags, Il2CppGuid* clsid)
{
    Il2CppIMarshal* freeThreadedMarshaler;
    il2cpp_hresult_t hr = GetFreeThreadedMarshalerNoAddRef(&freeThreadedMarshaler);
    if (IL2CPP_HR_FAILED(hr))
        return hr;

    return freeThreadedMarshaler->GetUnmarshalClass(iid, object, context, reserved, flags, clsid);
}

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::GetMarshalSizeMax(const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags, uint32_t* size)
{
    Il2CppIMarshal* freeThreadedMarshaler;
    il2cpp_hresult_t hr = GetFreeThreadedMarshalerNoAddRef(&freeThreadedMarshaler);
    if (IL2CPP_HR_FAILED(hr))
        return hr;

    return freeThreadedMarshaler->GetMarshalSizeMax(iid, object, context, reserved, flags, size);
}

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::MarshalInterface(Il2CppIStream* stream, const Il2CppGuid& iid, void* object, uint32_t context, void* reserved, uint32_t flags)
{
    Il2CppIMarshal* freeThreadedMarshaler;
    il2cpp_hresult_t hr = GetFreeThreadedMarshalerNoAddRef(&freeThreadedMarshaler);
    if (IL2CPP_HR_FAILED(hr))
        return hr;

    return freeThreadedMarshaler->MarshalInterface(stream, iid, object, context, reserved, flags);
}

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::UnmarshalInterface(Il2CppIStream* stream, const Il2CppGuid& iid, void** object)
{
    Il2CppIMarshal* freeThreadedMarshaler;
    il2cpp_hresult_t hr = GetFreeThreadedMarshalerNoAddRef(&freeThreadedMarshaler);
    if (IL2CPP_HR_FAILED(hr))
        return hr;

    return freeThreadedMarshaler->UnmarshalInterface(stream, iid, object);
}

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::ReleaseMarshalData(Il2CppIStream* stream)
{
    Il2CppIMarshal* freeThreadedMarshaler;
    il2cpp_hresult_t hr = GetFreeThreadedMarshalerNoAddRef(&freeThreadedMarshaler);
    if (IL2CPP_HR_FAILED(hr))
        return hr;

    return freeThreadedMarshaler->ReleaseMarshalData(stream);
}

il2cpp_hresult_t STDCALL il2cpp::vm::ComObjectBase::DisconnectObject(uint32_t reserved)
{
    Il2CppIMarshal* freeThreadedMarshaler;
    il2cpp_hresult_t hr = GetFreeThreadedMarshalerNoAddRef(&freeThreadedMarshaler);
    if (IL2CPP_HR_FAILED(hr))
        return hr;

    return freeThreadedMarshaler->DisconnectObject(reserved);
}

il2cpp_hresult_t il2cpp::vm::ComObjectBase::GetFreeThreadedMarshalerNoAddRef(Il2CppIMarshal** destination)
{
    Il2CppIMarshal* freeThreadedMarshaler = m_FreeThreadedMarshaler;
    if (freeThreadedMarshaler == NULL)
    {
        // We don't really want to aggregate FTM, as then we'd have to store its IUnknown too
        // So we pass NULL as the first parameter
        Il2CppIUnknown* freeThreadedMarshalerUnknown;
        il2cpp_hresult_t hr = COM::CreateFreeThreadedMarshaler(NULL, &freeThreadedMarshalerUnknown);
        if (IL2CPP_HR_FAILED(hr))
            return hr;

        hr = freeThreadedMarshalerUnknown->QueryInterface(Il2CppIMarshal::IID, reinterpret_cast<void**>(&freeThreadedMarshaler));
        freeThreadedMarshalerUnknown->Release();
        if (IL2CPP_HR_FAILED(hr))
            return hr;

        if (os::Atomic::CompareExchangePointer<Il2CppIMarshal>(&m_FreeThreadedMarshaler, freeThreadedMarshaler, NULL) != NULL)
        {
            freeThreadedMarshaler->Release();
            freeThreadedMarshaler = m_FreeThreadedMarshaler;
        }
    }

    *destination = freeThreadedMarshaler;
    return IL2CPP_S_OK;
}
