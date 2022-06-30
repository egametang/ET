#include "il2cpp-config.h"

#if !IL2CPP_USE_GENERIC_MEMORY_MAPPED_FILE && IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include <sys/mman.h>
#include <map>
#include "os/File.h"
#include "os/MemoryMappedFile.h"
#include "os/Mutex.h"
#include "utils/Memory.h"
#include "FileHandle.h"
#include <unistd.h>
#include <fcntl.h>

#ifdef DEFFILEMODE
#define DEFAULT_FILEMODE DEFFILEMODE
#else
#define DEFAULT_FILEMODE 0666
#endif

#ifndef MAP_32BIT
#define MAP_32BIT 0
#endif

#define MMAP_PAGE_SIZE 4096

namespace il2cpp
{
namespace os
{
    enum MemoryMappedFileFlags
    {
        /* protection */
        MONO_MMAP_NONE = 0,
        MONO_MMAP_READ    = 1 << 0,
        MONO_MMAP_WRITE   = 1 << 1,
        MONO_MMAP_EXEC    = 1 << 2,
        /* make the OS discard the dirty data and fill with 0 */
        MONO_MMAP_DISCARD = 1 << 3,
        /* other flags (add commit, sync) */
        MONO_MMAP_PRIVATE = 1 << 4,
        MONO_MMAP_SHARED  = 1 << 5,
        MONO_MMAP_ANON    = 1 << 6,
        MONO_MMAP_FIXED   = 1 << 7,
        MONO_MMAP_32BIT   = 1 << 8
    };

    static bool IsSpecialZeroSizeFile(struct stat *buf)
    {
        return buf->st_size == 0 && (buf->st_mode & (S_IFCHR | S_IFBLK | S_IFIFO | S_IFSOCK)) != 0;
    }

    static int64_t AlignUpToPageSize(int64_t size)
    {
        int64_t page_size = MMAP_PAGE_SIZE;
        return (size + page_size - 1) & ~(page_size - 1);
    }

    static int64_t AlignDownToPageSize(int64_t size)
    {
        int64_t page_size = MMAP_PAGE_SIZE;
        return size & ~(page_size - 1);
    }

    static int ConvertAccessToMemoryMappedFileFlags(int access)
    {
        switch (access)
        {
            case MMAP_FILE_ACCESS_READ_WRITE:
                return MONO_MMAP_WRITE | MONO_MMAP_READ | MONO_MMAP_SHARED;

            case MMAP_FILE_ACCESS_WRITE:
                return MONO_MMAP_WRITE | MONO_MMAP_SHARED;

            case MMAP_FILE_ACCESS_COPY_ON_WRITE:
                return MONO_MMAP_WRITE | MONO_MMAP_READ | MONO_MMAP_PRIVATE;

            case MMAP_FILE_ACCESS_READ_EXECUTE:
                return MONO_MMAP_EXEC | MONO_MMAP_PRIVATE | MONO_MMAP_SHARED;

            case MMAP_FILE_ACCESS_READ_WRITE_EXECUTE:
                return MONO_MMAP_WRITE | MONO_MMAP_READ | MONO_MMAP_EXEC | MONO_MMAP_SHARED;

            case MMAP_FILE_ACCESS_READ:
                return MONO_MMAP_READ | MONO_MMAP_SHARED;
            default:
                IL2CPP_ASSERT("unknown MemoryMappedFileAccess");
        }

        return 0;
    }

    static int ConvertFileModeToUnix(int mode)
    {
        switch (mode)
        {
            case FILE_MODE_CREATE_NEW:
                return O_CREAT | O_EXCL;
            case FILE_MODE_CREATE:
                return O_CREAT | O_TRUNC;
            case FILE_MODE_OPEN:
                return 0;
            case FILE_MODE_OPEN_OR_CREATE:
                return O_CREAT;
            case FILE_MODE_TRUNCATE:
                return O_TRUNC;
            case FILE_MODE_APPEND:
                return O_APPEND;
            default:
                IL2CPP_ASSERT("unknown FileMode");
        }

        return 0;
    }

    static int ConvertAccessModeToUnix(int access)
    {
        switch (access)
        {
            case MMAP_FILE_ACCESS_READ_WRITE:
            case MMAP_FILE_ACCESS_COPY_ON_WRITE:
            case MMAP_FILE_ACCESS_READ_WRITE_EXECUTE:
                return O_RDWR;
            case MMAP_FILE_ACCESS_READ:
            case MMAP_FILE_ACCESS_READ_EXECUTE:
                return O_RDONLY;
            case MMAP_FILE_ACCESS_WRITE:
                return O_WRONLY;
            default:
                IL2CPP_ASSERT("unknown MemoryMappedFileAccess");
        }

        return 0;
    }

    static int ConvertFlagsToProt(int flags)
    {
        int prot = PROT_NONE;
        /* translate the protection bits */
        if (flags & MONO_MMAP_READ)
            prot |= PROT_READ;
        if (flags & MONO_MMAP_WRITE)
            prot |= PROT_WRITE;
        if (flags & MONO_MMAP_EXEC)
            prot |= PROT_EXEC;
        return prot;
    }

