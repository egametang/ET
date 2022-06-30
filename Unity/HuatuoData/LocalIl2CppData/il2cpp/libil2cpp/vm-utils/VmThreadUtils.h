#pragma once

#include "il2cpp-config.h"

enum Il2CppStackPointerResult
{
    Il2CppStackPointerNotSupported = -1,
    Il2CppStackPointerIsNotOnStack = 0,
    Il2CppStackPointerIsOnStack = 1,
};


namespace il2cpp
{
namespace utils
{
    class LIBIL2CPP_CODEGEN_API VmThreadUtils
    {
    public:
        static Il2CppStackPointerResult PointerIsOnCurrentThreadStack(void* ptr);
    };
}     // namespace utils
} // namespace il2cpp

#define IL2CPP_ASSERT_STACK_PTR(ptr) IL2CPP_ASSERT(ptr != NULL && il2cpp::utils::VmThreadUtils::PointerIsOnCurrentThreadStack(ptr) != Il2CppStackPointerIsNotOnStack)
