#pragma once

#include "il2cpp-config.h"
#include "utils/StringView.h"

namespace il2cpp
{
namespace os
{
    class Debug
    {
    public:
        static bool IsDebuggerPresent();
        static void WriteString(const utils::StringView<Il2CppNativeChar>& message);
    };
}
}
