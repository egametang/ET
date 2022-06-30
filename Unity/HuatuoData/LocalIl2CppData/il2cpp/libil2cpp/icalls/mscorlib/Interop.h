#pragma once

struct Il2CppMonoAssemblyName;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
    class LIBIL2CPP_CODEGEN_API Interop
    {
    public:
        class LIBIL2CPP_CODEGEN_API Sys
        {
        public:
            static int32_t DoubleToString(double value, char* format, char* buffer, int32_t bufferLength);
        };
    };
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
