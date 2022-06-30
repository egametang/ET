#include "il2cpp-config.h"
#include "../char-conversions.h"
#include "il2cpp-object-internals.h"
#include "utils/Functional.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"
#include "utils/utf8-cpp/source/utf8/unchecked.h"
#include <stdarg.h>

namespace il2cpp
{
namespace utils
{
    std::string StringUtils::Printf(const char* format, ...)
    {
        va_list argsToCheckSize;
        int n;
        std::string ret;

        va_start(argsToCheckSize, format);
#if IL2CPP_COMPILER_MSVC
        // MS vsnprintf always returns -1 if string doesn't fit, rather than
        // the needed size. Used their 'special' function instead to get required size
        n = _vscprintf_p(format, argsToCheckSize);
#else
        // use a temporary buffer as some docs indicate we cannot pass NULL to vsnprintf
        char buf[1];
        n = vsnprintf(buf, 0, format, argsToCheckSize);
#endif
        if (n == -1)
            return NULL;

        ret.resize(n + 1, 0);
        va_end(argsToCheckSize);

        va_list argsToFormat;

        va_start(argsToFormat, format);
        n = vsnprintf(&ret[0], ret.size(), format, argsToFormat);
        va_end(argsToFormat);

        IL2CPP_ASSERT(n < (int)ret.size());

        if (n == -1)
            return NULL;

        // The v*printf methods might put a trailing NUL character, which should not not be in a
        // std::string, so strip it out.
        if (!ret.empty() && ret[ret.size() - 1] == '\0')
            ret = ret.substr(0, ret.size() - 1);

        return ret;
    }

    std::string StringUtils::NPrintf(const char* format, size_t max_n, ...)
    {
        va_list argsToCheckSize;
        size_t n;
        std::string ret;

        va_start(argsToCheckSize, max_n);
#if IL2CPP_COMPILER_MSVC
        // MS vsnprintf always returns -1 if string doesn't fit, rather than
        // the needed size. Used their 'special' function instead to get required size
        n = _vscprintf_p(format, argsToCheckSize);
#else
        // use a temporary buffer as some docs indicate we cannot pass NULL to vsnprintf
        char buf[1];
        n = vsnprintf(buf, 0, format, argsToCheckSize);
#endif
        if (n == -1)
            return NULL;

        n = (max_n < ++n) ? max_n : n;

        ret.resize(n, 0);
        va_end(argsToCheckSize);

        va_list argsToFormat;

        va_start(argsToFormat, max_n);
        n = vsnprintf(&ret[0], n, format, argsToFormat);
        va_end(argsToFormat);

        IL2CPP_ASSERT(n < ret.size());

        if (n == -1)
            return NULL;

        // The v*printf methods might put a trailing NUL character, which should not not be in a
        // std::string, so strip it out.
        if (!ret.empty() && ret[ret.size() - 1] == '\0')
            ret = ret.substr(0, ret.size() - 1);

        return ret;
    }

    std::string StringUtils::Utf16ToUtf8(const Il2CppChar* utf16String)
    {
        return Utf16ToUtf8(utf16String, -1);
    }

    std::string StringUtils::Utf16ToUtf8(const Il2CppChar* utf16String, int maximumSize)
    {
        const Il2CppChar* ptr = utf16String;
        size_t length = 0;
        while (*ptr)
        {
            ptr++;
            length++;
            if (maximumSize != -1 && length == maximumSize)
                break;
        }

        std::string utf8String;
        utf8String.reserve(length);
        utf8::unchecked::utf16to8(utf16String, ptr, std::back_inserter(utf8String));

        return utf8String;
    }

    std::string StringUtils::Utf16ToUtf8(const UTF16String& utf16String)
    {
        return Utf16ToUtf8(utf16String.c_str(), static_cast<int>(utf16String.length()));
    }

    UTF16String StringUtils::Utf8ToUtf16(const char* utf8String)
    {
        return Utf8ToUtf16(utf8String, strlen(utf8String));
    }

    UTF16String StringUtils::Utf8ToUtf16(const char* utf8String, size_t length)
    {
        UTF16String utf16String;

        if (utf8::is_valid(utf8String, utf8String + length))
        {
            utf16String.reserve(length);
            utf8::unchecked::utf8to16(utf8String, utf8String + length, std::back_inserter(utf16String));
        }

        return utf16String;
    }

    UTF16String StringUtils::Utf8ToUtf16(const std::string& utf8String)
    {
        return Utf8ToUtf16(utf8String.c_str(), utf8String.length());
    }

    char* StringUtils::StringDuplicate(const char *strSource)
    {
        char* result = NULL;

        if (!strSource)
            return NULL;

        size_t length = strlen(strSource) + 1;

        if ((result = (char*)IL2CPP_MALLOC(length)))
#if IL2CPP_COMPILER_MSVC
            strcpy_s(result, length, strSource);
#elif IL2CPP_TARGET_LINUX
            strncpy(result, strSource, length);
#else
            strlcpy(result, strSource, length);
#endif

        return result;
    }

    Il2CppChar* StringUtils::StringDuplicate(const Il2CppChar* strSource, size_t length)
    {
        size_t byteLengthWithNullTerminator = sizeof(Il2CppChar) * (length + 1);
        Il2CppChar* utf16name = (Il2CppChar*)IL2CPP_MALLOC(byteLengthWithNullTerminator);
        memcpy(utf16name, strSource, byteLengthWithNullTerminator);

        return utf16name;
    }

    bool StringUtils::EndsWith(const std::string& string, const std::string& suffix)
    {
        const size_t stringLength = string.length();
        const size_t suffixLength = suffix.length();

        if (suffixLength > stringLength)
            return false;

        return string.rfind(suffix.c_str(), stringLength - suffixLength, suffixLength) != std::string::npos;
    }

    Il2CppChar* StringUtils::GetChars(Il2CppString* str)
    {
        return str->chars;
    }

    int32_t StringUtils::GetLength(Il2CppString* str)
    {
        return str->length;
    }
} /* utils */
} /* il2cpp */
