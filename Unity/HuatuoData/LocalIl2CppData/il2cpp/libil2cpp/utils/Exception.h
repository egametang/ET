#pragma once
#include "il2cpp-config.h"
#include <string>
struct Il2CppException;

#include "Baselib.h"
#include "C/Baselib_ErrorState.h"

namespace il2cpp
{
namespace utils
{
    class LIBIL2CPP_CODEGEN_API Exception
    {
    public:
        static std::string FormatException(const Il2CppException* ex);
        static std::string FormatStackTrace(const Il2CppException* ex);
        static std::string FormatInvalidCastException(const Il2CppClass* fromType, const Il2CppClass* toType);
        static std::string FormatBaselibErrorState(const Baselib_ErrorState& errorState);
    };
} // utils
} // utils
