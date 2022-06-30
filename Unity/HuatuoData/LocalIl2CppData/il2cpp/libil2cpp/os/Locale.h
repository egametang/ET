#pragma once

#include <stdint.h>
#include <string>

namespace il2cpp
{
namespace os
{
    class Locale
    {
    public:
        static void Initialize();
        static void UnInitialize();

        static std::string GetLocale();
#if IL2CPP_SUPPORT_LOCALE_INDEPENDENT_PARSING
        static double DoubleParseLocaleIndependentImpl(char *ptr, char** endptr);
#endif

#if IL2CPP_TARGET_WINRT
        static il2cpp_hresult_t InitializeUserPreferredCultureInfoInAppX(CultureInfoChangedCallback onCultureInfoChangedInAppX);
        static il2cpp_hresult_t SetUserPreferredCultureInfoInAppX(const Il2CppChar* name);
        static void UnInitializeWinRT();
#endif
    };
} /* namespace os */
} /* namespace il2cpp */
