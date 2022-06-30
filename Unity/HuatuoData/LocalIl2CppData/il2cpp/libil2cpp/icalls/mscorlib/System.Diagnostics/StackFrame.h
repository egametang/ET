#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppString;
struct Il2CppReflectionMethod;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Diagnostics
{
    class LIBIL2CPP_CODEGEN_API StackFrame
    {
    public:
        static bool get_frame_info(
            int32_t skip,
            bool needFileInfo,
            Il2CppReflectionMethod ** method,
            int32_t* iloffset,
            int32_t* native_offset,
            Il2CppString** file,
            int32_t* line,
            int32_t* column);
    };
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
