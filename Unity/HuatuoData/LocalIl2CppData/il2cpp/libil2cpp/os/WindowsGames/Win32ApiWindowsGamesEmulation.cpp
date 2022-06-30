#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS_GAMES

#include "Win32ApiWindowsGamesEmulation.h"

#include "os/Win32/WindowsHeaders.h"

#undef GetFileAttributes

#include "os/File.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace os
{
    UnityPalFileAttributes File::GetFileAttributes(const std::string& path, int *error)
    {
        const UTF16String utf16Path(utils::StringUtils::Utf8ToUtf16(path.c_str()));

        // HACK: DLC api returns these funky long form UNC paths (\\?\GLOBALROOT\)
        // For some reason even though many of the file APIs on Xbox understand such a path
        // GetFileAttributesEx does not, so we check explicitly for such a path, and fake it.
        wchar_t * p = (wchar_t*)utf16Path.c_str();
        size_t len = wcslen(p);
        if (((len > 3 && (p[0] == L'\\' && p[1] == L'?'  && p[2] == L'\\'))
             || (len > 4 && (p[0] == L'\\' && p[1] == L'\\' && p[2] == L'?' && p[3] == L'\\'))
            ) && NULL != wcsstr(p, L"GLOBALROOT\\Device\\Harddisk"))
        {
            size_t diff = len - (wcsstr(p, L"Partition") - p);
            if (diff <= 11)
                return static_cast<UnityPalFileAttributes>(FILE_ATTRIBUTE_DIRECTORY | FILE_ATTRIBUTE_HIDDEN);
        }

        WIN32_FILE_ATTRIBUTE_DATA fileAttributes;

        BOOL result = ::GetFileAttributesExW((LPCWSTR)utf16Path.c_str(), GetFileExInfoStandard, &fileAttributes);
        if (result == FALSE)
        {
            *error = ::GetLastError();
            return static_cast<UnityPalFileAttributes>(INVALID_FILE_ATTRIBUTES);
        }

        *error = kErrorCodeSuccess;
        return static_cast<UnityPalFileAttributes>(fileAttributes.dwFileAttributes);
    }
} //os
} //il2cpp


#endif
