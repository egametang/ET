#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "CompareOptions.h"

struct Il2CppString;
struct Il2CppObject;
struct Il2CppSortKey;
struct mscorlib_System_Globalization_CompareInfo;

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
    class LIBIL2CPP_CODEGEN_API CompareInfo
    {
    public:
        static void assign_sortkey(void* /* System.Globalization.CompareInfo */ self, Il2CppSortKey* key, Il2CppString* source, CompareOptions options);
        static void free_internal_collator(mscorlib_System_Globalization_CompareInfo * thisPtr);
        static int internal_compare(mscorlib_System_Globalization_CompareInfo *, Il2CppString *, int, int, Il2CppString *, int, int, int);
        static int internal_index(mscorlib_System_Globalization_CompareInfo *thisPtr, Il2CppString *source, int sindex, int count, Il2CppString *value, int options, bool first);
        static void construct_compareinfo(mscorlib_System_Globalization_CompareInfo *, Il2CppString *);
    };
} /* namespace Globalization */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
