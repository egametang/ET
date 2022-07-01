#pragma once

#include <stdint.h>
#include <vector>
#include "il2cpp-vm-support.h"

namespace il2cpp
{
namespace utils
{
    class NativeSymbol
    {
    public:
#if IL2CPP_ENABLE_NATIVE_STACKTRACES
        static void RegisterMethods(const std::vector<MethodDefinitionKey>& managedMethods);
        static const VmMethod* GetMethodFromNativeSymbol(Il2CppMethodPointer nativeMethod);
        static bool GetMethodDebugInfo(const MethodInfo* method, Il2CppMethodDebugInfo* methodDebugInfo);
#endif
    };
} /* namespace vm */
} /* namespace mono */
