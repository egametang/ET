#include <algorithm>
#include <vector>
#include "il2cpp-config.h"
#include "MemoryMapImpl.h"
#include "os/File.h"
#include "os/MemoryMappedFile.h"
#include "os/Mutex.h"
#include "utils/StringUtils.h"
#include "utils/Memory.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace Core
{
namespace System
{
namespace IO
{
namespace MemoryMappedFiles
{
    static baselib::ReentrantLock s_Mutex;
    static std::vector<os::FileHandle*> s_OwnedFileHandles;

    typedef struct
    {
        void *address;
        void *free_handle;
        size_t length;
    } MmapInstance;

    static int32_t ConvertMemoryMappedFileAccessToIL2CPPFileAccess(os::MemoryMappedFileAccess access)
    {
        switch (access)
        {
            case os::MMAP_FILE_ACCESS_READ:
            case os::MMAP_FILE_ACCESS_READ_EXECUTE:
                return kFileAccessRead;
            case os::MMAP_FILE_ACCESS_READ_WRITE:
            case os::MMAP_FILE_ACCESS_COPY_ON_WRITE:
            case os::MMAP_FILE_ACCESS_READ_WRITE_EXECUTE:
                return kFileAccessRead | kFileAccessWrite;
            case os::MMAP_FILE_ACCESS_WRITE:
                return kFileAccessWrite;
            default:
                IL2CPP_ASSERT("unknown MemoryMappedFileAccess");
        }

        return kFileAccessRead;
    }

    static void CloseFileHandleIfOwned(os::FileHandle* file)
    {
        std::vector<os::FileHandle*>::iterator ownedHandleEntry = std::find(s_OwnedFileHandles.begin(), s_OwnedFileHandles.end(), file);
        if (ownedHandleEntry != s_OwnedFileHandles.end())
        {
            int error = 0;
            os::File::Close(file, &error);
            s_OwnedFileHandles.erase(ownedHandleEntry);
            IL2CPP_ASSERT(error == 0);
        }
    }

    bool MemoryMapImpl::Unmap(intptr_t mmap_handle)
    {
        IL2CPP_ASSERT(mmap_handle);

        MmapInstance *h = (MmapInstance*)mmap_handle;

        bool result = os::MemoryMappedFile::UnmapView(h->address, h->length);

        IL2CPP_FREE(h);

        return result;
    }

    int32_t MemoryMapImpl::MapInternal(intptr_t handle, int64_t offset, int64_t* size, int32_t access, intptr_t* mmap_handle, intptr_t* base_address)
    {
        IL2CPP_ASSERT(handle);

        MmapInstance* h = (MmapInstance*)IL2CPP_MALLOC_ZERO(sizeof(MmapInstance));

        os::MemoryMappedFileError error = os::NO_MEMORY_MAPPED_FILE_ERROR;
        int64_t actualOffset = offset;
        h->address = os::MemoryMappedFile::View((os::FileHandle*)handle, size, offset, (os::MemoryMappedFileAccess)access, &actualOffset, &error);
        h->length = (size_t)*size;

        if (h->address)
        {
            *mmap_handle = (intptr_t)h;
            *base_address = (intptr_t)((char*)h->address + (offset - actualOffset));
        }
        else
        {
            IL2CPP_FREE(h);
        }

        return error;
    }

    static os::FileHandle* OpenHandle(os::FileHandle* handle, Il2CppString* mapName, os::MemoryMappedFileMode mode, int64_t* capacity, int32_t access, int32_t options, int32_t* error)
    {
        os::MemoryMappedFileError memoryMappedFileError = os::NO_MEMORY_MAPPED_FILE_ERROR;
        std::string utf8MapNameString = mapName != NULL ? utils::StringUtils::Utf16ToUtf8(mapName->chars) : std::string();
        const char* utf8MapName = !utf8MapNameString.empty() ? utf8MapNameString.c_str() : NULL;
        os::FileHandle* memoryMappedFileData = os::MemoryMappedFile::Create(handle, utf8MapName, mode, capacity, (os::MemoryMappedFileAccess)access, options, &memoryMappedFileError);

        *error = (int32_t)memoryMappedFileError;
        return memoryMappedFileData;
    }

    intptr_t MemoryMapImpl::OpenFileInternal(Il2CppString* path, int32_t mode, Il2CppString* mapName, int64_t* capacity, int32_t access, int32_t options, int32_t* error)
    {
        IL2CPP_ASSERT(path || mapName);
        os::FastAutoLock lock(&s_Mutex);

        *error = 0;

        os::FileHandle* file = NULL;
        if (path != NULL)
        {
            std::string filePath = utils::StringUtils::Utf16ToUtf8(path->chars);
            file = os::File::Open(filePath, mode, ConvertMemoryMappedFileAccessToIL2CPPFileAccess((os::MemoryMappedFileAccess)access), 0, options, error);

            s_OwnedFileHandles.push_back(file);
        }

        os::FileHandle* result = NULL;
        if (*error == 0)
        {
            result = OpenHandle(file, mapName, (os::MemoryMappedFileMode)mode, capacity, access, options, error);
            if (result != NULL && result != file)
                CloseFileHandleIfOwned(file);
        }

        return (intptr_t)result;
    }

    intptr_t MemoryMapImpl::OpenHandleInternal(intptr_t handle, Il2CppString* mapName, int64_t* capacity, int32_t access, int32_t options, int32_t* error)
    {
        IL2CPP_ASSERT(handle);

        return (intptr_t)OpenHandle((os::FileHandle*)handle, mapName, os::FILE_MODE_OPEN, capacity, access, options, error);
    }

    void MemoryMapImpl::CloseMapping(intptr_t handle)
    {
        IL2CPP_ASSERT(handle);
        os::FastAutoLock lock(&s_Mutex);

        os::FileHandle* file = (os::FileHandle*)handle;

        os::MemoryMappedFile::Close(file);
        CloseFileHandleIfOwned(file);
    }

    void MemoryMapImpl::ConfigureHandleInheritability(intptr_t handle, int32_t inheritability)
    {
        IL2CPP_ASSERT(handle);

        os::MemoryMappedFile::ConfigureHandleInheritability((os::FileHandle*)handle, inheritability);
    }

    void MemoryMapImpl::Flush(intptr_t mmap_handle)
    {
        IL2CPP_ASSERT(mmap_handle);

        MmapInstance *h = (MmapInstance*)mmap_handle;
        os::MemoryMappedFile::Flush(h->address, h->length);
    }
} // namespace MemoryMappedFiles
} // namespace IO
} // namespace System
} // namespace Core
} // namespace System
} // namespace icalls
} // namespace il2cpp
