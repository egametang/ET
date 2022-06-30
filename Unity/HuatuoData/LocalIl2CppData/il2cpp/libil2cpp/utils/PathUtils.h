#pragma once

#include "il2cpp-config.h"
#include <string>
#include "StringViewUtils.h"

namespace il2cpp
{
namespace utils
{
namespace PathUtils
{
    std::string BasenameNoExtension(const std::string& path);
    std::string PathNoExtension(const std::string& path);

    template<typename CharType>
    std::basic_string<CharType> Basename(const utils::StringView<CharType>& path)
    {
        if (path.IsEmpty())
            return std::basic_string<CharType>(1, static_cast<CharType>('.'));

        const size_t pos = path.RFind(IL2CPP_DIR_SEPARATOR);

        // No seperators. Path is filename
        if (pos == utils::StringView<CharType>::NPos())
            return std::basic_string<CharType>(path.Str(), path.Length());

        return std::basic_string<CharType>(path.Str() + pos + 1, path.Length() - pos - 1);
    }

    template<typename CharType>
    std::basic_string<CharType> Basename(const std::basic_string<CharType>& path)
    {
        return Basename(STRING_TO_STRINGVIEW(path));
    }

    template<typename CharType>
    std::basic_string<CharType> DirectoryName(const utils::StringView<CharType>& path)
    {
        if (path.IsEmpty())
            return std::basic_string<CharType>();

        const size_t pos = path.RFind(IL2CPP_DIR_SEPARATOR);

        if (pos == utils::StringView<CharType>::NPos())
            return std::basic_string<CharType>(1, static_cast<CharType>('.'));

        if (pos == 0)
            return std::basic_string<CharType>(1, static_cast<CharType>('/'));

        return std::basic_string<CharType>(path.Str(), pos);
    }

    template<typename CharType>
    std::basic_string<CharType> Combine(const utils::StringView<CharType>& path1, const utils::StringView<CharType>& path2)
    {
        std::basic_string<CharType> result;
        result.reserve(path1.Length() + path2.Length() + 1);
        result.append(path1.Str(), path1.Length());
        result.append(1, static_cast<CharType>(IL2CPP_DIR_SEPARATOR));
        result.append(path2.Str(), path2.Length());
        return result;
    }

    template<typename CharType>
    std::basic_string<CharType> DirectoryName(const std::basic_string<CharType>& path)
    {
        return DirectoryName(STRING_TO_STRINGVIEW(path));
    }

    template<typename CharType>
    std::basic_string<CharType> Combine(const std::basic_string<CharType>& path1, const std::basic_string<CharType>& path2)
    {
        return Combine(STRING_TO_STRINGVIEW(path1), STRING_TO_STRINGVIEW(path2));
    }

    template<typename CharType>
    std::basic_string<CharType> Combine(const std::basic_string<CharType>& path1, const utils::StringView<CharType>& path2)
    {
        return Combine(STRING_TO_STRINGVIEW(path1), path2);
    }

    template<typename CharType>
    std::basic_string<CharType> Combine(const utils::StringView<CharType>& path1, const std::basic_string<CharType>& path2)
    {
        return Combine(path1, STRING_TO_STRINGVIEW(path2));
    }
}
} /* utils */
} /* il2cpp */
