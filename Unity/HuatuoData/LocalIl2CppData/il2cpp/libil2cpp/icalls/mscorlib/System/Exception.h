#pragma once

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API Exception
    {
    public:
        static bool nIsTransient(int32_t hr);
        static Il2CppObject* GetMethodFromStackTrace(Il2CppObject* stackTrace);
        static void ReportUnhandledException(Il2CppException* exception);
    };
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
