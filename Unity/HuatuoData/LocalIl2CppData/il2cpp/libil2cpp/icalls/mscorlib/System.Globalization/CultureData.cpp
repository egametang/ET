#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "CultureData.h"
#include "il2cpp-api.h"
#include <icalls/mscorlib/System.Globalization/CultureInfoInternals.h>
#include <icalls/mscorlib/System.Globalization/CultureInfoTables.h>
#include "gc/WriteBarrier.h"
#include <vm/Array.h>

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
    static Il2CppArray* culture_data_create_names_array_idx(const uint16_t* names, int max, const char* strings_array)
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
            il2cpp_array_setref(ret, i, il2cpp_string_new(strings_array + names[i]));

        return ret;
    }

    static Il2CppArray* culture_data_create_group_sizes_array(const int *gs, int ml)
    {
        int i, len = 0;

        for (i = 0; i < ml; i++)
        {
            if (gs[i] == -1)
                break;
            len++;
        }

        Il2CppArray* ret = il2cpp_array_new_specific(il2cpp_array_class_get(il2cpp_defaults.int32_class, 1), len);

        for (i = 0; i < len; i++)
            il2cpp_array_set(ret, int32_t, i, gs[i]);

        return ret;
    }

    const void* CultureData::fill_number_data(int32_t number_index, NumberFormatEntryManaged* managed)
    {
        IL2CPP_ASSERT(number_index >= 0 && number_index < IL2CPP_ARRAY_SIZE(number_format_entries));

        NumberFormatEntry const * const native = &number_format_entries[number_index];

        managed->currency_decimal_digits = native->currency_decimal_digits;
        managed->currency_decimal_separator = native->currency_decimal_separator;
        managed->currency_group_separator = native->currency_group_separator;
        managed->currency_group_sizes0 = native->currency_group_sizes[0];
        managed->currency_group_sizes1 = native->currency_group_sizes[1];
        managed->currency_negative_pattern = native->currency_negative_pattern;
        managed->currency_positive_pattern = native->currency_positive_pattern;
        managed->currency_symbol = native->currency_symbol;
        managed->nan_symbol = native->nan_symbol;
        managed->negative_infinity_symbol = native->negative_infinity_symbol;
        managed->negative_sign = native->negative_sign;
        managed->number_decimal_digits = native->number_decimal_digits;
        managed->number_decimal_separator = native->number_decimal_separator;
        managed->number_group_separator = native->number_group_separator;
        managed->number_group_sizes0 = native->number_group_sizes[0];
        managed->number_group_sizes1  = native->number_group_sizes[1];
        managed->number_negative_pattern = native->number_negative_pattern;
        managed->per_mille_symbol = native->per_mille_symbol;
        managed->percent_negative_pattern = native->percent_negative_pattern;
        managed->percent_positive_pattern = native->percent_positive_pattern;
        managed->percent_symbol = native->percent_symbol;
        managed->positive_infinity_symbol = native->positive_infinity_symbol;
        managed->positive_sign = native->positive_sign;

        return locale_strings;
    }

    void CultureData::fill_culture_data(Il2CppCultureData* _this, int32_t datetimeIndex)
    {
        const DateTimeFormatEntry *dfe;

        IL2CPP_ASSERT(datetimeIndex >= 0);

        dfe = &datetime_format_entries[datetimeIndex];
        IL2CPP_OBJECT_SETREF(_this, AMDesignator, il2cpp_string_new(idx2string(dfe->am_designator)));
        IL2CPP_OBJECT_SETREF(_this, PMDesignator, il2cpp_string_new(idx2string(dfe->pm_designator)));
        IL2CPP_OBJECT_SETREF(_this, TimeSeparator, il2cpp_string_new(idx2string(dfe->time_separator)));
        Il2CppArray *long_time_patterns = culture_data_create_names_array_idx(dfe->long_time_patterns, NUM_LONG_TIME_PATTERNS, &patterns[0]);
        IL2CPP_OBJECT_SETREF(_this, LongTimePatterns, long_time_patterns);
        Il2CppArray *short_time_patterns = culture_data_create_names_array_idx(dfe->short_time_patterns, NUM_SHORT_TIME_PATTERNS, &patterns[0]);
        IL2CPP_OBJECT_SETREF(_this, ShortTimePatterns, short_time_patterns);
        _this->FirstDayOfWeek = dfe->first_day_of_week;
        _this->CalendarWeekRule = dfe->calendar_week_rule;
    }
} // namespace Globalization
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
