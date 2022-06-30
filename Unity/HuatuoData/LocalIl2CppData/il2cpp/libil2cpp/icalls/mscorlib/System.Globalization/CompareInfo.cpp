#include "il2cpp-config.h"
#include <memory>
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "icalls/mscorlib/System.Globalization/CompareInfo.h"
#include "icalls/mscorlib/System.Globalization/CompareOptions.h"
#include "vm/String.h"
#include "vm/Exception.h"
#include "vm/Array.h"
#include "utils/StringUtils.h"
#include "vm-utils/VmStringUtils.h"
#include <cwctype>
#include <wctype.h>
#include <algorithm>

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
    static int string_invariant_compare_char(Il2CppChar c1, Il2CppChar c2, int options)
    {
        int result = 0;

        // Ordinal can not be mixed with other options, and must return the difference, not only -1, 0, 1.
        if (options & CompareOptions_Ordinal)
            return (int)(c1 - c2);

        if (options & CompareOptions_IgnoreCase)
        {
            result = towlower(c1) - towlower(c2);
        }
        else
        {
            /*
             * No options. Kana, symbol and spacing options don't
             * apply to the invariant culture.
             */

            /*
             * FIXME: here we must use the information from c1type and c2type
             * to find out the proper collation, even on the InvariantCulture, the
             * sorting is not done by computing the unicode values, but their
             * actual sort order.
             */
            result = (int)(c1 - c2);
        }

        return ((result < 0) ? -1 : (result > 0) ? 1 : 0);
    }

    int32_t CompareInfo::internal_compare_icall(Il2CppChar* str1, int32_t length1, Il2CppChar* str2, int32_t length2, int32_t options)
    {
        // Do a normal ascii string compare, as we only know the invariant locale if we dont have ICU.
        /* c translation of C# code from old string.cs.. :) */
        const int length = std::max(length1, length2);
        int charcmp;
        int pos = 0;

        for (pos = 0; pos != length; pos++)
        {
            if (pos >= length1 || pos >= length2)
                break;

            charcmp = string_invariant_compare_char(str1[pos], str2[pos],
                options);
            if (charcmp != 0)
                return (charcmp);
        }

        /* the lesser wins, so if we have looped until length we just
         * need to check the last char
         */
        if (pos == length)
            return (string_invariant_compare_char(str1[pos - 1], str2[pos - 1], options));

        /* Test if one of the strings has been compared to the end */
        if (pos >= length1)
        {
            if (pos >= length2)
                return (0);
            return (-1);
        }
        else if (pos >= length2)
            return (1);

        /* if not, check our last char only.. (can this happen?) */
        return (string_invariant_compare_char(str1[pos], str2[pos], options));
    }

    int32_t CompareInfo::internal_index_icall(Il2CppChar* source, int32_t sindex, int32_t count, Il2CppChar* value, int32_t value_length, bool first)
    {
        int pos, i;

        if (first)
        {
            count -= value_length;
            for (pos = sindex; pos <= sindex + count; pos++)
                for (i = 0; source[pos + i] == value[i];)
                    if (++i == value_length)
                        return (pos);

            return (-1);
        }
        else
        {
            for (pos = sindex - value_length + 1; pos > sindex - count; pos--)
                if (memcmp(source + pos, value, value_length * sizeof(Il2CppChar)) == 0)
                    return (pos);

            return (-1);
        }
    }
} /* namespace Globalization */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
