#pragma once
#include "il2cpp-config.h"
#include <string>
#include "StringView.h"

#define STRING_TO_STRINGVIEW(sv) il2cpp::utils::StringViewUtils::StringToStringView(sv)

namespace il2cpp
{
namespace utils
{
    class StringViewUtils
    {
    public:
        template<typename CharType, typename CharTraits, typename StringAlloc>
        static StringView<CharType> StringToStringView(const std::basic_string<CharType, CharTraits, StringAlloc>& str)
        {
            return StringView<CharType>(str.c_str(), str.length());
        }

        // This will prevent accidentally assigning temporary values (like function return values)
        // to a string view. While this protection will only be enabled on C++11 compiles, even those
        // are enough to catch the bug in our runtime
#if IL2CPP_HAS_DELETED_FUNCTIONS
        template<typename CharType, typename CharTraits, typename StringAlloc>
        static StringView<CharType> StringToStringView(const std::basic_string<CharType, CharTraits, StringAlloc>&& str)
        {
            IL2CPP_ASSERT(0 && "Cannot create stringview into R-value reference");
            return StringView<CharType>::Empty();
        }

#endif
    };
}
}
