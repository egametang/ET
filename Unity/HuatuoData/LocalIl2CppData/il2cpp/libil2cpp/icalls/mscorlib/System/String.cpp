#include "il2cpp-config.h"
#include <memory>
#include "il2cpp-api.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "icalls/mscorlib/System/String.h"
#include "utils/StringUtils.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/String.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    Il2CppString* String::FastAllocateString(int32_t length)
    {
        return vm::String::NewSize(length);
    }

    Il2CppString* String::InternalIntern(Il2CppString* str)
    {
        return il2cpp_string_intern(str);
    }

    Il2CppString* String::InternalIsInterned(Il2CppString* str)
    {
        return il2cpp_string_is_interned(str);
    }

    void String::RedirectToCreateString()
    {
        NOT_SUPPORTED_IL2CPP(String::RedirectToCreateString, "All String constructors should be redirected to String.CreateString.");
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
