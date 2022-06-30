#include <iostream>

#include "CommonDef.h"


namespace huatuo
{
    void LogPanic(const char* errMsg)
    {
        std::cerr << "panic:" << std::endl;
        std::cerr << "\t" << errMsg << std::endl;
        exit(1);
    }

    const char* GetAssemblyNameFromPath(const char* assPath)
    {
        const char* last = nullptr;
        for (const char* p = assPath; *p; p++)
        {
            if (*p == '/' || *p == '\\')
            {
                last = p + 1;
            }
        }
        return last ? last : assPath;
    }

    const char* CopyString(const char* src)
    {
        size_t len = std::strlen(src);
        char* dst = (char*)IL2CPP_MALLOC(len + 1);
        std::strcpy(dst, src);
        return dst;
    }

    const char* ConcatNewString(const char* s1, const char* s2)
    {
        size_t len1 = std::strlen(s1);
        size_t len = len1 + std::strlen(s2);
        char* dst = (char*)IL2CPP_MALLOC(len + 1);
        std::strcpy(dst, s1);
        strcpy(dst + len1, s2);
        return dst;
    }

    void* CopyBytes(const void* src, size_t length)
    {
        void* dst = IL2CPP_MALLOC(length);
        std::memcpy(dst, src, length);
        return dst;
    }
}