    FileHandle* MemoryMappedFile::Create(FileHandle* file, const char* mapName, int32_t mode, int64_t *capacity, MemoryMappedFileAccess access, int32_t options, MemoryMappedFileError* error)
    {
        struct stat buf;
        int result, fd;

        if (file != NULL)
            result = fstat(file->fd, &buf);
        else
            result = stat(mapName, &buf);

        if (mode == FILE_MODE_TRUNCATE || mode == FILE_MODE_APPEND || mode == FILE_MODE_OPEN)
        {
            if (result == -1)
            {
                if (error != NULL)
                    *error = FILE_NOT_FOUND;
                return NULL;
            }
        }

        if (mode == FILE_MODE_CREATE_NEW && result == 0)
        {
            if (error != NULL)
                *error = FILE_ALREADY_EXISTS;
            return NULL;
        }

        if (result == 0)
        {
            if (*capacity == 0)
            {
                /**
                 * Special files such as FIFOs, sockets, and devices can have a size of 0. Specifying a capacity for these
                 * also makes little sense, so don't do the check if th file is one of these.
                 */
                if (buf.st_size == 0 && !IsSpecialZeroSizeFile(&buf))
                {
                    if (error != NULL)
                        *error = CAPACITY_SMALLER_THAN_FILE_SIZE;
                    return NULL;
                }
                *capacity = buf.st_size;
            }
            else if (*capacity < buf.st_size)
            {
                if (error != NULL)
                    *error = CAPACITY_SMALLER_THAN_FILE_SIZE;
                return NULL;
            }
        }
        else
        {
            if (mode == FILE_MODE_CREATE_NEW && *capacity == 0)
            {
                if (error != NULL)
                    *error = CAPACITY_SMALLER_THAN_FILE_SIZE;
                return NULL;
            }
        }

        bool ownsFd = true;
        if (file != NULL)
        {
#if IL2CPP_HAS_DUP
            fd = dup(file->fd);
#else
            fd = file->fd;
            ownsFd = false;
#endif
        }
        else
        {
            fd = open(mapName, ConvertFileModeToUnix(mode) | ConvertAccessModeToUnix(access), DEFAULT_FILEMODE);
        }

        if (fd == -1)
        {
            if (error != NULL)
                *error = COULD_NOT_OPEN;
            return NULL;
        }

        if (result != 0 || *capacity > buf.st_size)
        {
            int result = ftruncate(fd, (off_t)*capacity);
            IL2CPP_ASSERT(result == 0);
            NO_UNUSED_WARNING(result);
        }

        if (ownsFd)
        {
            FileHandle* handle = new FileHandle;
            handle->fd = fd;
            handle->doesNotOwnFd = !ownsFd;
            return handle;
        }
        else
        {
            file->doesNotOwnFd = true;
            return file;
        }
    }

    MemoryMappedFile::MemoryMappedFileHandle MemoryMappedFile::View(FileHandle* mappedFileHandle, int64_t* length, int64_t offset, MemoryMappedFileAccess access, int64_t* actualOffset, MemoryMappedFileError* error)
    {
        IL2CPP_ASSERT(actualOffset != NULL);

        int64_t mmap_offset = 0;
        int64_t eff_size = *length;
        struct stat buf = { 0 };
        fstat(mappedFileHandle->fd, &buf);

        if (offset > buf.st_size || ((eff_size + offset) > buf.st_size && !IsSpecialZeroSizeFile(&buf)))
        {
            if (error != NULL)
                *error = ACCESS_DENIED;
            return NULL;
        }
        /**
          * We use the file size if one of the following conditions is true:
          *  -input size is zero
          *  -input size is bigger than the file and the file is not a magical zero size file such as /dev/mem.
          */
        if (eff_size == 0)
            eff_size = AlignUpToPageSize(buf.st_size) - offset;

        mmap_offset = AlignDownToPageSize(offset);
        eff_size += (offset - mmap_offset);
        *length = eff_size;
        *actualOffset = mmap_offset;

        int flags = ConvertAccessToMemoryMappedFileFlags(access);
        int prot = ConvertFlagsToProt(flags);

        /* translate the flags */
        int mflags = 0;
        if (flags & MONO_MMAP_PRIVATE)
            mflags |= MAP_PRIVATE;
        if (flags & MONO_MMAP_SHARED)
            mflags |= MAP_SHARED;
        if (flags & MONO_MMAP_FIXED)
            mflags |= MAP_FIXED;
        if (flags & MONO_MMAP_32BIT)
            mflags |= MAP_32BIT;

        void* address = mmap(NULL, eff_size, prot, mflags, mappedFileHandle->fd, mmap_offset);
        if (address == MAP_FAILED)
        {
            if (error != NULL)
                *error = COULD_NOT_MAP_MEMORY;
            return NULL;
        }

        return address;
    }

    void MemoryMappedFile::Flush(MemoryMappedFileHandle memoryMappedFileData, int64_t length)
    {
        if (memoryMappedFileData != NULL)
        {
            int error = msync(memoryMappedFileData, length, MS_SYNC);
            IL2CPP_ASSERT(error == 0);
            NO_UNUSED_WARNING(error);
        }
    }

    bool MemoryMappedFile::UnmapView(MemoryMappedFileHandle memoryMappedFileData, int64_t length)
    {
        int error = munmap(memoryMappedFileData, (size_t)length);
        return error == 0;
    }

    bool MemoryMappedFile::Close(FileHandle* file)
    {
        bool result = true;
        if (!file->doesNotOwnFd)
        {
            int error = 0;
            os::File::Close(file, &error);
            if (error != 0)
                result = false;
        }

        return result;
    }

    void MemoryMappedFile::ConfigureHandleInheritability(FileHandle* file, bool inheritability)
    {
#if IL2CPP_HAS_CLOSE_EXEC
        int flags = fcntl(file->fd, F_GETFD, 0);
        if (inheritability)
            flags &= ~FD_CLOEXEC;
        else
            flags |= FD_CLOEXEC;
        int result = fcntl(file->fd, F_SETFD, flags);
        IL2CPP_ASSERT(result != -1);
        NO_UNUSED_WARNING(result);
#endif
    }

    bool MemoryMappedFile::OwnsDuplicatedFileHandle(FileHandle* file)
    {
        return !file->doesNotOwnFd;
    }
}
}
#endif
