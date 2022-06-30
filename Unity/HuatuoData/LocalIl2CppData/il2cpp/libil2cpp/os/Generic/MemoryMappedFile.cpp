#include "il2cpp-config.h"

#if IL2CPP_USE_GENERIC_MEMORY_MAPPED_FILE
#include "os/File.h"
#include "os/MemoryMappedFile.h"
#include "os/Mutex.h"
#include "utils/dynamic_array.h"
#include "utils/Memory.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

namespace il2cpp
{
namespace os
{
    struct MemoryFileData
    {
        MemoryFileData(const char* name, int64_t dataSize)
            : size(dataSize)
        {
            size_t nameSize = strlen(name);
            mapName = (char*)IL2CPP_MALLOC(nameSize  + 1);
            strcpy(mapName, name);

            handle = IL2CPP_MALLOC_ZERO((size_t)dataSize);
        }

        ~MemoryFileData()
        {
            IL2CPP_FREE(mapName);
            IL2CPP_FREE(handle);
        }

        char* mapName;
        void* handle;
        int64_t size;
    };

    static baselib::ReentrantLock s_Mutex;
    static il2cpp::utils::dynamic_array<MemoryFileData*> s_MemoryFiles;
    static il2cpp::utils::dynamic_array<void*> s_MemoryFileViews;

    static bool IsMemoryFile(void* possibleMemoryFile)
    {
        os::FastAutoLock lock(&s_Mutex);
        for (il2cpp::utils::dynamic_array<MemoryFileData*>::const_iterator it = s_MemoryFiles.begin(); it != s_MemoryFiles.end(); ++it)
        {
            if ((*it)->handle == possibleMemoryFile)
                return true;
        }

        return false;
    }

    static MemoryFileData* FindMemoryFile(void* possibleMemoryFile)
    {
        for (il2cpp::utils::dynamic_array<MemoryFileData*>::iterator it = s_MemoryFiles.begin(); it != s_MemoryFiles.end(); ++it)
        {
            if ((*it)->handle == possibleMemoryFile)
            {
                return *it;
            }
        }

        return NULL;
    }

    static MemoryFileData* FindMemoryFile(const char* mapName)
    {
        for (il2cpp::utils::dynamic_array<MemoryFileData*>::iterator it = s_MemoryFiles.begin(); it != s_MemoryFiles.end(); ++it)
        {
            if (strcmp((*it)->mapName, mapName) == 0)
            {
                return *it;
            }
        }

        return NULL;
    }

    static bool IsMemoryFileView(void* possibleMemoryFile)
    {
        os::FastAutoLock lock(&s_Mutex);
        return std::find(s_MemoryFileViews.begin(), s_MemoryFileViews.end(), possibleMemoryFile) != s_MemoryFileViews.end();
    }

    FileHandle* MemoryMappedFile::Create(FileHandle* file, const char* mapName, int32_t mode, int64_t *capacity, MemoryMappedFileAccess access, int32_t options, MemoryMappedFileError* error)
    {
        if (file == NULL)
        {
            if (mapName != NULL)
            {
                os::FastAutoLock lock(&s_Mutex);

                const MemoryFileData* memoryFileData = FindMemoryFile(mapName);

                if (memoryFileData != NULL)
                {
                    file = (FileHandle*)memoryFileData->handle;
                }
                else
                {
                    MemoryFileData* memoryFileData = new MemoryFileData(mapName, *capacity);
                    s_MemoryFiles.push_back(memoryFileData);

                    file = (FileHandle*)memoryFileData->handle;
                }
            }
        }

        return file;
    }

    static MemoryMappedFile::MemoryMappedFileHandle ViewRealFile(FileHandle* mappedFileHandle, int64_t* length, int64_t offset, MemoryMappedFileAccess access, MemoryMappedFileError* error)
    {
        int fileError = 0;
        if (*length == 0)
        {
            *length = os::File::GetLength(mappedFileHandle, &fileError);
            if (fileError != 0)
            {
                if (error != NULL)
                    *error = COULD_NOT_MAP_MEMORY;
                return NULL;
            }
        }

        void* buffer = IL2CPP_MALLOC_ZERO((size_t)*length);

        os::File::Seek(mappedFileHandle, offset, 0, &fileError);
        if (fileError != 0)
        {
            IL2CPP_FREE(buffer);
            if (error != NULL)
                *error = COULD_NOT_MAP_MEMORY;
            return NULL;
        }

        int bytesRead = File::Read(mappedFileHandle, (char*)buffer, (int)*length, &fileError);
        if (bytesRead != *length || fileError != 0)
        {
            IL2CPP_FREE(buffer);
            if (error != NULL)
                *error = COULD_NOT_MAP_MEMORY;
            return NULL;
        }

        return buffer;
    }

    static MemoryMappedFile::MemoryMappedFileHandle ViewMemoryFile(void* mappedFileHandle, int64_t* length, int64_t offset, MemoryMappedFileAccess access, MemoryMappedFileError* error)
    {
        os::FastAutoLock lock(&s_Mutex);
        const MemoryFileData* memoryFileData = FindMemoryFile(mappedFileHandle);
        IL2CPP_ASSERT(memoryFileData != NULL);

        if (*length == 0)
            *length = memoryFileData->size;

        if (*length > memoryFileData->size)
        {
            if (error != NULL)
                *error = COULD_NOT_MAP_MEMORY;
            return NULL;
        }

        void* memoryMappedFileView = (uint8_t*)memoryFileData->handle + offset;

        s_MemoryFileViews.push_back(memoryMappedFileView);

        return memoryMappedFileView;
    }

    MemoryMappedFile::MemoryMappedFileHandle MemoryMappedFile::View(FileHandle* mappedFileHandle, int64_t* length, int64_t offset, MemoryMappedFileAccess access, int64_t* actualOffset, MemoryMappedFileError* error)
    {
        IL2CPP_ASSERT(actualOffset != NULL);

        *actualOffset = offset;
        if (IsMemoryFile((void*)mappedFileHandle))
            return ViewMemoryFile((void*)mappedFileHandle, length, offset, access, error);
        else
            return ViewRealFile(mappedFileHandle, length, offset, access, error);
    }

    void MemoryMappedFile::Flush(MemoryMappedFileHandle memoryMappedFileData, int64_t length)
    {
    }

    bool MemoryMappedFile::UnmapView(MemoryMappedFileHandle memoryMappedFileData, int64_t length)
    {
        if (IsMemoryFileView(memoryMappedFileData))
            s_MemoryFileViews.erase_swap_back(&memoryMappedFileData);
        else if (!IsMemoryFile(memoryMappedFileData))
            IL2CPP_FREE(memoryMappedFileData);
        return true;
    }

    bool MemoryMappedFile::Close(FileHandle* file)
    {
        os::FastAutoLock lock(&s_Mutex);
        MemoryFileData* memoryFileData = FindMemoryFile((void*)file);
        if (memoryFileData != NULL)
        {
            delete memoryFileData;
            s_MemoryFiles.erase_swap_back(&memoryFileData);
        }
        return true;
    }

    void MemoryMappedFile::ConfigureHandleInheritability(FileHandle* file, bool inheritability)
    {
    }

    bool MemoryMappedFile::OwnsDuplicatedFileHandle(FileHandle* file)
    {
        return true;
    }
}
}
#endif
