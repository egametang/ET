#pragma once
#include <il2cpp-object-internals.h>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Reflection
{
    class LIBIL2CPP_CODEGEN_API RuntimeEventInfo
    {
    public:
        static int32_t get_metadata_token(Il2CppObject* monoEvent);
        static void get_event_info(Il2CppReflectionMonoEvent* event, Il2CppReflectionMonoEventInfo* eventInfo);
    };
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
