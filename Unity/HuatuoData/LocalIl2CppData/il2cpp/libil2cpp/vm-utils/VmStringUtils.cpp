#include "il2cpp-config.h"

#if !RUNTIME_TINY

#include "VmStringUtils.h"
#include "il2cpp-object-internals.h"
#include "../char-conversions.h"
#include "utils/Functional.h"
#include "utils/StringUtils.h"
#include "utils/utf8-cpp/source/utf8/unchecked.h"

namespace il2cpp
{
namespace utils
{
    Il2CppChar VmStringUtils::Utf16ToLower(Il2CppChar c)
    {
        const Il2CppChar kDataLowThreshold = 9423;
        const Il2CppChar kDataHighThreshold = 65313;

        if (c <= kDataLowThreshold)
        {
            c = ToLowerDataLow[c];
        }
        else if (c >= kDataHighThreshold)
        {
            c = ToLowerDataHigh[c - kDataHighThreshold];
        }

        return c;
    }

    bool VmStringUtils::CaseSensitiveComparer::operator()(const std::string& left, const std::string& right) const
    {
        return left == right;
    }

    bool VmStringUtils::CaseSensitiveComparer::operator()(const std::string& left, const char* right) const
    {
        return left.compare(right) == 0;
    }

    bool VmStringUtils::CaseSensitiveComparer::operator()(const char* left, const std::string& right) const
    {
        return right.compare(left) == 0;
    }

    bool VmStringUtils::CaseSensitiveComparer::operator()(const char* left, const char* right) const
    {
        return strcmp(left, right) == 0;
    }

    static inline void Utf32CharToSurrogatePair(uint32_t c, Il2CppChar(&surrogatePair)[2])
    {
        const Il2CppChar kLeadOffset = 55232;
        const Il2CppChar kTrailSurrogateMin = 56320;

        if (c > 0xffff)
        {
            surrogatePair[0] = static_cast<Il2CppChar>((c >> 10) + kLeadOffset);
            surrogatePair[1] = static_cast<Il2CppChar>((c & 0x3ff) + kTrailSurrogateMin);
        }
        else
        {
            surrogatePair[0] = static_cast<Il2CppChar>(c);
            surrogatePair[1] = 0;
        }
    }

    static inline bool Utf16CharEqualsIgnoreCase(Il2CppChar left, Il2CppChar right)
    {
        return VmStringUtils::Utf16ToLower(left) == VmStringUtils::Utf16ToLower(right);
    }

    bool VmStringUtils::CaseInsensitiveComparer::operator()(const std::string& left, const std::string& right) const
    {
        return operator()(left.c_str(), right.c_str());
    }

    bool VmStringUtils::CaseInsensitiveComparer::operator()(const std::string& left, const char* right) const
    {
        return operator()(left.c_str(), right);
    }

    bool VmStringUtils::CaseInsensitiveComparer::operator()(const char* left, const std::string& right) const
    {
        return operator()(left, right.c_str());
    }

    bool VmStringUtils::CaseInsensitiveComparer::operator()(const char* left, const char* right) const
    {
#if IL2CPP_DEBUG    // Invalid UTF8 strings shouldn't be passed here, so let's assert in debug mode
        IL2CPP_ASSERT(utf8::is_valid(left, left + strlen(left)));
        IL2CPP_ASSERT(utf8::is_valid(right, right + strlen(right)));
#endif

        Il2CppChar utf16Left[2];
        Il2CppChar utf16Right[2];

        while (*left && *right)
        {
            Utf32CharToSurrogatePair(utf8::unchecked::next(left), utf16Left);
            Utf32CharToSurrogatePair(utf8::unchecked::next(right), utf16Right);

            if (!Utf16CharEqualsIgnoreCase(utf16Left[0], utf16Right[0]) ||
                !Utf16CharEqualsIgnoreCase(utf16Left[1], utf16Right[1]))
            {
                return false;
            }
        }

        return *left == '\0' && *right == '\0';
    }

    bool VmStringUtils::CaseSensitiveEquals(Il2CppString* left, const char* right)
    {
        std::string leftString = StringUtils::Utf16ToUtf8(left->chars);
        functional::Filter<const char*, CaseSensitiveComparer> equalsLeft(leftString.c_str());
        return equalsLeft(right);
    }

    bool VmStringUtils::CaseSensitiveEquals(const char* left, const char* right)
    {
        functional::Filter<const char*, CaseSensitiveComparer> equalsLeft(left);
        return equalsLeft(right);
    }

    bool VmStringUtils::CaseInsensitiveEquals(Il2CppString* left, const char* right)
    {
        std::string leftString = StringUtils::Utf16ToUtf8(left->chars);
        functional::Filter<const char*, CaseInsensitiveComparer> equalsLeft(leftString.c_str());
        return equalsLeft(right);
    }

    bool VmStringUtils::CaseInsensitiveEquals(const char* left, const char* right)
    {
        functional::Filter<const char *, CaseInsensitiveComparer> equalsLeft(left);
        return equalsLeft(right);
    }
}
}

#endif
