#include "utils/Exception.h"
#include "utils/StringUtils.h"
#include "il2cpp-object-internals.h"

struct Il2CppClass;

namespace il2cpp
{
namespace utils
{
    std::string Exception::FormatException(const Il2CppException* ex)
    {
#if RUNTIME_TINY
        IL2CPP_ASSERT(0 && "Exceptions are not supported with the Tiny runtime");
        return std::string();
#else
        std::string exception_namespace = ex->klass->namespaze;
        std::string exception_type = ex->klass->name;
        if (ex->message)
            return exception_namespace + "." + exception_type + ": " + il2cpp::utils::StringUtils::Utf16ToUtf8(il2cpp::utils::StringUtils::GetChars(ex->message));
        else
            return exception_namespace + "." + exception_type;
#endif
    }

    std::string Exception::FormatInvalidCastException(const Il2CppClass* fromType, const Il2CppClass* toType)
    {
        std::string message;
#if RUNTIME_TINY
        IL2CPP_ASSERT(0 && "Exceptions are not supported with the Tiny runtime");
#else
        if (fromType != NULL && toType != NULL)
        {
            message += "Unable to cast object of type '";
            message += fromType->name;
            message += "' to type '";
            message += toType->name;
            message += "'.";
        }
#endif

        return message;
    }

    std::string Exception::FormatStackTrace(const Il2CppException* ex)
    {
        if (ex->stack_trace)
            return il2cpp::utils::StringUtils::Utf16ToUtf8(il2cpp::utils::StringUtils::GetChars(ex->stack_trace));

        return "";
    }
} // utils
} // il2cpp
