#include "il2cpp-config.h"

#include "icalls/mscorlib/System.Globalization/CultureInfoInternals.h"
#include "icalls/mscorlib/System.Globalization/CultureInfoTables.h"
#include "icalls/mscorlib/System.Globalization/RegionInfo.h"
#include "gc/WriteBarrier.h"
#include "utils/StringUtils.h"
#include "vm/String.h"

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
    static bool ConstructRegion(Il2CppRegionInfo* regionInfo, const RegionInfoEntry* ri)
    {
        regionInfo->geo_id = ri->geo_id;
        IL2CPP_OBJECT_SETREF(regionInfo, iso2name, vm::String::New(idx2string(ri->iso2name)));
        IL2CPP_OBJECT_SETREF(regionInfo, iso3name, vm::String::New(idx2string(ri->iso3name)));
        IL2CPP_OBJECT_SETREF(regionInfo, win3name, vm::String::New(idx2string(ri->win3name)));
        IL2CPP_OBJECT_SETREF(regionInfo, english_name, vm::String::New(idx2string(ri->english_name)));
        IL2CPP_OBJECT_SETREF(regionInfo, native_name, vm::String::New(idx2string(ri->native_name)));
        IL2CPP_OBJECT_SETREF(regionInfo, currency_symbol, vm::String::New(idx2string(ri->currency_symbol)));
        IL2CPP_OBJECT_SETREF(regionInfo, iso_currency_symbol, vm::String::New(idx2string(ri->iso_currency_symbol)));
        IL2CPP_OBJECT_SETREF(regionInfo, currency_english_name, vm::String::New(idx2string(ri->currency_english_name)));
        IL2CPP_OBJECT_SETREF(regionInfo, currency_native_name, vm::String::New(idx2string(ri->currency_native_name)));

        return true;
    }

    static int RegionNameLocator(const void *a, const void *b)
    {
        const char* aa = (const char*)a;
        const RegionInfoNameEntry* bb = (const RegionInfoNameEntry*)b;

        return strcmp(aa, idx2string(bb->name));
    }

    bool RegionInfo::construct_internal_region_from_name(Il2CppRegionInfo* regionInfo, Il2CppString* name)
    {
        std::string n = utils::StringUtils::Utf16ToUtf8(utils::StringUtils::GetChars(name));
        RegionInfoNameEntry* nameEntry = (RegionInfoNameEntry*)bsearch(n.c_str(), region_name_entries, NUM_REGION_ENTRIES,
            sizeof(RegionInfoNameEntry), RegionNameLocator);

        if (nameEntry == NULL)
            return false;

        return ConstructRegion(regionInfo, &region_entries[nameEntry->region_entry_index]);
    }
} /* namespace Globalization */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
