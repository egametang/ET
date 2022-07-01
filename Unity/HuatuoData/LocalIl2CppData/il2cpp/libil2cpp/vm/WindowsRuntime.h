#pragma once
#include "il2cpp-vm-support.h"
#include "os/WindowsRuntime.h"
#include "vm/Exception.h"
#include "utils/StringView.h"

struct Il2CppIActivationFactory;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API WindowsRuntime
    {
    public:
        static Il2CppIActivationFactory* GetActivationFactory(const utils::StringView<Il2CppNativeChar>& runtimeClassName);

        static inline void CreateHStringReference(const utils::StringView<Il2CppNativeChar>& str, Il2CppHStringHeader* header, Il2CppHString* hstring)
        {
            il2cpp_hresult_t hr = os::WindowsRuntime::CreateHStringReference(str, header, hstring);
            IL2CPP_VM_RAISE_IF_FAILED(hr, false);
        }

        static inline Il2CppHString CreateHString(Il2CppString* str)
        {
            Il2CppHString result;
            il2cpp_hresult_t hr = os::WindowsRuntime::CreateHString(utils::StringView<Il2CppChar>(str->chars, str->length), &result);
            IL2CPP_VM_RAISE_IF_FAILED(hr, false);
            return result;
        }

        static inline Il2CppHString CreateHString(const utils::StringView<Il2CppNativeChar>& str)
        {
            Il2CppHString result;
            il2cpp_hresult_t hr = os::WindowsRuntime::CreateHString(str, &result);
            IL2CPP_VM_RAISE_IF_FAILED(hr, false);
            return result;
        }

        static inline void DeleteHString(Il2CppHString hstring)
        {
            il2cpp_hresult_t hr = os::WindowsRuntime::DeleteHString(hstring);
            IL2CPP_VM_RAISE_IF_FAILED(hr, false);
        }

        static inline Il2CppString* HStringToManagedString(Il2CppHString hstring)
        {
            return os::WindowsRuntime::HStringToManagedString(hstring);
        }

        static inline void* PreallocateHStringBuffer(uint32_t length, Il2CppNativeChar** buffer)
        {
            void* bufferHandle;
            il2cpp_hresult_t hr = os::WindowsRuntime::PreallocateHStringBuffer(length, buffer, &bufferHandle);
            IL2CPP_VM_RAISE_IF_FAILED(hr, false);
            return bufferHandle;
        }

        static inline Il2CppHString PromoteHStringBuffer(void* bufferHandle)
        {
            Il2CppHString hstring;
            il2cpp_hresult_t hr = os::WindowsRuntime::PromoteHStringBuffer(bufferHandle, &hstring);

            if (IL2CPP_HR_FAILED(hr))
            {
                // Prevent memory leaks by deleting the hstring buffer that was supposed to be promoted before raising an exception
                os::WindowsRuntime::DeleteHStringBuffer(bufferHandle);
                IL2CPP_VM_RAISE_COM_EXCEPTION(hr, false);
            }

            return hstring;
        }

        static void MarshalTypeToNative(const Il2CppType* type, Il2CppWindowsRuntimeTypeName& nativeType);
        static const Il2CppType* MarshalTypeFromNative(Il2CppWindowsRuntimeTypeName& nativeType);
        static inline void DeleteNativeType(Il2CppWindowsRuntimeTypeName& nativeType)
        {
            DeleteHString(nativeType.typeName);
        }
    };
}
}
