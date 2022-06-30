#pragma once

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppComObject;
struct Il2CppGuid;
struct Il2CppIUnknown;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API RCW
    {
    public:
        static void Register(Il2CppComObject* rcw);
        static Il2CppObject* GetOrCreateFromIUnknown(Il2CppIUnknown* unknown, Il2CppClass* fallbackClass);
        static Il2CppObject* GetOrCreateFromIInspectable(Il2CppIInspectable* inspectable, Il2CppClass* fallbackClass);
        static Il2CppObject* GetOrCreateForSealedClass(Il2CppIUnknown* unknown, Il2CppClass* objectClass);
        static void Cleanup(Il2CppComObject* rcw);
        static bool CacheQueriedInterface(Il2CppComObject* rcw, const Il2CppGuid& iid, Il2CppIUnknown* queriedInterface);
        static const VirtualInvokeData* GetComInterfaceInvokeData(Il2CppComObject* rcw, const Il2CppClass* targetInterface, Il2CppMethodSlot slot);

        template<bool throwOnError>
        inline static Il2CppIUnknown* QueryInterfaceNoAddRef(Il2CppComObject* rcw, const Il2CppGuid& iid)
        {
            IL2CPP_ASSERT(rcw);
            IL2CPP_ASSERT(rcw->identity);

            Il2CppIUnknown* result = QueryInterfaceCached(rcw, iid);
            if (result != NULL)
                return result;

            const il2cpp_hresult_t hr = rcw->identity->QueryInterface(iid, reinterpret_cast<void**>(&result));
            if (IL2CPP_HR_FAILED(hr))
            {
                if (throwOnError)
                    Exception::Raise(hr, true);

                return NULL;
            }

            IL2CPP_ASSERT(result);
            if (!CacheQueriedInterface(rcw, iid, result))
            {
                // We lost the race - another thread won it.
                result->Release();
                return QueryInterfaceCached(rcw, iid);
            }

            return result;
        }

    private:
        static Il2CppIUnknown* QueryInterfaceCached(Il2CppComObject* rcw, const Il2CppGuid& iid);
        static const VirtualInvokeData* GetComInterfaceInvokeData(Il2CppClass* queriedInterface, const Il2CppClass* targetInterface, Il2CppMethodSlot slot);
    };
} /* namespace vm */
} /* namespace il2cpp */
