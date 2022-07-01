#pragma once

#include <string>
#include "File.h"
#include <map>
#include "os/Mutex.h"

namespace il2cpp
{
namespace os
{
    // This enum must match the same enum in the Mono code, as it is used on the icall boundary.
    enum MemoryMappedFileAccess
    {
        MMAP_FILE_ACCESS_READ_WRITE = 0,
        MMAP_FILE_ACCESS_READ = 1,
        MMAP_FILE_ACCESS_WRITE = 2,
        MMAP_FILE_ACCESS_COPY_ON_WRITE = 3,
        MMAP_FILE_ACCESS_READ_EXECUTE = 4,
        MMAP_FILE_ACCESS_READ_WRITE_EXECUTE = 5,
    };

    // This enum must match the same enum in the Mono code, as it is used on the icall boundary.
    enum MemoryMappedFileError
    {
        NO_MEMORY_MAPPED_FILE_ERROR = 0,
        BAD_CAPACITY_FOR_FILE_BACKED,
        CAPACITY_SMALLER_THAN_FILE_SIZE,
        FILE_NOT_FOUND,
        FILE_ALREADY_EXISTS,
        PATH_TOO_LONG,
        COULD_NOT_OPEN,
        CAPACITY_MUST_BE_POSITIVE,
        INVALID_FILE_MODE,
        COULD_NOT_MAP_MEMORY,
        ACCESS_DENIED,
        CAPACITY_LARGER_THAN_LOGICAL_ADDRESS_SPACE
    };

    // This enum must match the same enum in the Mono code, as it is used on the icall boundary.
    enum MemoryMappedFileMode
    {
        FILE_MODE_CREATE_NEW = 1,
        FILE_MODE_CREATE = 2,
        FILE_MODE_OPEN = 3,
        FILE_MODE_OPEN_OR_CREATE = 4,
        FILE_MODE_TRUNCATE = 5,
        FILE_MODE_APPEND = 6,
    };

    class MemoryMappedFile
    {
    public:
        typedef void* MemoryMappedFileHandle;

        static FileHandle* Create(FileHandle* file, const char* mapName, int32_t mode, int64_t *capacity, MemoryMappedFileAccess access, int32_t options, MemoryMappedFileError* error);
        static MemoryMappedFileHandle View(FileHandle* mappedFileHandle, int64_t* length, int64_t offset, MemoryMappedFileAccess access, int64_t* actualOffset, MemoryMappedFileError* error);
        static void Flush(MemoryMappedFileHandle memoryMappedFileData, int64_t length);
        static bool UnmapView(MemoryMappedFileHandle memoryMappedFileData, int64_t length);
        static bool Close(FileHandle* file);
        static void ConfigureHandleInheritability(FileHandle* file, bool inheritability);
    };
}
}
