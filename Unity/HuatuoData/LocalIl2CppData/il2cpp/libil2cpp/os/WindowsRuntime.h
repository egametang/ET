#pragma once

#include "il2cpp-windowsruntime-types.h"
#include "utils/Expected.h"
#include "utils/StringView.h"

namespace il2cpp
{
namespace os
{
    class LIBIL2CPP_CODEGEN_API WindowsRuntime
    {
    public:
        static il2cpp_hresult_t GetActivationFactory(Il2CppHString className, Il2CppIActivationFactory** activationFactory);

        static il2cpp_hresult_t CreateHStringReference(const utils::StringView<Il2CppNativeChar>& str, Il2CppHStringHeader* header, Il2CppHString* hstring);
        static il2cpp_hresult_t CreateHString(const utils::StringView<Il2CppChar>& str, Il2CppHString* hstring);
#if !IL2CPP_TARGET_WINDOWS // Il2CppChar and Il2CppNativeChar are the same on Windows
        static il2cpp_hresult_t CreateHString(const utils::StringView<Il2CppNativeChar>& str, Il2CppHString* hstring);
#endif
        static il2cpp_hresult_t DuplicateHString(Il2CppHString hstring, Il2CppHString* duplicated);
        static il2cpp_hresult_t DeleteHString(Il2CppHString hstring);

        static utils::Expected<const Il2CppChar*> GetHStringBuffer(Il2CppHString hstring, uint32_t* length);
        static utils::Expected<const Il2CppNativeChar*> GetNativeHStringBuffer(Il2CppHString hstring, uint32_t* length);

        static utils::Expected<il2cpp_hresult_t> PreallocateHStringBuffer(uint32_t length, Il2CppNativeChar** mutableBuffer, void** bufferHandle);
        static utils::Expected<il2cpp_hresult_t> PromoteHStringBuffer(void* bufferHandle, Il2CppHString* hstring);
        static utils::Expected<il2cpp_hresult_t> DeleteHStringBuffer(void* bufferHandle);

        static Il2CppIRestrictedErrorInfo* GetRestrictedErrorInfo();
        typedef Il2CppIUnknown* (*GetOrCreateFunc)(Il2CppObject* obj, const Il2CppGuid& iid);
        static void OriginateLanguageException(il2cpp_hresult_t hresult, Il2CppException* ex, Il2CppString* exceptionString, GetOrCreateFunc createCCWCallback);

        static void EnableErrorReporting();
    };
}
}
