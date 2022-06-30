#include "il2cpp-config.h"

#if !IL2CPP_USE_GENERIC_MEMORY_MAPPED_FILE && IL2CPP_TARGET_WINDOWS

#include <map>
#include <limits>
#include "WindowsHelpers.h"
#include "os/MemoryMappedFile.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace os
{
    static DWORD ConvertMappedFileAccessToWindowsPageAccess(MemoryMappedFileAccess access)
    {
        switch (access)
        {
            case MMAP_FILE_ACCESS_READ:
                return PAGE_READONLY;
            case MMAP_FILE_ACCESS_READ_WRITE:
                return PAGE_READWRITE;
            case MMAP_FILE_ACCESS_COPY_ON_WRITE:
                return PAGE_WRITECOPY;
            case MMAP_FILE_ACCESS_READ_EXECUTE:
                return PAGE_EXECUTE_READ;
            case MMAP_FILE_ACCESS_READ_WRITE_EXECUTE:
                return PAGE_EXECUTE_READWRITE;
            default:
                IL2CPP_ASSERT("unknown MemoryMappedFileAccess");
        }

        return MMAP_FILE_ACCESS_READ;
    }

    static int ConvertMappedFileAccessToWindowsFileAccess(MemoryMappedFileAccess access)
    {
        switch (access)
        {
            case MMAP_FILE_ACCESS_READ:
                return FILE_MAP_READ;
            case MMAP_FILE_ACCESS_WRITE:
                return FILE_MAP_WRITE;
            case MMAP_FILE_ACCESS_READ_WRITE:
                return FILE_MAP_READ | FILE_MAP_WRITE;
            case MMAP_FILE_ACCESS_COPY_ON_WRITE:
                return FILE_MAP_COPY;
            case MMAP_FILE_ACCESS_READ_EXECUTE:
                return FILE_MAP_EXECUTE | FILE_MAP_READ;
            case MMAP_FILE_ACCESS_READ_WRITE_EXECUTE:
                return FILE_MAP_EXECUTE | FILE_MAP_READ | FILE_MAP_WRITE;
            default:
                IL2CPP_ASSERT("unknown MemoryMappedFileAccess");
        }

        return MMAP_FILE_ACCESS_READ;
    }

    static MemoryMappedFileError ConvertWindowsErrorToMemoryMappedFileError(DWORD error, MemoryMappedFileError defaultError)
    {
        switch (error)
        {
            case ERROR_FILE_NOT_FOUND:
                return FILE_NOT_FOUND;
            case ERROR_FILE_EXISTS:
            case ERROR_ALREADY_EXISTS:
                return FILE_ALREADY_EXISTS;
            case ERROR_ACCESS_DENIED:
                return ACCESS_DENIED;
        }
        return defaultError;
    }

    FileHandle* MemoryMappedFile::Create(FileHandle* file, const char* mapName, int32_t mode, int64_t *capacity, MemoryMappedFileAccess access, int32_t options, MemoryMappedFileError* error)
    {
        HANDLE result = NULL;
        HANDLE handle = file != NULL ? file : INVALID_HANDLE_VALUE;

        if (handle == INVALID_HANDLE_VALUE)
        {
            if (*capacity <= 0 && mode != os::FILE_MODE_OPEN)
            {
                *error = CAPACITY_MUST_BE_POSITIVE;
                return NULL;
            }
#if IL2CPP_SIZEOF_VOID_P == 4
            if (*capacity > UINT32_MAX)
            {
                *error = CAPACITY_LARGER_THAN_LOGICAL_ADDRESS_SPACE;
                return NULL;
            }
#endif
            if (!(mode == FILE_MODE_CREATE_NEW || mode == FILE_MODE_OPEN_OR_CREATE || mode == FILE_MODE_OPEN))
            {
                *error = INVALID_FILE_MODE;
                return NULL;
            }
        }
        else
        {
            FILE_STANDARD_INFO info;
            if (!GetFileInformationByHandleEx(handle, FileStandardInfo, &info, sizeof(FILE_STANDARD_INFO)))
            {
                *error = ConvertWindowsErrorToMemoryMappedFileError(GetLastError(), COULD_NOT_OPEN);
                return NULL;
            }
            if (*capacity == 0)
            {
                if (info.EndOfFile.QuadPart == 0)
                {
                    *error = CAPACITY_SMALLER_THAN_FILE_SIZE;
                    return NULL;
                }
            }
            else if (*capacity < info.EndOfFile.QuadPart)
            {
                *error = CAPACITY_SMALLER_THAN_FILE_SIZE;
                return NULL;
            }
        }

        UTF16String utf16MapNameString = mapName != NULL ? il2cpp::utils::StringUtils::Utf8ToUtf16(mapName) : UTF16String();
        LPCWSTR utf16MapName = mapName != NULL ? utf16MapNameString.c_str() : NULL;

        if (mode == FILE_MODE_CREATE_NEW || handle != INVALID_HANDLE_VALUE)
        {
            result = CreateFileMapping(handle, NULL, ConvertMappedFileAccessToWindowsPageAccess(access) | options, (DWORD)(((uint64_t)*capacity) >> 32), (DWORD)*capacity, utf16MapName);
            if (result && GetLastError() == ERROR_ALREADY_EXISTS)
            {
                CloseHandle(result);
                result = NULL;
                *error = FILE_ALREADY_EXISTS;
            }
            else if (!result && GetLastError() != NO_ERROR)
            {
                *error = ConvertWindowsErrorToMemoryMappedFileError(GetLastError(), COULD_NOT_OPEN);
            }
        }
        else if (mode == FILE_MODE_OPEN || mode == FILE_MODE_OPEN_OR_CREATE && access == MMAP_FILE_ACCESS_WRITE)
        {
            result = OpenFileMappingW(ConvertMappedFileAccessToWindowsFileAccess(access), FALSE, utf16MapName);
            if (!result)
            {
                if (mode == FILE_MODE_OPEN_OR_CREATE && GetLastError() == ERROR_FILE_NOT_FOUND)
                {
                    *error = INVALID_FILE_MODE;
                }
                else
                {
                    *error = ConvertWindowsErrorToMemoryMappedFileError(GetLastError(), COULD_NOT_OPEN);
                }
            }
        }
        else if (mode == FILE_MODE_OPEN_OR_CREATE)
        {
            // This replicates how CoreFX does MemoryMappedFile.CreateOrOpen ().

            /// Try to open the file if it exists -- this requires a bit more work. Loop until we can
            /// either create or open a memory mapped file up to a timeout. CreateFileMapping may fail
            /// if the file exists and we have non-null security attributes, in which case we need to
            /// use OpenFileMapping.  But, there exists a race condition because the memory mapped file
            /// may have closed between the two calls -- hence the loop.
            ///
            /// The retry/timeout logic increases the wait time each pass through the loop and times
            /// out in approximately 1.4 minutes. If after retrying, a MMF handle still hasn't been opened,
            /// throw an InvalidOperationException.

            uint32_t waitRetries = 14;   //((2^13)-1)*10ms == approximately 1.4mins
            uint32_t waitSleep = 0;

            while (waitRetries > 0)
            {
                result = CreateFileMapping(handle, NULL, ConvertMappedFileAccessToWindowsPageAccess(access) | options, (DWORD)(((uint64_t)*capacity) >> 32), (DWORD)*capacity, utf16MapName);
                if (result)
                    break;
                if (GetLastError() != ERROR_ACCESS_DENIED)
                {
                    *error = ConvertWindowsErrorToMemoryMappedFileError(GetLastError(), COULD_NOT_OPEN);
                    break;
                }
                result = OpenFileMapping(ConvertMappedFileAccessToWindowsFileAccess(access), FALSE, utf16MapName);
                if (result)
                    break;
                if (GetLastError() != ERROR_FILE_NOT_FOUND)
                {
                    *error = ConvertWindowsErrorToMemoryMappedFileError(GetLastError(), COULD_NOT_OPEN);
                    break;
                }
                // increase wait time
                --waitRetries;
                if (waitSleep == 0)
                {
                    waitSleep = 10;
                }
                else
                {
                    Sleep(waitSleep);
                    waitSleep *= 2;
                }
            }

            if (!result)
                *error = COULD_NOT_OPEN;
        }

        return (os::FileHandle*)result;
    }

    MemoryMappedFile::MemoryMappedFileHandle MemoryMappedFile::View(FileHandle* mappedFileHandle, int64_t* length, int64_t offset, MemoryMappedFileAccess access, int64_t* actualOffset, MemoryMappedFileError* error)
    {
        IL2CPP_ASSERT(actualOffset != NULL);
        IL2CPP_ASSERT(offset <= std::numeric_limits<DWORD>::max());
        IL2CPP_ASSERT(*length <= std::numeric_limits<DWORD>::max());

        static DWORD allocationGranularity = 0;
        if (allocationGranularity == 0)
        {
            SYSTEM_INFO info;
            GetSystemInfo(&info);
            allocationGranularity = info.dwAllocationGranularity;
        }

        int64_t extraMemNeeded = offset % allocationGranularity;
        uint64_t newOffset = offset - extraMemNeeded;
        uint64_t nativeSize = (*length != 0) ? *length + extraMemNeeded : 0;
        *actualOffset = newOffset;

        void* address = MapViewOfFile((HANDLE)mappedFileHandle, ConvertMappedFileAccessToWindowsFileAccess(access), (DWORD)(newOffset >> 32), (DWORD)newOffset, (SIZE_T)nativeSize);
        if (address == NULL)
        {
            if (error != NULL)
                *error = ConvertWindowsErrorToMemoryMappedFileError(GetLastError(), COULD_NOT_MAP_MEMORY);

            CloseHandle(mappedFileHandle);
            return NULL;
        }

        // Query the view for its size and allocation type
        MEMORY_BASIC_INFORMATION viewInfo;
        VirtualQuery(address, &viewInfo, sizeof(MEMORY_BASIC_INFORMATION));
        uint64_t viewSize = (uint64_t)viewInfo.RegionSize;

        // Allocate the pages if we were using the MemoryMappedFileOptions.DelayAllocatePages option
        // OR check if the allocated view size is smaller than the expected native size
        // If multiple overlapping views are created over the file mapping object, the pages in a given region
        // could have different attributes(MEM_RESERVE OR MEM_COMMIT) as MapViewOfFile preserves coherence between
        // views created on a mapping object backed by same file.
        // In which case, the viewSize will be smaller than nativeSize required and viewState could be MEM_COMMIT
        // but more pages may need to be committed in the region.
        // This is because, VirtualQuery function(that internally invokes VirtualQueryEx function) returns the attributes
        // and size of the region of pages with matching attributes starting from base address.
        // VirtualQueryEx: http://msdn.microsoft.com/en-us/library/windows/desktop/aa366907(v=vs.85).aspx
        if (((viewInfo.State & MEM_RESERVE) != 0) || viewSize < (uint64_t)nativeSize)
        {
            void *tempAddress = VirtualAlloc(address, (SIZE_T)(nativeSize != 0 ? nativeSize : viewSize), MEM_COMMIT, ConvertMappedFileAccessToWindowsPageAccess(access));
            if (!tempAddress)
            {
                if (error != NULL)
                    *error = ConvertWindowsErrorToMemoryMappedFileError(GetLastError(), COULD_NOT_MAP_MEMORY);
                return NULL;
            }
            // again query the view for its new size
            VirtualQuery(address, &viewInfo, sizeof(MEMORY_BASIC_INFORMATION));
            viewSize = (uint64_t)viewInfo.RegionSize;
        }

        if (*length == 0)
            *length = viewSize - extraMemNeeded;

        return address;
    }

    void MemoryMappedFile::Flush(MemoryMappedFileHandle memoryMappedFileData, int64_t length)
    {
        BOOL success = FlushViewOfFile(memoryMappedFileData, (SIZE_T)length);
        if (success)
            return;

        // This replicates how CoreFX does MemoryMappedView.Flush ().

        // It is a known issue within the NTFS transaction log system that
        // causes FlushViewOfFile to intermittently fail with ERROR_LOCK_VIOLATION
        // As a workaround, we catch this particular error and retry the flush operation
        // a few milliseconds later. If it does not work, we give it a few more tries with
        // increasing intervals. Eventually, however, we need to give up. In ad-hoc tests
        // this strategy successfully flushed the view after no more than 3 retries.

        if (GetLastError() != ERROR_LOCK_VIOLATION)
            // TODO: Propagate error to caller
            return;

        // These control the retry behaviour when lock violation errors occur during Flush:
        const int MAX_FLUSH_WAITS = 15;  // must be <=30
        const int MAX_FLUSH_RETIRES_PER_WAIT = 20;

        for (int w = 0; w < MAX_FLUSH_WAITS; w++)
        {
            int pause = (1 << w);  // MaxFlushRetries should never be over 30
            Sleep(pause);

            for (int r = 0; r < MAX_FLUSH_RETIRES_PER_WAIT; r++)
            {
                if (FlushViewOfFile(memoryMappedFileData, (SIZE_T)length))
                    return;

                if (GetLastError() != ERROR_LOCK_VIOLATION)
                    // TODO: Propagate error to caller
                    return;

                SwitchToThread();
            }
        }

        // We got to here, so there was no success
        IL2CPP_ASSERT(false);
    }

    bool MemoryMappedFile::UnmapView(MemoryMappedFileHandle memoryMappedFileData, int64_t length)
    {
        if (memoryMappedFileData != NULL)
        {
            BOOL success = UnmapViewOfFile(memoryMappedFileData);
            IL2CPP_ASSERT(success);
            if (!success)
                return false;
        }

        return true;
    }

    bool MemoryMappedFile::Close(FileHandle* file)
    {
        BOOL success = CloseHandle(file);
        IL2CPP_ASSERT(success);
        return success;
    }

    void MemoryMappedFile::ConfigureHandleInheritability(FileHandle* file, bool inheritability)
    {
#if IL2CPP_TARGET_WINDOWS_DESKTOP
        BOOL success = SetHandleInformation((HANDLE)file, HANDLE_FLAG_INHERIT, inheritability ? HANDLE_FLAG_INHERIT : 0);
        IL2CPP_ASSERT(success);
#endif
    }

    bool MemoryMappedFile::OwnsDuplicatedFileHandle(FileHandle* file)
    {
        return true;
    }
}
}
#endif
