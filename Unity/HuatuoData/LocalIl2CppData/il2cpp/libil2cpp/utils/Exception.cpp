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
        // Exception.RestoreExceptionDispatchInfo() will clear stack_trace, so we need to ensure that we read it only once
        Il2CppString* stack_trace = ex->stack_trace;

        if (stack_trace)
            return il2cpp::utils::StringUtils::Utf16ToUtf8(il2cpp::utils::StringUtils::GetChars(stack_trace));

        return "";
    }

    std::string Exception::FormatBaselibErrorState(const Baselib_ErrorState& errorState)
    {
        const auto len = Baselib_ErrorState_Explain(&errorState, nullptr, 0, Baselib_ErrorState_ExplainVerbosity_ErrorType_SourceLocation_Explanation);
        std::string buffer(len, ' ');
        // std::string::data() is const only until C++17
        Baselib_ErrorState_Explain(&errorState, &buffer[0], len, Baselib_ErrorState_ExplainVerbosity_ErrorType_SourceLocation_Explanation);
        return buffer;
    }
} // utils
} // il2cpp
