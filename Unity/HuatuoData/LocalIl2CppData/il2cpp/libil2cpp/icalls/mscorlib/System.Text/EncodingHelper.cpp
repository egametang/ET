#include "il2cpp-config.h"
#include "EncodingHelper.h"
#include <string>
#include "os/Encoding.h"
#include <vm/String.h>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Text
{
    static const char* encodings[] =
    {
        (char*)1,
        "ascii", "us_ascii", "us", "ansi_x3.4_1968",
        "ansi_x3.4_1986", "cp367", "csascii", "ibm367",
        "iso_ir_6", "iso646_us", "iso_646.irv:1991",
        (char*)2,
        "utf_7", "csunicode11utf7", "unicode_1_1_utf_7",
        "unicode_2_0_utf_7", "x_unicode_1_1_utf_7",
        "x_unicode_2_0_utf_7",
        (char*)3,
        "utf_8", "unicode_1_1_utf_8", "unicode_2_0_utf_8",
        "x_unicode_1_1_utf_8", "x_unicode_2_0_utf_8",
        (char*)4,
        "utf_16", "UTF_16LE", "ucs_2", "unicode",
        "iso_10646_ucs2",
        (char*)5,
        "unicodefffe", "utf_16be",
        (char*)6,
        "iso_8859_1",
        (char*)0
    };

    Il2CppString* EncodingHelper::InternalCodePage(int32_t* resultCodePage)
    {
        const int32_t want_name = *resultCodePage;

        *resultCodePage = -1;

        const std::string charSet = os::Encoding::GetCharSet();

        std::string codepage(charSet);

        for (size_t i = 0, length = codepage.length(); i < length; ++i)
        {
            char& c = codepage[i];

            if (isalpha(c))
                c = tolower(c);

            if (c == '-')
                c = '_';
        }

        // handle some common aliases
        const char* p = encodings[0];
        int code = 0;

        for (size_t i = 0; p != 0;)
        {
            if ((size_t)p < 7)
            {
                code = (int)(size_t)p;
                p = encodings[++i];
                continue;
            }

            if (codepage == p)
            {
                *resultCodePage = code;
                break;
            }

            p = encodings[++i];
        }

        if (codepage.find("utf_8") != std::string::npos)
            *resultCodePage |= 0x10000000;

        if (want_name && *resultCodePage == -1)
            return il2cpp::vm::String::NewWrapper(charSet.c_str());
        else
            return NULL;
    }
} // namespace Text
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
