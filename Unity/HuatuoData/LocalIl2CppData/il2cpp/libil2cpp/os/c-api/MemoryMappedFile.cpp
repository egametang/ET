#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/c-api/MemoryMappedFile-c-api.h"
#include "utils/MemoryMappedFile.h"

extern "C"
{
    void* UnityPalMemoryMappedFileMap(UnityPalFileHandle* file)
    {
        return il2cpp::utils::MemoryMappedFile::Map(file);
    }

    void UnityPalMemoryMappedFileUnmap(void* address)
    {
        il2cpp::utils::MemoryMappedFile::Unmap(address);
    }

    void* UnityPalMemoryMappedFileMapWithParams(UnityPalFileHandle* file, int64_t length, int64_t offset)
    {
        return il2cpp::utils::MemoryMappedFile::Map(file, length, offset);
    }

    void UnityPalMemoryMappedFileUnmapWithParams(void* address, int64_t length)
    {
        il2cpp::utils::MemoryMappedFile::Unmap(address, length);
    }
}

#endif
