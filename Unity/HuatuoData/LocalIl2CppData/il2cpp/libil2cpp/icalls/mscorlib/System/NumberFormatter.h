#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API NumberFormatter
    {
    public:
        static void GetFormatterTables(uint64_t * * mantissas,
            int32_t * * exponents,
            int16_t * * digitLowerTable,
            int16_t * * digitUpperTable,
            int64_t * * tenPowersList,
            int32_t * * decHexDigits);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
