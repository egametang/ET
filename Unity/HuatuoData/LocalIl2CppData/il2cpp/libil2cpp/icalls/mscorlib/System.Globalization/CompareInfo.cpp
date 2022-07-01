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
    void CompareInfo::free_internal_collator(mscorlib_System_Globalization_CompareInfo * thisPtr)
    {
        // This method does not need any implementation.
    }

    static int string_invariant_indexof(Il2CppString *source, int sindex, int count, Il2CppString *value, bool first)
    {
        int lencmpstr = il2cpp::utils::StringUtils::GetLength(value);
        Il2CppChar* src = il2cpp::utils::StringUtils::GetChars(source);
        Il2CppChar* cmpstr = il2cpp::utils::StringUtils::GetChars(value);

        if (first)
        {
            count -= lencmpstr;
            for (int pos = sindex; pos <= sindex + count; pos++)
            {
                for (int i = 0; src[pos + i] == cmpstr[i];)
                {
                    if (++i == lencmpstr)
                        return (pos);
                }
            }

            return (-1);
        }
        else
        {
            for (int pos = sindex - lencmpstr + 1; pos > sindex - count; pos--)
            {
                if (memcmp(src + pos, cmpstr, lencmpstr * sizeof(Il2CppChar)) == 0)
                    return (pos);
            }

            return (-1);
        }
    }

    int CompareInfo::internal_index(mscorlib_System_Globalization_CompareInfo *thisPtr, Il2CppString *source, int sindex, int count, Il2CppString *value, int options, bool first)
    {
        return (string_invariant_indexof(source, sindex, count, value, first));
    }

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

    static int string_invariant_compare(Il2CppString *str1, int off1, int len1, Il2CppString *str2, int off2, int len2, int options)
    {
        int length;
        if (len1 >= len2)
            length = len1;
        else
            length = len2;

        Il2CppChar* ustr1 = il2cpp::utils::StringUtils::GetChars(str1) + off1;
        Il2CppChar* ustr2 = il2cpp::utils::StringUtils::GetChars(str2) + off2;

        int pos = 0;
        for (pos = 0; pos != length; pos++)
        {
            if (pos >= len1 || pos >= len2)
                break;

            int charcmp = string_invariant_compare_char(ustr1[pos], ustr2[pos], options);
            if (charcmp != 0)
                return (charcmp);
        }

        // The lesser wins, so if we have looped until length we just need to check the last char.
        if (pos == length)
            return (string_invariant_compare_char(ustr1[pos - 1], ustr2[pos - 1], options));

        // Test if one of the strings has been compared to the end.
        if (pos >= len1)
        {
            if (pos >= len2)
                return (0);
            else
                return (-1);
        }
        else if (pos >= len2)
            return (1);

        // If not, check our last char only.. (can this happen?)
        return (string_invariant_compare_char(ustr1[pos], ustr2[pos], options));
    }

    int CompareInfo::internal_compare(mscorlib_System_Globalization_CompareInfo *thisPtr, Il2CppString *str1, int off1, int len1, Il2CppString *str2, int off2, int len2, int options)
    {
        //MONO_ARCH_SAVE_REGS;

        // Do a normal ascii string compare, as we only know the invariant locale if we dont have ICU.
        return (string_invariant_compare(str1, off1, len1, str2, off2, len2, options));
    }

    void CompareInfo::construct_compareinfo(mscorlib_System_Globalization_CompareInfo *, Il2CppString *)
    {
        // This method does not need any implementation.
    }

    static Il2CppArray* GetSortKeyCaseSensitive(Il2CppString* source)
    {
        const int32_t keyLength = sizeof(Il2CppChar) * source->length;
        Il2CppArray* keyBytes = vm::Array::New(il2cpp_defaults.byte_class, keyLength);
        memcpy(il2cpp_array_addr(keyBytes, uint8_t, 0), source->chars, keyLength);

        return keyBytes;
    }

    static Il2CppArray* GetSortKeyIgnoreCase(Il2CppString* source)
    {
        const int32_t keyLength = sizeof(Il2CppChar) * source->length;
        Il2CppArray* keyBytes = vm::Array::New(il2cpp_defaults.byte_class, keyLength);
        Il2CppChar* destination = reinterpret_cast<Il2CppChar*>(il2cpp_array_addr(keyBytes, uint8_t, 0));

        for (int i = 0; i < source->length; i++, destination++)
        {
            *destination = utils::VmStringUtils::Utf16ToLower(source->chars[i]);
        }

        return keyBytes;
    }

    void CompareInfo::assign_sortkey(void* /* System.Globalization.CompareInfo */ self, Il2CppSortKey* key, Il2CppString* source, CompareOptions options)
    {
        if ((options & CompareOptions_IgnoreCase) != 0 || (options & CompareOptions_OrdinalIgnoreCase) != 0)
        {
            key->key = GetSortKeyIgnoreCase(source);
        }
        else
        {
            key->key = GetSortKeyCaseSensitive(source);
        }
    }
} /* namespace Globalization */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
