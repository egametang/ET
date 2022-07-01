#include "il2cpp-config.h"
#include "CalendarData.h"
#include "il2cpp-object-internals.h"
#include "CultureInfoTables.h"
#include "il2cpp-api.h"
#include "il2cpp-class-internals.h"
#include "vm/Array.h"
#include "vm/String.h"
#include <string>
#include <cstdlib>
#include "utils/StringUtils.h"

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
    static Il2CppArray* calendar_data_create_names_array_idx(const uint16_t* names, int len, const char* strings_array)
    {
        if (names == NULL)
            return NULL;

        Il2CppArray* ret = il2cpp_array_new_specific(il2cpp_array_class_get(il2cpp_defaults.string_class, 1), len);

        for (int i = 0; i < len; i++)
        {
            if (names[i] == 0)
                il2cpp_array_setref(ret, i, vm::String::Empty());
            else
                il2cpp_array_setref(ret, i, il2cpp_string_new(strings_array + names[i]));
        }

        return ret;
    }

    static Il2CppArray* calendar_data_create_names_array_idx_dynamic(const uint16_t* names, int max, const char* strings_array)
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

        return calendar_data_create_names_array_idx(names, len, strings_array);
    }

    static int culture_data_culture_name_locator(const void *a, const void *b)
    {
        const char* aa = (const char*)a;
        const CultureInfoNameEntry* bb = (const CultureInfoNameEntry*)b;
        int ret;

        ret = strcmp(aa, idx2string(bb->name));

        return ret;
    }

    bool CalendarData::fill_calendar_data(Il2CppCalendarData* _this, Il2CppString* localeName, int32_t datetimeIndex)
    {
        const DateTimeFormatEntry *dfe;
        const CultureInfoEntry *ci;

        std::string cultureName = il2cpp::utils::StringUtils::Utf16ToUtf8(localeName->chars);

        const CultureInfoNameEntry* ne = (const CultureInfoNameEntry*)bsearch(cultureName.c_str(), culture_name_entries, NUM_CULTURE_ENTRIES, sizeof(CultureInfoNameEntry), culture_data_culture_name_locator);

        if (ne == NULL)
        {
            return false;
        }

        ci = &culture_entries[ne->culture_entry_index];
        dfe = &datetime_format_entries[ci->datetime_format_index];

        IL2CPP_OBJECT_SETREF(_this, NativeName, il2cpp_string_new(idx2string(ci->nativename)));
        Il2CppArray *short_date_patterns = calendar_data_create_names_array_idx_dynamic(dfe->short_date_patterns, NUM_SHORT_DATE_PATTERNS, &patterns[0]);
        IL2CPP_OBJECT_SETREF(_this, ShortDatePatterns, short_date_patterns);
        Il2CppArray *year_month_patterns = calendar_data_create_names_array_idx_dynamic(dfe->year_month_patterns, NUM_YEAR_MONTH_PATTERNS, &patterns[0]);
        IL2CPP_OBJECT_SETREF(_this, YearMonthPatterns, year_month_patterns);
        Il2CppArray *long_date_patterns = calendar_data_create_names_array_idx_dynamic(dfe->long_date_patterns, NUM_LONG_DATE_PATTERNS, &patterns[0]);
        IL2CPP_OBJECT_SETREF(_this, LongDatePatterns, long_date_patterns);
        IL2CPP_OBJECT_SETREF(_this, MonthDayPattern, il2cpp_string_new(pattern2string(dfe->month_day_pattern)));
        Il2CppArray *day_names = calendar_data_create_names_array_idx(dfe->day_names, NUM_DAYS, &datetime_strings[0]);
        IL2CPP_OBJECT_SETREF(_this, DayNames, day_names);
        Il2CppArray *abbr_day_names = calendar_data_create_names_array_idx(dfe->abbreviated_day_names, NUM_DAYS, &datetime_strings[0]);
        IL2CPP_OBJECT_SETREF(_this, AbbreviatedDayNames, abbr_day_names);
        Il2CppArray *ss_day_names = calendar_data_create_names_array_idx(dfe->shortest_day_names, NUM_DAYS, &datetime_strings[0]);
        IL2CPP_OBJECT_SETREF(_this, SuperShortDayNames, ss_day_names);
        Il2CppArray *month_names = calendar_data_create_names_array_idx(dfe->month_names, NUM_MONTHS, &datetime_strings[0]);
        IL2CPP_OBJECT_SETREF(_this, MonthNames, month_names);
        Il2CppArray *abbr_mon_names = calendar_data_create_names_array_idx(dfe->abbreviated_month_names, NUM_MONTHS, &datetime_strings[0]);
        IL2CPP_OBJECT_SETREF(_this, AbbreviatedMonthNames, abbr_mon_names);
        Il2CppArray *gen_month_names = calendar_data_create_names_array_idx(dfe->month_genitive_names, NUM_MONTHS, &datetime_strings[0]);
        IL2CPP_OBJECT_SETREF(_this, GenitiveMonthNames, gen_month_names);
        Il2CppArray *gen_abbr_mon_names = calendar_data_create_names_array_idx(dfe->abbreviated_month_genitive_names, NUM_MONTHS, &datetime_strings[0]);
        IL2CPP_OBJECT_SETREF(_this, GenitiveAbbreviatedMonthNames, gen_abbr_mon_names);

        return true;
    }
} // namespace Globalization
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
