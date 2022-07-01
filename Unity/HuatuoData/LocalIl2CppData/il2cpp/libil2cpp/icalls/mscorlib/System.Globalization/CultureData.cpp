#include "il2cpp-config.h"
#include "CultureData.h"
#include "CultureInfoInternals.h"
#include "CultureInfoTables.h"
#include "il2cpp-api.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "vm/Array.h"

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

    void CultureData::fill_number_data(Il2CppNumberFormatInfo* number, int32_t numberIndex)
    {
        const NumberFormatEntry *nfe;

        IL2CPP_ASSERT(numberIndex >= 0);

        nfe = &number_format_entries[numberIndex];

        number->currencyDecimalDigits = nfe->currency_decimal_digits;
        IL2CPP_OBJECT_SETREF(number, currencyDecimalSeparator, il2cpp_string_new(idx2string(nfe->currency_decimal_separator)));
        IL2CPP_OBJECT_SETREF(number, currencyGroupSeparator, il2cpp_string_new(idx2string(nfe->currency_group_separator)));
        Il2CppArray *currency_sizes_arr = culture_data_create_group_sizes_array(nfe->currency_group_sizes, GROUP_SIZE);
        IL2CPP_OBJECT_SETREF(number, currencyGroupSizes, currency_sizes_arr);
        number->currencyNegativePattern = nfe->currency_negative_pattern;
        number->currencyPositivePattern = nfe->currency_positive_pattern;
        IL2CPP_OBJECT_SETREF(number, currencySymbol, il2cpp_string_new(idx2string(nfe->currency_symbol)));
        IL2CPP_OBJECT_SETREF(number, naNSymbol, il2cpp_string_new(idx2string(nfe->nan_symbol)));
        IL2CPP_OBJECT_SETREF(number, negativeInfinitySymbol, il2cpp_string_new(idx2string(nfe->negative_infinity_symbol)));
        IL2CPP_OBJECT_SETREF(number, negativeSign, il2cpp_string_new(idx2string(nfe->negative_sign)));
        number->numberDecimalDigits = nfe->number_decimal_digits;
        IL2CPP_OBJECT_SETREF(number, numberDecimalSeparator, il2cpp_string_new(idx2string(nfe->number_decimal_separator)));
        IL2CPP_OBJECT_SETREF(number, numberGroupSeparator, il2cpp_string_new(idx2string(nfe->number_group_separator)));
        Il2CppArray *number_sizes_arr = culture_data_create_group_sizes_array(nfe->number_group_sizes, GROUP_SIZE);
        IL2CPP_OBJECT_SETREF(number, numberGroupSizes, number_sizes_arr);
        number->numberNegativePattern = nfe->number_negative_pattern;
        number->percentNegativePattern = nfe->percent_negative_pattern;
        number->percentPositivePattern = nfe->percent_positive_pattern;
        IL2CPP_OBJECT_SETREF(number, percentSymbol, il2cpp_string_new(idx2string(nfe->percent_symbol)));
        IL2CPP_OBJECT_SETREF(number, perMilleSymbol, il2cpp_string_new(idx2string(nfe->per_mille_symbol)));
        IL2CPP_OBJECT_SETREF(number, positiveInfinitySymbol, il2cpp_string_new(idx2string(nfe->positive_infinity_symbol)));
        IL2CPP_OBJECT_SETREF(number, positiveSign, il2cpp_string_new(idx2string(nfe->positive_sign)));
    }
} // namespace Globalization
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
