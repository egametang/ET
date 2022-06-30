//This is a Generated File.... Run CultureInfoUpdater tool to update
/**
 * \file
 */

#ifndef _MONO_METADATA_CULTURE_INFO_H_
#define _MONO_METADATA_CULTURE_INFO_H_ 1




#define NUM_DAYS 7
#define NUM_MONTHS 13
#define GROUP_SIZE 2
#define NUM_CALENDARS 4

#define NUM_SHORT_DATE_PATTERNS 14
#define NUM_LONG_DATE_PATTERNS 10
#define NUM_SHORT_TIME_PATTERNS 12
#define NUM_LONG_TIME_PATTERNS 9
#define NUM_YEAR_MONTH_PATTERNS 8

#define idx2string(idx) (locale_strings + (idx))
#define pattern2string(idx) (patterns + (idx))
#define dtidx2string(idx) (datetime_strings + (idx))

/* need to change this if the string data ends up to not fit in a 64KB array. */


typedef struct {
	const uint16_t month_day_pattern;
	const uint16_t am_designator;
	const uint16_t pm_designator;

	const uint16_t day_names [NUM_DAYS]; 
	const uint16_t abbreviated_day_names [NUM_DAYS];
	const uint16_t shortest_day_names [NUM_DAYS];
	const uint16_t month_names [NUM_MONTHS];
	const uint16_t month_genitive_names [NUM_MONTHS];
	const uint16_t abbreviated_month_names [NUM_MONTHS];
	const uint16_t abbreviated_month_genitive_names [NUM_MONTHS];

	const int8_t calendar_week_rule;
	const int8_t first_day_of_week;

	const uint16_t date_separator;
	const uint16_t time_separator;	

	const uint16_t short_date_patterns [NUM_SHORT_DATE_PATTERNS];
	const uint16_t long_date_patterns [NUM_LONG_DATE_PATTERNS];
	const uint16_t short_time_patterns [NUM_SHORT_TIME_PATTERNS];
	const uint16_t long_time_patterns [NUM_LONG_TIME_PATTERNS];
	const uint16_t year_month_patterns [NUM_YEAR_MONTH_PATTERNS];
} DateTimeFormatEntry;

typedef struct {
	const uint16_t currency_decimal_separator;
	const uint16_t currency_group_separator;
	const uint16_t number_decimal_separator;
	const uint16_t number_group_separator;

	const uint16_t currency_symbol;
	const uint16_t percent_symbol;
	const uint16_t nan_symbol;
	const uint16_t per_mille_symbol;
	const uint16_t negative_infinity_symbol;
	const uint16_t positive_infinity_symbol;

	const uint16_t negative_sign;
	const uint16_t positive_sign;

	const int8_t currency_negative_pattern;
	const int8_t currency_positive_pattern;
	const int8_t percent_negative_pattern;
	const int8_t percent_positive_pattern;
	const int8_t number_negative_pattern;

	const int8_t currency_decimal_digits;
	const int8_t number_decimal_digits;

	const int currency_group_sizes [GROUP_SIZE];
	const int number_group_sizes [GROUP_SIZE];	
} NumberFormatEntry;

typedef struct {
	int ansi;
	int ebcdic;
	int mac;
	int oem;
	bool is_right_to_left;
	char list_sep;
} TextInfoEntry;

typedef struct {
	int16_t lcid;
	int16_t parent_lcid;
	int16_t calendar_type;
	int16_t region_entry_index;
	uint16_t name;
	uint16_t englishname;
	uint16_t nativename;
	uint16_t win3lang;
	uint16_t iso3lang;
	uint16_t iso2lang;
	uint16_t territory;
	uint16_t native_calendar_names [NUM_CALENDARS];

	int16_t datetime_format_index;
	int16_t number_format_index;
	
	TextInfoEntry text_info;
} CultureInfoEntry;

typedef struct {
	const uint16_t name;
	const int16_t culture_entry_index;
} CultureInfoNameEntry;

typedef struct {
	const int16_t geo_id;
	const uint16_t iso2name;
	const uint16_t iso3name;
	const uint16_t win3name;
	const uint16_t english_name;
	const uint16_t native_name;
	const uint16_t currency_symbol;
	const uint16_t iso_currency_symbol;
	const uint16_t currency_english_name;
	const uint16_t currency_native_name;
} RegionInfoEntry;

typedef struct {
	const uint16_t name;
	const int16_t region_entry_index;
} RegionInfoNameEntry;

#endif

