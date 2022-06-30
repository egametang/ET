#pragma once
#include "il2cpp-config.h"
#include <limits.h>
#if IL2CPP_TARGET_WINDOWS
#include <malloc.h>
#else
#include <alloca.h>
#endif

#if IL2CPP_TARGET_LINUX
#define GCC_VERSION (__GNUC__ * 10000 \
                   + __GNUC_MINOR__ * 100 \
                   + __GNU_PATCHLEVEL__)
#endif

namespace il2cpp
{
namespace utils
{
    template<typename CharType>
    class StringView
    {
    private:
        const CharType* m_String;
        size_t m_Length;

        // Intended to only be used by Empty()
        inline StringView() :
            m_String(NULL),
            m_Length(0)
        {
        }

    public:
        template<size_t Length>
        inline StringView(const CharType(&str)[Length]) :
            m_String(str), m_Length(Length - 1)
        {
        }

        inline StringView(const CharType* str, size_t length) :
            m_String(str), m_Length(length)
        {
            IL2CPP_ASSERT(str != NULL);
        }

        inline StringView(const CharType* str, size_t startIndex, size_t length) :
            m_String(str + startIndex), m_Length(length)
        {
            IL2CPP_ASSERT(str != NULL);
        }

        inline StringView(const StringView<CharType>& str, size_t startIndex, size_t length) :
            m_String(str.Str() + startIndex),
            m_Length(length)
        {
            IL2CPP_ASSERT(startIndex + length <= str.Length());
        }

// This is to work around a bug in gcc (24666) where arrays decay to pointers too fast
// This is known to be fixed by at least 7.3.0
#if IL2CPP_TARGET_LINUX && GCC_VERSION < 70300
        inline StringView(const char* str) :
            m_String(str), m_Length(strlen(str))
        {
        }

#endif

        inline const CharType* Str() const
        {
            return m_String;
        }

        inline size_t Length() const
        {
            return m_Length;
        }

        inline CharType operator[](size_t index) const
        {
            return m_String[index];
        }

        inline bool IsNullTerminated() const
        {
            return m_String[m_Length] == 0;
        }

        inline bool IsEmpty() const
        {
            return Length() == 0;
        }

        static inline StringView<CharType> Empty()
        {
            return StringView<CharType>();
        }

        inline size_t Find(CharType c, size_t startIndex = 0) const
        {
            const CharType* end = m_String + m_Length;
            for (const CharType* ptr = m_String + startIndex; ptr < end; ptr++)
            {
                if (*ptr == c)
                    return ptr - m_String;
            }

            return NPos();
        }

        inline size_t RFind(CharType c) const
        {
            for (const CharType* ptr = m_String + m_Length; ptr-- > m_String;)
            {
                if (*ptr == c)
                    return ptr - m_String;
            }

            return NPos();
        }

        inline StringView<CharType> SubStr(size_t startIndex, size_t length)
        {
            return StringView<CharType>(*this, startIndex, length);
        }

        inline StringView<CharType> SubStr(size_t startIndex)
        {
            return StringView<CharType>(*this, startIndex, Length() - startIndex);
        }

        inline static size_t NPos()
        {
            return static_cast<size_t>(-1);
        }

        inline bool TryParseAsInt(int& outResult)
        {
            if (Length() == 0)
                return false;

            int result = 0;
            bool isNegative = false;
            const CharType* ptr = m_String;
            const CharType* end = m_String + m_Length;

            if (ptr[0] == '-')
            {
                isNegative = true;
                ptr++;
            }

            for (; ptr < end; ptr++)
            {
                CharType digit = *ptr;
                if (digit < '0' || digit > '9')
                    return false;

                int digitNumeric = digit - '0';
                if (result > INT_MAX / 10)
                    return false;

                result = result * 10;
                if (result > INT_MAX - digitNumeric)
                    return false;

                result += digitNumeric;
            }

            if (isNegative)
            {
                outResult = -result;
            }
            else
            {
                outResult = result;
            }

            return true;
        }
    };

#define StringViewAsNullTerminatedStringOf(CharType, stringView, variableName) \
    const CharType* variableName; \
    do \
    { \
        if (!stringView.IsEmpty() && stringView.IsNullTerminated()) \
        { \
            variableName = stringView.Str(); \
        } \
        else \
        { \
            CharType* buffer = static_cast<CharType*>(alloca((stringView.Length() + 1) * sizeof(CharType))); \
            memcpy(buffer, stringView.Str(), stringView.Length() * sizeof(CharType)); \
            buffer[stringView.Length()] = 0; \
            variableName = buffer; \
        } \
    } \
    while (false)
}
}
