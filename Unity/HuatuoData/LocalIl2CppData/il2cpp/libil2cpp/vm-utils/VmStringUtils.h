#pragma once

#include "il2cpp-config.h"
#include <string>

namespace il2cpp
{
namespace utils
{
    class LIBIL2CPP_CODEGEN_API VmStringUtils
    {
    public:
        static Il2CppChar Utf16ToLower(Il2CppChar c);
        static bool CaseSensitiveEquals(Il2CppString* left, const char* right);
        static bool CaseSensitiveEquals(const char* left, const char* right);
        static bool CaseInsensitiveEquals(Il2CppString* left, const char* right);
        static bool CaseInsensitiveEquals(const char* left, const char* right);

        struct CaseSensitiveComparer
        {
            bool operator()(const std::string& left, const std::string& right) const;
            bool operator()(const std::string& left, const char* right) const;
            bool operator()(const char* left, const std::string& right) const;
            bool operator()(const char* left, const char* right) const;
        };

        struct CaseInsensitiveComparer
        {
            bool operator()(const std::string& left, const std::string& right) const;
            bool operator()(const std::string& left, const char* right) const;
            bool operator()(const char* left, const std::string& right) const;
            bool operator()(const char* left, const char* right) const;
        };
    };
} // namespace utils
} // namespace il2cpp
