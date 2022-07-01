#include "il2cpp-config.h"
#include <memory>
#include "il2cpp-api.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "icalls/mscorlib/System/String.h"
#include "utils/StringUtils.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/String.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    void String::RedirectToCreateString()
    {
        NOT_SUPPORTED_IL2CPP(String::RedirectToCreateString, "All String constructors should be redirected to String.CreateString.");
    }

    Il2CppString * String::InternalAllocateStr(int length)
    {
        return il2cpp::vm::String::NewSize(length);
    }

    static bool
    string_icall_is_in_array(Il2CppArray *chars, int32_t arraylength, Il2CppChar chr)
    {
        Il2CppChar cmpchar;
        int32_t arrpos;

        for (arrpos = 0; arrpos != arraylength; arrpos++)
        {
            cmpchar = il2cpp_array_get(chars, Il2CppChar, arrpos);
            if (cmpchar == chr)
                return true;
        }

        return false;
    }

/* System.StringSplitOptions */
    typedef enum
    {
        STRINGSPLITOPTIONS_NONE = 0,
        STRINGSPLITOPTIONS_REMOVE_EMPTY_ENTRIES = 1
    } StringSplitOptions;

    Il2CppArray * String::InternalSplit(Il2CppString* me, Il2CppArray* separator, int count, int options)
    {
        static Il2CppClass *String_array;
        Il2CppString * tmpstr;
        Il2CppArray * retarr;
        Il2CppChar *src;
        int32_t arrsize, srcsize, splitsize;
        int32_t i, lastpos, arrpos;
        int32_t tmpstrsize;
        int32_t remempty;
        int32_t flag;
        Il2CppChar *tmpstrptr;

        remempty = options & STRINGSPLITOPTIONS_REMOVE_EMPTY_ENTRIES;
        src = il2cpp::utils::StringUtils::GetChars(me);
        srcsize = il2cpp::utils::StringUtils::GetLength(me);
        arrsize = il2cpp::vm::Array::GetLength(separator);

        if (!String_array)
        {
            Il2CppClass *klass = il2cpp::vm::Class::GetArrayClass(il2cpp_defaults.string_class, 1);
            //mono_memory_barrier ();
            String_array = klass;
        }

        splitsize = 1;
        /* Count the number of elements we will return. Note that this operation
         * guarantees that we will return exactly splitsize elements, and we will
         * have enough data to fill each. This allows us to skip some checks later on.
         */
        if (remempty == 0)
        {
            for (i = 0; i != srcsize && splitsize < count; i++)
            {
                if (string_icall_is_in_array(separator, arrsize, src[i]))
                    splitsize++;
            }
        }
        else if (count > 1)
        {
            /* Require pattern "Nondelim + Delim + Nondelim" to increment counter.
             * Lastpos != 0 means first nondelim found.
             * Flag = 0 means last char was delim.
             * Efficient, though perhaps confusing.
             */
            lastpos = 0;
            flag = 0;
            for (i = 0; i != srcsize && splitsize < count; i++)
            {
                if (string_icall_is_in_array(separator, arrsize, src[i]))
                {
                    flag = 0;
                }
                else if (flag == 0)
                {
                    if (lastpos == 1)
                        splitsize++;
                    flag = 1;
                    lastpos = 1;
                }
            }

            /* Nothing but separators */
            if (lastpos == 0)
            {
                retarr = il2cpp::vm::Array::NewSpecific(String_array, 0);
                return retarr;
            }
        }

        /* if no split chars found return the string */
        if (splitsize == 1)
        {
            if (remempty == 0 || count == 1)
            {
                /* Copy the whole string */
                retarr = il2cpp::vm::Array::NewSpecific(String_array, 1);
                il2cpp_array_setref(retarr, 0, me);
            }
            else
            {
                /* otherwise we have to filter out leading & trailing delims */

                /* find first non-delim char */
                for (; srcsize != 0; srcsize--, src++)
                {
                    if (!string_icall_is_in_array(separator, arrsize, src[0]))
                        break;
                }
                /* find last non-delim char */
                for (; srcsize != 0; srcsize--)
                {
                    if (!string_icall_is_in_array(separator, arrsize, src[srcsize - 1]))
                        break;
                }
                tmpstr = il2cpp::vm::String::NewSize(srcsize);
                tmpstrptr = il2cpp::utils::StringUtils::GetChars(tmpstr);

                memcpy(tmpstrptr, src, srcsize * sizeof(Il2CppChar));
                retarr = il2cpp::vm::Array::NewSpecific(String_array, 1);
                il2cpp_array_setref(retarr, 0, tmpstr);
            }
            return retarr;
        }

        lastpos = 0;
        arrpos = 0;

        retarr = il2cpp::vm::Array::NewSpecific(String_array, splitsize);

        for (i = 0; i != srcsize && arrpos != splitsize; i++)
        {
            if (string_icall_is_in_array(separator, arrsize, src[i]))
            {
                if (lastpos != i || remempty == 0)
                {
                    tmpstrsize = i - lastpos;
                    tmpstr = il2cpp::vm::String::NewSize(tmpstrsize);
                    tmpstrptr = il2cpp::utils::StringUtils::GetChars(tmpstr);

                    memcpy(tmpstrptr, src + lastpos, tmpstrsize * sizeof(Il2CppChar));
                    il2cpp_array_setref(retarr, arrpos, tmpstr);
                    arrpos++;

                    if (arrpos == splitsize - 1)
                    {
                        /* Shortcut the last array element */

                        lastpos = i + 1;
                        if (remempty != 0)
                        {
                            /* Search for non-delim starting char (guaranteed to find one) Note that loop
                             * condition is only there for safety. It will never actually terminate the loop. */
                            for (; lastpos != srcsize; lastpos++)
                            {
                                if (!string_icall_is_in_array(separator, arrsize, src[lastpos]))
                                    break;
                            }
                            if (count > splitsize)
                            {
                                /* Since we have fewer results than our limit, we must remove
                                 * trailing delimiters as well.
                                 */
                                for (; srcsize != lastpos + 1; srcsize--)
                                {
                                    if (!string_icall_is_in_array(separator, arrsize, src[srcsize - 1]))
                                        break;
                                }
                            }
                        }

                        tmpstrsize = srcsize - lastpos;
                        tmpstr = il2cpp::vm::String::NewSize(tmpstrsize);
                        tmpstrptr = il2cpp::utils::StringUtils::GetChars(tmpstr);

                        memcpy(tmpstrptr, src + lastpos, tmpstrsize * sizeof(Il2CppChar));
                        il2cpp_array_setref(retarr, arrpos, tmpstr);

                        /* Loop will ALWAYS end here. Test criteria in the FOR loop is technically unnecessary. */
                        break;
                    }
                }
                lastpos = i + 1;
            }
        }

        return retarr;
    }

    Il2CppString* String::InternalIntern(Il2CppString* str)
    {
        return il2cpp_string_intern(str);
    }

    Il2CppString* String::InternalIsInterned(Il2CppString* str)
    {
        return il2cpp_string_is_interned(str);
    }

    Il2CppString* String::FastAllocateString(int32_t length)
    {
        return vm::String::NewSize(length);
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
