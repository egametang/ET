#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppString;
struct Il2CppCultureInfo;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Globalization
{
    class LIBIL2CPP_CODEGEN_API CultureInfo
    {
    public:
        static bool construct_internal_locale_from_lcid(Il2CppCultureInfo* cultureInfo, int lcid);
        static bool construct_internal_locale_from_name(Il2CppCultureInfo* cultureInfo, Il2CppString* name);
        static Il2CppArray* internal_get_cultures(bool neutral, bool specific, bool installed);
        static Il2CppString* get_current_locale_name();
    };
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
