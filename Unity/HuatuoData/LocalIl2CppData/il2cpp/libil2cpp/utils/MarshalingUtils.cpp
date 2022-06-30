#include "MarshalingUtils.h"
#include "il2cpp-pinvoke-support.h"

namespace il2cpp
{
namespace utils
{
    void MarshalingUtils::MarshalStructToNative(void* managedStructure, void* marshaledStructure, const Il2CppInteropData* interopData)
    {
#if RUNTIME_TINY
        IL2CPP_ASSERT(0 && "Not supported with the Tiny runtime");
#else
        IL2CPP_ASSERT(interopData);
        IL2CPP_ASSERT(interopData->pinvokeMarshalToNativeFunction);
        interopData->pinvokeMarshalToNativeFunction(managedStructure, marshaledStructure);
#endif
    }

    void MarshalingUtils::MarshalStructFromNative(void* marshaledStructure, void* managedStructure, const Il2CppInteropData* interopData)
    {
#if RUNTIME_TINY
        IL2CPP_ASSERT(0 && "Not supported with the Tiny runtime");
#else
        IL2CPP_ASSERT(interopData);
        IL2CPP_ASSERT(interopData->pinvokeMarshalFromNativeFunction);
        interopData->pinvokeMarshalFromNativeFunction(marshaledStructure, managedStructure);
#endif
    }

    bool MarshalingUtils::MarshalFreeStruct(void* marshaledStructure, const Il2CppInteropData* interopData)
    {
#if RUNTIME_TINY
        IL2CPP_ASSERT(0 && "Not supported with the Tiny runtime");
        return false;
#else
        if (interopData == NULL)
            return false;

        PInvokeMarshalCleanupFunc cleanup = interopData->pinvokeMarshalCleanupFunction;

        if (cleanup == NULL)
            return false;

        cleanup(marshaledStructure);
        return true;
#endif
    }
} // namespace utils
} // namespace il2cpp
