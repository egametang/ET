#pragma once

#include <string.h>

namespace il2cpp
{
namespace utils
{
    class BaselibHandleUtils
    {
    public:
        template<typename BaselibHandleType>
        static void* HandleToVoidPtr(BaselibHandleType baselibHandle)
        {
            // following asserts check that the handle fits into void* in its entirety
            static_assert(sizeof(BaselibHandleType) <= sizeof(void*), "baselib handle does not fit void*");
            static_assert(sizeof(BaselibHandleType::handle) <= sizeof(void*), "baselib handle does not fit void*");
            void* result = nullptr;
            memcpy(&result, &baselibHandle.handle, sizeof(result));
            return result;
        }

        template<typename BaselibHandleType>
        static BaselibHandleType VoidPtrToHandle(void* ptr)
        {
            static_assert(sizeof(BaselibHandleType) <= sizeof(void*), "baselib handle does not fit void*");
            static_assert(sizeof(BaselibHandleType::handle) <= sizeof(void*), "baselib handle does not fit void*");
            decltype(BaselibHandleType::handle)result = {};
            memcpy(&result, &ptr, sizeof(ptr));
            return BaselibHandleType { result };
        }
    };
}
}
