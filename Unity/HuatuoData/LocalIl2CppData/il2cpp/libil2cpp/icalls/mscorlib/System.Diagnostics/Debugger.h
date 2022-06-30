#pragma once

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
    class LIBIL2CPP_CODEGEN_API Debugger
    {
    public:
        static bool IsAttached_internal();
        static bool IsLogging();
        static void Log_icall(int32_t level, Il2CppString** category, Il2CppString** message);
    };
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
