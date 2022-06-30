#pragma once

namespace il2cpp
{
namespace os
{
    class CrashHelpers
    {
    public:
        NORETURN static void Crash();
    private:
        NORETURN static void CrashImpl();
    };
}
}
