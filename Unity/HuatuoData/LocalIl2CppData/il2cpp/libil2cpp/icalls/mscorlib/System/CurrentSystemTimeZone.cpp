#include "il2cpp-config.h"
#include <string>
#include "gc/WriteBarrier.h"
#include "icalls/mscorlib/System/CurrentSystemTimeZone.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "vm/Array.h"
#include "vm/Exception.h"
#include "vm/String.h"
#include "os/TimeZone.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    bool CurrentSystemTimeZone::GetTimeZoneData40(int year, Il2CppArray** data, Il2CppArray** names, bool* daylight_inverted)
    {
        int64_t dataTemp[4] = {0};
        std::string namesTemp[2];
        IL2CPP_CHECK_ARG_NULL(data);
        IL2CPP_CHECK_ARG_NULL(names);

        gc::WriteBarrier::GenericStore(data, vm::Array::New(il2cpp_defaults.int64_class, 4));
        gc::WriteBarrier::GenericStore(names, vm::Array::New(il2cpp_defaults.string_class, 2));
        if (!os::TimeZone::GetTimeZoneData(year, dataTemp, namesTemp, daylight_inverted))
            return false;

        for (int i = 0; i < 4; i++)
            il2cpp_array_set((*data), int64_t, i, dataTemp[i]);

        for (int i = 0; i < 2; i++)
            il2cpp_array_setref((*names), i, vm::String::New(namesTemp[i].c_str()));

        return true;
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
