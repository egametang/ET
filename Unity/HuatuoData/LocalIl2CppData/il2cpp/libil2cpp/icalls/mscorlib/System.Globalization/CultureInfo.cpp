#include "il2cpp-config.h"
#include <string>
#include <algorithm>
#include "il2cpp-api.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "gc/WriteBarrier.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"
#include "vm/Array.h"
#include "vm/Exception.h"
#include "vm/String.h"
#include "os/Locale.h"
#include "icalls/mscorlib/System.Globalization/CultureInfo.h"
#include "CultureInfoInternals.h"
#include "CultureInfoTables.h"


/*
* The following methods is modified from the ICU source code. (http://oss.software.ibm.com/icu)
* Copyright (c) 1995-2003 International Business Machines Corporation and others
* All rights reserved.
*/
static std::string get_current_locale_name(void)
{
    char* locale;
    char* corrected = NULL;
    const char* p;

    std::string locale_str = il2cpp::os::Locale::GetLocale();

    if (locale_str.empty())
        return std::string();

    locale = il2cpp::utils::StringUtils::StringDuplicate(locale_str.c_str());

    if ((p = strchr(locale, '.')) != NULL)
    {
        /* assume new locale can't be larger than old one? */
        corrected = (char*)IL2CPP_MALLOC(strlen(locale));
        strncpy(corrected, locale, p - locale);
        corrected[p - locale] = 0;

        /* do not copy after the @ */
        if ((p = strchr(corrected, '@')) != NULL)
            corrected[p - corrected] = 0;
    }

    /* Note that we scan the *uncorrected* ID. */
    if ((p = strrchr(locale, '@')) != NULL)
    {
        /*
        * Mono we doesn't handle the '@' modifier because it does
        * not have any cultures that use it. Just trim it
        * off of the end of the name.
        */

        if (corrected == NULL)
        {
            corrected = (char*)IL2CPP_MALLOC(strlen(locale));
            strncpy(corrected, locale, p - locale);
            corrected[p - locale] = 0;
        }
    }

    if (corrected == NULL)
        corrected = locale;
    else
        IL2CPP_FREE(locale);

    char* c;
    if ((c = strchr(corrected, '_')) != NULL)
        *c = '-';

    std::string result(corrected);
    IL2CPP_FREE(corrected);

    std::transform(result.begin(), result.end(), result.begin(), ::tolower);

    return result;
}

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
    static Il2CppArray* culture_info_create_names_array_idx(const uint16_t* names, int max)
    {
        if (names == NULL)
            return NULL;

        int len = 0;
        for (int i = 0; i < max; i++)
        {
            if (names[i] == 0)
                break;
            len++;
        }

        Il2CppArray* ret = il2cpp_array_new_specific(il2cpp_array_class_get(il2cpp_defaults.string_class, 1), len);

        for (int i = 0; i < len; i++)
            il2cpp_array_setref(ret, i, il2cpp_string_new(dtidx2string(names[i])));

        return ret;
    }

    static bool construct_culture(Il2CppCultureInfo* cultureInfo, const CultureInfoEntry *ci)
    {
        cultureInfo->lcid = ci->lcid;
        IL2CPP_OBJECT_SETREF(cultureInfo, name, il2cpp_string_new(idx2string(ci->name)));
        IL2CPP_OBJECT_SETREF(cultureInfo, englishname, il2cpp_string_new(idx2string(ci->englishname)));
        IL2CPP_OBJECT_SETREF(cultureInfo, nativename, il2cpp_string_new(idx2string(ci->nativename)));
        IL2CPP_OBJECT_SETREF(cultureInfo, win3lang, il2cpp_string_new(idx2string(ci->win3lang)));
        IL2CPP_OBJECT_SETREF(cultureInfo, iso3lang, il2cpp_string_new(idx2string(ci->iso3lang)));
        IL2CPP_OBJECT_SETREF(cultureInfo, iso2lang, il2cpp_string_new(idx2string(ci->iso2lang)));

        // It's null for neutral cultures
        if (ci->territory > 0)
            IL2CPP_OBJECT_SETREF(cultureInfo, territory, il2cpp_string_new(idx2string(ci->territory)));
        cultureInfo->parent_lcid = ci->parent_lcid;
        cultureInfo->datetime_index = ci->datetime_format_index;
        cultureInfo->number_index = ci->number_format_index;
        cultureInfo->text_info_data = &ci->text_info;

        IL2CPP_OBJECT_SETREF(cultureInfo, native_calendar_names, culture_info_create_names_array_idx(ci->native_calendar_names, NUM_CALENDARS));
        cultureInfo->default_calendar_type = ci->calendar_type;

        return true;
    }

    static int culture_info_culture_name_locator(const void *a, const void *b)
    {
        const char* aa = (const char*)a;
        const CultureInfoNameEntry* bb = (const CultureInfoNameEntry*)b;
        int ret;

        ret = strcmp(aa, idx2string(bb->name));

        return ret;
    }

    static int culture_lcid_locator(const void *a, const void *b)
    {
        const CultureInfoEntry *aa = (const CultureInfoEntry*)a;
        const CultureInfoEntry *bb = (const CultureInfoEntry*)b;

        return (aa->lcid - bb->lcid);
    }

    static const CultureInfoEntry* culture_info_entry_from_lcid(int lcid)
    {
        CultureInfoEntry key;
        key.lcid = lcid;
        return (const CultureInfoEntry*)bsearch(&key, culture_entries, NUM_CULTURE_ENTRIES, sizeof(CultureInfoEntry), culture_lcid_locator);
    }
    bool CultureInfo::construct_internal_locale_from_lcid(Il2CppCultureInfo* cultureInfo, int lcid)
    {
        const CultureInfoEntry* ci = culture_info_entry_from_lcid(lcid);
        if (ci == NULL)
            return false;

        return construct_culture(cultureInfo, ci);
    }

    bool CultureInfo::construct_internal_locale_from_name(Il2CppCultureInfo* cultureInfo, Il2CppString* name)
    {
        std::string cultureName = il2cpp::utils::StringUtils::Utf16ToUtf8(name->chars);
        const CultureInfoNameEntry* ne = (const CultureInfoNameEntry*)bsearch(cultureName.c_str(), culture_name_entries, NUM_CULTURE_ENTRIES, sizeof(CultureInfoNameEntry), culture_info_culture_name_locator);

        if (ne == NULL)
            return false;

        return construct_culture(cultureInfo, &culture_entries[ne->culture_entry_index]);
    }

    static bool IsMatchingCultureInfoEntry(const CultureInfoEntry& entry, bool neutral, bool specific, bool installed)
    {
        const bool isNeutral = entry.territory == 0;
        return ((neutral && isNeutral) || (specific && !isNeutral));
    }

    Il2CppArray* CultureInfo::internal_get_cultures(bool neutral, bool specific, bool installed)
    {
        // Count culture infos that match.
        int numMatchingCultures = 0;
        for (int i = 0; i < NUM_CULTURE_ENTRIES; ++i)
        {
            const CultureInfoEntry& entry = culture_entries[i];
            if (IsMatchingCultureInfoEntry(entry, neutral, specific, installed))
                ++numMatchingCultures;
        }

        if (neutral)
            ++numMatchingCultures;

        // Allocate result array.
        Il2CppClass* cultureInfoClass = il2cpp_defaults.culture_info;
        Il2CppArray* array = il2cpp_array_new(cultureInfoClass, numMatchingCultures);

        int index = 0;

        // InvariantCulture is not in culture table. We reserve the first
        // array element for it.
        if (neutral)
            il2cpp_array_setref(array, index++, NULL);

        // Populate CultureInfo entries.
        for (int i = 0; i < NUM_CULTURE_ENTRIES; ++i)
        {
            const CultureInfoEntry& entry = culture_entries[i];
            if (!IsMatchingCultureInfoEntry(entry, neutral, specific, installed))
                continue;

            Il2CppCultureInfo* info = reinterpret_cast<Il2CppCultureInfo*>(il2cpp_object_new(cultureInfoClass));
            construct_culture(info, &entry);

            il2cpp_array_setref(array, index++, info);
        }

        return array;
    }

    Il2CppString* CultureInfo::get_current_locale_name()
    {
        return vm::String::New(::get_current_locale_name().c_str());
    }

    extern "C" void STDCALL InitializeUserPreferredCultureInfoInAppX(CultureInfoChangedCallback onCultureInfoChangedInAppX)
    {
#if IL2CPP_TARGET_WINRT
        il2cpp_hresult_t hr = os::Locale::InitializeUserPreferredCultureInfoInAppX(onCultureInfoChangedInAppX);
        if (IL2CPP_HR_FAILED(hr))
            vm::Exception::Raise(hr, false);
#endif
    }

    extern "C" void STDCALL SetUserPreferredCultureInfoInAppX(const Il2CppChar* name)
    {
#if IL2CPP_TARGET_WINRT
        il2cpp_hresult_t hr = os::Locale::SetUserPreferredCultureInfoInAppX(name);
        if (IL2CPP_HR_FAILED(hr))
            vm::Exception::Raise(hr, false);
#endif
    }
} /* namespace Globalization */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
