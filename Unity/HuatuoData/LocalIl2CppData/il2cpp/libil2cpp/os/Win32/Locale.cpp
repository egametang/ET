#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "os/Locale.h"
#include "WindowsHelpers.h"
#include <locale.h>
#include <vector>

namespace il2cpp
{
namespace os
{
#if IL2CPP_TARGET_WINDOWS_DESKTOP

    std::string Locale::GetLocale()
    {
        LCID lcid = GetThreadLocale();

        int number_of_characters = GetLocaleInfo(lcid, LOCALE_SNAME, NULL, 0);
        std::vector<WCHAR> locale_name(number_of_characters);
        if (GetLocaleInfo(lcid, LOCALE_SNAME, &locale_name[0], number_of_characters) == 0)
            return std::string();

        std::vector<char> locale_name_char(number_of_characters);
        if (WideCharToMultiByte(CP_ACP, 0, &locale_name[0], number_of_characters, &locale_name_char[0], number_of_characters, NULL, NULL) == 0)
            return std::string();

        return std::string(locale_name_char.begin(), locale_name_char.end());
    }

#endif

#if IL2CPP_SUPPORT_LOCALE_INDEPENDENT_PARSING
    static _locale_t s_cLocale = NULL;
#endif

    void Locale::Initialize()
    {
#if IL2CPP_SUPPORT_LOCALE_INDEPENDENT_PARSING
        s_cLocale = _create_locale(LC_ALL, "C");
#endif
    }

    void Locale::UnInitialize()
    {
#if IL2CPP_SUPPORT_LOCALE_INDEPENDENT_PARSING
        _free_locale(s_cLocale);
        s_cLocale = NULL;
#endif
    }

#if IL2CPP_SUPPORT_LOCALE_INDEPENDENT_PARSING
    double Locale::DoubleParseLocaleIndependentImpl(char *ptr, char** endptr)
    {
        return _strtod_l(ptr, endptr, s_cLocale);
    }

#endif
} /* namespace os */
} /* namespace il2cpp */

#endif
