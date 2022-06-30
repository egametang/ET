#include "il2cpp-config.h"

#if !RUNTIME_TINY

#include "MemoryMappedFile.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

namespace il2cpp
{
namespace utils
{
    static baselib::ReentrantLock s_Mutex;
    static std::map<void*, os::FileHandle*> s_MappedAddressToMappedFileObject;
    static std::map<void*, int64_t> s_MappedAddressToMappedLength;

    void* MemoryMappedFile::Map(os::FileHandle* file)
    {
        return Map(file, 0, 0);
    }

    bool MemoryMappedFile::Unmap(void* address)
    {
        return Unmap(address, 0);
    }

    void* MemoryMappedFile::Map(os::FileHandle* file, int64_t length, int64_t offset)
    {
        return Map(file, length, offset, os::MMAP_FILE_ACCESS_READ);
    }

    void* MemoryMappedFile::Map(os::FileHandle* file, int64_t length, int64_t offset, int32_t access)
    {
        os::FastAutoLock lock(&s_Mutex);

        int64_t unused = 0;
        os::MemoryMappedFileError error = os::NO_MEMORY_MAPPED_FILE_ERROR;
        os::FileHandle* mappedFileHandle = os::MemoryMappedFile::Create(file, NULL, 0, &unused, (os::MemoryMappedFileAccess)access, 0, &error);
        if (error != 0)
            return NULL;

        int64_t actualOffset = offset;
        void* address = os::MemoryMappedFile::View(mappedFileHandle, &length, offset, (os::MemoryMappedFileAccess)access, &actualOffset, &error);

        if (address != NULL)
        {
            address = (uint8_t*)address + (offset - actualOffset);
            if (os::MemoryMappedFile::OwnsDuplicatedFileHandle(mappedFileHandle))
                s_MappedAddressToMappedFileObject[address] = mappedFileHandle;
            s_MappedAddressToMappedLength[address] = length;
        }

        return address;
    }

    bool MemoryMappedFile::Unmap(void* address, int64_t length)
    {
        os::FastAutoLock lock(&s_Mutex);

        if (length == 0)
        {
            std::map<void*, int64_t>::iterator entry = s_MappedAddressToMappedLength.find(address);
            if (entry != s_MappedAddressToMappedLength.end())
            {
                length = entry->second;
                s_MappedAddressToMappedLength.erase(entry);
            }
        }

        bool success = os::MemoryMappedFile::UnmapView(address, length);
        if (!success)
            return false;

        std::map<void*, os::FileHandle*>::iterator entry = s_MappedAddressToMappedFileObject.find(address);
        if (entry != s_MappedAddressToMappedFileObject.end())
        {
            bool result = os::MemoryMappedFile::Close(entry->second);
            s_MappedAddressToMappedFileObject.erase(entry);
            return result;
        }

        return true;
    }
}
}

#endif
