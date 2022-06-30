/*
    Directory utility functions that are common to all posix and posix-like platforms.
*/

#include "il2cpp-config.h"

#include "StringUtils.h"
#include "DirectoryUtils.h"

namespace il2cpp
{
namespace utils
{
    bool Match(const std::string name, size_t nameIndex, const std::string& pattern, const size_t patternIndex)
    {
        const size_t nameLength = name.length();

        for (size_t i = patternIndex, patternLength = pattern.length(); i < patternLength; ++i)
        {
            const char c = pattern[i];

            if (c == '*')
            {
                if (i + 1 == patternLength) // Star is last character, match everything.
                    return true;

                do
                {
                    // Check that we match the rest of the pattern against name.
                    if (Match(name, nameIndex, pattern, i + 1))
                        return true;
                }
                while (nameIndex++ < nameLength);

                return false;
            }
            else if (c == '?')
            {
                if (nameIndex == nameLength)
                    return false;

                nameIndex++;
            }
            else
            {
                if (nameIndex == nameLength)
                {
                    // A pattern ending with .* should match a file with no extension
                    // The pattern "file.*" should match "file"
                    if (c == '.' && i + 2 == patternLength && pattern[i + 1] == '*')
                        return true;
                    return false;
                }
                else if (name[nameIndex] != c)
                {
                    return false;
                }

                nameIndex++;
            }
        }

        // All characters matched
        return nameIndex == nameLength;
    }

    bool Match(const std::string name, const std::string& pattern)
    {
        return Match(name, 0, pattern, 0);
    }

    std::string CollapseAdjacentStars(const std::string& pattern)
    {
        std::string matchPattern;
        matchPattern.reserve(pattern.length());

        // Collapse adjacent stars into one
        for (size_t i = 0, length = pattern.length(); i < length; ++i)
        {
            if (i > 0 && pattern[i] == '*' && pattern[i - 1] == '*')
                continue;

            matchPattern.append(1, pattern[i]);
        }

        return matchPattern;
    }
}
}
