#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API LastError
    {
    public:
        static uint32_t GetLastError();
        static void StoreLastError();

        static void InitializeLastErrorThreadStatic();

    private:
        static int32_t s_LastErrorThreadLocalStorageOffset;
    };
} /* namespace vm */
} /* namespace il2cpp */
