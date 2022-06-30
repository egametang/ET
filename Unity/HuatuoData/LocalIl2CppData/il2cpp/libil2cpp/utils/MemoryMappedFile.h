#pragma once

#include <map>
#include "os/File.h"
#include "os/Mutex.h"
#include "os/MemoryMappedFile.h"

namespace il2cpp
{
namespace utils
{
    class MemoryMappedFile
    {
    public:
        static void* Map(os::FileHandle* file);
        static void* Map(os::FileHandle* file, int64_t length, int64_t offset);
        static void* Map(os::FileHandle* file, int64_t length, int64_t offset, int32_t access);
        static bool Unmap(void* address);
        static bool Unmap(void* address, int64_t length);
    };
}
}
