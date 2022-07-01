#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE

#include <io.h>

#include "il2cpp-vm-support.h"
#include "os/File.h"
#include "os/Win32/WindowsHeaders.h"

namespace il2cpp
{
namespace os
{
    static inline std::wstring GetDirectoryForStandardOutput()
    {
#if IL2CPP_TARGET_XBOXONE

        return L"D:\\";

#elif IL2CPP_TARGET_WINRT

        wchar_t buffer[MAX_PATH + 2];
        uint32_t tempPathLength = GetTempPathW(MAX_PATH + 2, buffer);
        return std::wstring(buffer, tempPathLength);

#else
#error Unknown platform
#endif
    }

    static FileHandle* GetOrCreateRedirectedHandle(FILE* stdFile, const wchar_t* fileNameOnDisk)
    {
#if IL2CPP_DEBUG || IL2CPP_DEVELOPMENT
        auto stdFd = _fileno(stdFile);
        auto stdHandle = reinterpret_cast<FileHandle*>(_get_osfhandle(stdFd));

        if (stdHandle != INVALID_HANDLE_VALUE && !_isatty(stdFd))
            return stdHandle;

        std::wstring pathOnDisk = GetDirectoryForStandardOutput() + fileNameOnDisk;
        auto redirectedFile = _wfreopen(pathOnDisk.c_str(), L"w+", stdFile);
        return reinterpret_cast<FileHandle*>(_get_osfhandle(_fileno(redirectedFile)));
#else
        return NULL;
#endif
    }

    bool File::Isatty(FileHandle* fileHandle)
    {
        IL2CPP_VM_NOT_IMPLEMENTED(File::IsAtty);
        return false;
    }

    FileHandle* File::GetStdInput()
    {
        return GetOrCreateRedirectedHandle(stdin, L"stdin.txt");
    }

    FileHandle* File::GetStdError()
    {
        return GetOrCreateRedirectedHandle(stderr, L"stderr.txt");
    }

    FileHandle* File::GetStdOutput()
    {
        return GetOrCreateRedirectedHandle(stdout, L"stdout.txt");
    }
} //os
} //il2cpp

#endif
