#include "il2cpp-config.h"
#include "pal_platform.h"

#if IL2CPP_USES_POSIX_CLASS_LIBRARY_PAL

#include "os/ClassLibraryPAL/pal_mirror_structs.h"
#include "os/File.h"
#include "os/Posix/FileHandle.h"

#include <stdlib.h>
#include <errno.h>
#include <sys/stat.h>
#include <unistd.h>

#if IL2CPP_HAVE_FCOPYFILE
#include <copyfile.h>
#endif

struct DirectoryEntry;

extern "C"
{
#define READDIR_SORT 1

    struct DIRWrapper
    {
        DIR* dir;
#if READDIR_SORT
        void* result;
        size_t curIndex;
        size_t numEntries;
#if !IL2CPP_HAVE_REWINDDIR
        char* dirPath;
#endif
#endif
    };
    // Items needed by mscorlib
    IL2CPP_EXPORT int32_t SystemNative_Stat2(const char* path, struct FileStatus* output); // 179
    IL2CPP_EXPORT int32_t SystemNative_LStat2(const char* path, struct FileStatus* output); // 207
    IL2CPP_EXPORT int32_t SystemNative_Unlink(const char* path); // 305
    IL2CPP_EXPORT int32_t SystemNative_GetReadDirRBufferSize(void); // 371
    IL2CPP_EXPORT int32_t SystemNative_ReadDirR(struct DIRWrapper* dirWrapper, uint8_t* buffer, int32_t bufferSize, struct DirectoryEntry* outputEntry); // 388
    IL2CPP_EXPORT struct DIRWrapper* SystemNative_OpenDir(const char* path); // 468
    IL2CPP_EXPORT int32_t SystemNative_CloseDir(struct DIRWrapper* dirWrapper); // 473
    IL2CPP_EXPORT int32_t SystemNative_MkDir(const char* path, int32_t mode); // 592
    IL2CPP_EXPORT int32_t SystemNative_ChMod(const char* path, int32_t mode); // 599
    IL2CPP_EXPORT int32_t SystemNative_Link(const char* source, const char* linkTarget); // 660
    IL2CPP_EXPORT int32_t SystemNative_ReadLink(const char* path, char* buffer, int32_t bufferSize); // 1142
    IL2CPP_EXPORT int32_t SystemNative_Rename(const char* oldPath, const char* newPath); // 1159
    IL2CPP_EXPORT int32_t SystemNative_RmDir(const char* path); // 1166
    IL2CPP_EXPORT int32_t SystemNative_CopyFile(intptr_t sourceFd, intptr_t destinationFd); // 1251
    IL2CPP_EXPORT int32_t SystemNative_LChflagsCanSetHiddenFlag(); // 1482
}


/* Provide consistent access to nanosecond fields, if they exist. */
/* Seconds are always available through st_atime, st_mtime, st_ctime. */

#if IL2CPP_HAVE_STAT_TIMESPEC

#define ST_ATIME_NSEC(statstruct) ((statstruct)->st_atimespec.tv_nsec)
#define ST_MTIME_NSEC(statstruct) ((statstruct)->st_mtimespec.tv_nsec)
#define ST_CTIME_NSEC(statstruct) ((statstruct)->st_ctimespec.tv_nsec)

#else /* HAVE_STAT_TIMESPEC */

#if IL2CPP_HAVE_STAT_TIM

#define ST_ATIME_NSEC(statstruct) ((statstruct)->st_atim.tv_nsec)
#define ST_MTIME_NSEC(statstruct) ((statstruct)->st_mtim.tv_nsec)
#define ST_CTIME_NSEC(statstruct) ((statstruct)->st_ctim.tv_nsec)

#else /* HAVE_STAT_TIM */

#if IL2CPP_HAVE_STAT_NSEC

#define ST_ATIME_NSEC(statstruct) ((statstruct)->st_atimensec)
#define ST_MTIME_NSEC(statstruct) ((statstruct)->st_mtimensec)
#define ST_CTIME_NSEC(statstruct) ((statstruct)->st_ctimensec)

#else /* HAVE_STAT_NSEC */

#define ST_ATIME_NSEC(statstruct) 0
#define ST_MTIME_NSEC(statstruct) 0
#define ST_CTIME_NSEC(statstruct) 0

#endif /* HAVE_STAT_NSEC */
#endif /* HAVE_STAT_TIM */
#endif /* HAVE_STAT_TIMESPEC */

/**
 * Constants for interpreting FileStatus.Flags.
 */
enum
{
    FILESTATUS_FLAGS_NONE = 0,
    FILESTATUS_FLAGS_HAS_BIRTHTIME = 1,
};

/**
 * Constants for interpreting FileStatus.UserFlags.
 */
enum
{
    PAL_UF_HIDDEN = 0x8000
};

/**
 * Constants from dirent.h for the inode type returned from readdir variants
 */
enum NodeType
{
    PAL_DT_UNKNOWN = 0, // Unknown file type
    PAL_DT_FIFO = 1,    // Named Pipe
    PAL_DT_CHR = 2,     // Character Device
    PAL_DT_DIR = 4,     // Directory
    PAL_DT_BLK = 6,     // Block Device
    PAL_DT_REG = 8,     // Regular file
    PAL_DT_LNK = 10,    // Symlink
    PAL_DT_SOCK = 12,   // Socket
    PAL_DT_WHT = 14     // BSD Whiteout
};

/**
 * Our intermediate dirent struct that only gives back the data we need
 */
struct DirectoryEntry
{
    const char* Name;   // Address of the name of the inode
    int32_t NameLength; // Length (in chars) of the inode name
    int32_t InodeType; // The inode type as described in the NodeType enum
};

static void ConvertFileStatus(const struct stat_* src, struct FileStatus* dst)
{
    dst->Dev = (int64_t)src->st_dev;
    dst->Ino = (int64_t)src->st_ino;
    dst->Flags = FILESTATUS_FLAGS_NONE;
    dst->Mode = (int32_t)src->st_mode;
    dst->Uid = src->st_uid;
    dst->Gid = src->st_gid;
    dst->Size = src->st_size;

    dst->ATime = src->st_atime;
    dst->MTime = src->st_mtime;
    dst->CTime = src->st_ctime;

    dst->ATimeNsec = ST_ATIME_NSEC(src);
    dst->MTimeNsec = ST_MTIME_NSEC(src);
    dst->CTimeNsec = ST_CTIME_NSEC(src);

#if IL2CPP_HAVE_STAT_BIRTHTIME
    dst->BirthTime = src->st_birthtimespec.tv_sec;
    dst->BirthTimeNsec = src->st_birthtimespec.tv_nsec;
    dst->Flags |= FILESTATUS_FLAGS_HAS_BIRTHTIME;
#else
    // Linux path: until we use statx() instead
    dst->BirthTime = 0;
    dst->BirthTimeNsec = 0;
#endif

#if defined(IL2CPP_HAVE_STAT_FLAGS) && defined(UF_HIDDEN)
    dst->UserFlags = ((src->st_flags & UF_HIDDEN) == UF_HIDDEN) ? PAL_UF_HIDDEN : 0;
#else
    dst->UserFlags = 0;
#endif
}

#if IL2CPP_HAVE_REMAP_PATH
    #define REMAP_PATH(path) pal_remap_path(path).c_str()
#else
    #define REMAP_PATH(path) path
#endif

// CoreCLR expects the "2" suffixes on these: they should be cleaned up in our
// next coordinated System.Native changes
int32_t SystemNative_Stat2(const char* path, struct FileStatus* output)
{
    struct stat_ result = {};
    int ret;
    while ((ret = stat_(REMAP_PATH(path), &result)) < 0 && errno == EINTR)
        ;

    if (ret == 0)
    {
        ConvertFileStatus(&result, output);
    }

    return ret;
}

int32_t SystemNative_LStat2(const char* path, struct FileStatus* output)
{
    struct stat_ result = {};
    int ret = lstat_(REMAP_PATH(path), &result);

    if (ret == 0)
    {
        ConvertFileStatus(&result, output);
    }

    return ret;
}

int32_t SystemNative_Unlink(const char* path)
{
    int32_t result = 0;
    while ((result = unlink(REMAP_PATH(path))) < 0 && errno == EINTR)
        ;
    return result;
}

static void ConvertDirent(const struct dirent* entry, struct DirectoryEntry* outputEntry)
{
    // We use Marshal.PtrToStringAnsi on the managed side, which takes a pointer to
    // the start of the unmanaged string. Give the caller back a pointer to the
    // location of the start of the string that exists in their own byte buffer.
    outputEntry->Name = entry->d_name;
#if !defined(DT_UNKNOWN)
    // AIX has no d_type, and since we can't get the directory that goes with
    // the filename from ReadDir, we can't stat the file. Return unknown and
    // hope that managed code can properly stat the file.
    outputEntry->InodeType = PAL_DT_UNKNOWN;
#else
    outputEntry->InodeType = (int32_t)entry->d_type;
#endif

#if IL2CPP_HAVE_DIRENT_NAME_LEN
    outputEntry->NameLength = entry->d_namlen;
#else
    outputEntry->NameLength = -1; // sentinel value to mean we have to walk to find the first \0
#endif
}

#if IL2CPP_HAVE_READDIR_R_DEPRECATED_DO_NOT_USE
// struct dirent typically contains 64-bit numbers (e.g. d_ino), so we align it at 8-byte.
static const size_t dirent_alignment = 8;
#endif

int32_t SystemNative_GetReadDirRBufferSize(void)
{
#if IL2CPP_HAVE_READDIR_R_DEPRECATED_DO_NOT_USE
    // dirent should be under 2k in size
    IL2CPP_ASSERT(sizeof(struct dirent) < 2048);
    // add some extra space so we can align the buffer to dirent.
    return sizeof(struct dirent) + dirent_alignment - 1;
#else
    return 0;
#endif
}

#if READDIR_SORT
static int cmpstring(const void *p1, const void *p2)
{
    return strcmp(((struct dirent*)p1)->d_name, ((struct dirent*)p2)->d_name);
}

#endif

// To reduce the number of string copies, the caller of this function is responsible to ensure the memory
// referenced by outputEntry remains valid until it is read.
// If the platform supports readdir_r, the caller provides a buffer into which the data is read.
// If the platform uses readdir, the caller must ensure no calls are made to readdir/closedir since those will invalidate
// the current dirent. We assume the platform supports concurrent readdir calls to different DIRs.
int32_t SystemNative_ReadDirR(struct DIRWrapper* dirWrapper, uint8_t* buffer, int32_t bufferSize, struct DirectoryEntry* outputEntry)
{
    IL2CPP_ASSERT(dirWrapper != NULL);
    IL2CPP_ASSERT(dirWrapper->dir != NULL);
    IL2CPP_ASSERT(outputEntry != NULL);

#if IL2CPP_HAVE_READDIR_R_DEPRECATED_DO_NOT_USE
    IL2CPP_ASSERT(buffer != NULL);

    // align to dirent
    struct dirent* entry = (struct dirent*)((size_t)(buffer + dirent_alignment - 1) & ~(dirent_alignment - 1));

    // check there is dirent size available at entry
    if ((buffer + bufferSize) < ((uint8_t*)entry + sizeof(struct dirent)))
    {
        IL2CPP_ASSERT(false && "Buffer size too small; use GetReadDirRBufferSize to get required buffer size");
        return ERANGE;
    }

    struct dirent* result = NULL;
#ifdef _AIX
    // AIX returns 0 on success, but bizarrely, it returns 9 for both error and
    // end-of-directory. result is NULL for both cases. The API returns the
    // same thing for EOD/error, so disambiguation between the two is nearly
    // impossible without clobbering errno for yourself and seeing if the API
    // changed it. See:
    // https://www.ibm.com/support/knowledgecenter/ssw_aix_71/com.ibm.aix.basetrf2/readdir_r.htm

    errno = 0; // create a success condition for the API to clobber
    int error = readdir_r(dir, entry, &result);

    if (error == 9)
    {
        memset(outputEntry, 0, sizeof(*outputEntry)); // managed out param must be initialized
        return errno == 0 ? -1 : errno;
    }
#else
    int error = readdir_r(dir, entry, &result);

    // positive error number returned -> failure
    if (error != 0)
    {
        IL2CPP_ASSERT(error > 0);
        memset(outputEntry, 0, sizeof(*outputEntry)); // managed out param must be initialized
        return error;
    }

    // 0 returned with null result -> end-of-stream
    if (result == NULL)
    {
        memset(outputEntry, 0, sizeof(*outputEntry)); // managed out param must be initialized
        return -1;         // shim convention for end-of-stream
    }
#endif

    // 0 returned with non-null result (guaranteed to be set to entry arg) -> success
    IL2CPP_ASSERT(result == entry);
#else
    (void)buffer;     // unused
    (void)bufferSize; // unused
    errno = 0;

#if READDIR_SORT
    struct dirent* entry;

    if (!dirWrapper->result)
    {
        size_t numEntries = 0;
        while ((entry = readdir(dirWrapper->dir)))
            numEntries++;
        if (numEntries)
        {
            dirWrapper->result = malloc(numEntries * sizeof(struct dirent));
            dirWrapper->curIndex = 0;
            dirWrapper->numEntries = numEntries;
#if IL2CPP_HAVE_REWINDDIR
            rewinddir(dirWrapper->dir);
#else
            closedir(dirWrapper->dir);
            dirWrapper->dir = opendir(dirWrapper->dirPath);
#endif

            size_t index = 0;
            while ((entry = readdir(dirWrapper->dir)))
            {
                memcpy(&((struct dirent*)dirWrapper->result)[index], entry, sizeof(struct dirent));
                index++;
            }

            qsort(dirWrapper->result, numEntries, sizeof(struct dirent), cmpstring);
        }
    }

    if (dirWrapper->curIndex < dirWrapper->numEntries)
    {
        entry = &((struct dirent*)dirWrapper->result)[dirWrapper->curIndex];
        dirWrapper->curIndex++;
    }
    else
        entry = NULL;

#else
    struct dirent* entry = readdir(dirWrapper->dir);
#endif

    // 0 returned with null result -> end-of-stream
    if (entry == NULL)
    {
        memset(outputEntry, 0, sizeof(*outputEntry)); // managed out param must be initialized

        //  kernel set errno -> failure
        if (errno != 0)
        {
            IL2CPP_ASSERT(errno == EBADF && "Invalid directory stream descriptor dir" && errno);
            return errno;
        }
        return -1;
    }
#endif
    ConvertDirent(entry, outputEntry);
    return 0;
}

struct DIRWrapper* SystemNative_OpenDir(const char* path)
{
    const char* remapped_path = NULL;

#if IL2CPP_HAVE_REMAP_PATH
    auto remapped_path_string = pal_remap_path(path);
    remapped_path = remapped_path_string.c_str();
#else
    remapped_path = path;
#endif

    DIR* dir = opendir(remapped_path);

    if (dir == NULL)
        return NULL;

    struct DIRWrapper* ret = (struct DIRWrapper*)malloc(sizeof(struct DIRWrapper));
    ret->dir = dir;
#if READDIR_SORT
    ret->result = NULL;
    ret->curIndex = 0;
    ret->numEntries = 0;
#if !IL2CPP_HAVE_REWINDDIR
    ret->dirPath = strdup(remapped_path);
#endif
#endif
    return ret;
}

int32_t SystemNative_CloseDir(struct DIRWrapper* dirWrapper)
{
    IL2CPP_ASSERT(dirWrapper != NULL);
    int32_t ret = closedir(dirWrapper->dir);
#if READDIR_SORT
    if (dirWrapper->result)
        free(dirWrapper->result);
    dirWrapper->result = NULL;
#if !IL2CPP_HAVE_REWINDDIR
    if (dirWrapper->dirPath)
        free(dirWrapper->dirPath);
#endif
    free(dirWrapper);
#endif

    return ret;
}

int32_t SystemNative_MkDir(const char* path, int32_t mode)
{
    int32_t result = 0;
    while ((result = mkdir(REMAP_PATH(path), (mode_t)mode)) < 0 && errno == EINTR)
        ;
    return result;
}

int32_t SystemNative_ChMod(const char* path, int32_t mode)
{
    int32_t result = 0;
    while ((result = chmod(REMAP_PATH(path), (mode_t)mode)) < 0 && errno == EINTR)
        ;
    return result;
}

int32_t SystemNative_Link(const char* source, const char* linkTarget)
{
    int32_t result = 0;
    while ((result = link(REMAP_PATH(source), REMAP_PATH(linkTarget))) < 0 && errno == EINTR)
        ;
    return result;
}

int32_t SystemNative_Symlink(const char* target, const char* linkPath)
{
    return symlink(REMAP_PATH(target), REMAP_PATH(linkPath));
}

int32_t SystemNative_ReadLink(const char* path, char* buffer, int32_t bufferSize)
{
    IL2CPP_ASSERT(buffer != NULL || bufferSize == 0);
    IL2CPP_ASSERT(bufferSize >= 0);

    if (bufferSize <= 0)
    {
        errno = EINVAL;
        return -1;
    }

    ssize_t count = readlink(REMAP_PATH(path), buffer, (size_t)bufferSize);
    IL2CPP_ASSERT(count >= -1 && count <= bufferSize);

    return (int32_t)count;
}

int32_t SystemNative_Rename(const char* oldPath, const char* newPath)
{
    int32_t result;
    while ((result = rename(REMAP_PATH(oldPath), REMAP_PATH(newPath))) < 0 && errno == EINTR)
        ;
    return result;
}

int32_t SystemNative_RmDir(const char* path)
{
    int32_t result = 0;
    while ((result = rmdir(REMAP_PATH(path))) < 0 && errno == EINTR)
        ;
    return result;
}

#if !IL2CPP_HAVE_FCOPYFILE
// Read all data from inFd and write it to outFd
static int32_t CopyFile_ReadWrite(int inFd, int outFd)
{
    // Allocate a buffer
    const int BufferLength = 80 * 1024 * sizeof(char);
    char* buffer = (char*)malloc(BufferLength);
    if (buffer == NULL)
    {
        return -1;
    }

    // Repeatedly read from the source and write to the destination
    while (true)
    {
        // Read up to what will fit in our buffer.  We're done if we get back 0 bytes.
        ssize_t bytesRead;
        while ((bytesRead = read(inFd, buffer, BufferLength)) < 0 && errno == EINTR)
            ;
        if (bytesRead == -1)
        {
            int tmp = errno;
            free(buffer);
            errno = tmp;
            return -1;
        }
        if (bytesRead == 0)
        {
            break;
        }
        IL2CPP_ASSERT(bytesRead > 0);

        // Write what was read.
        ssize_t offset = 0;
        while (bytesRead > 0)
        {
            ssize_t bytesWritten;
            while ((bytesWritten = write(outFd, buffer + offset, (size_t)bytesRead)) < 0 && errno == EINTR)
                ;
            if (bytesWritten == -1)
            {
                int tmp = errno;
                free(buffer);
                errno = tmp;
                return -1;
            }
            IL2CPP_ASSERT(bytesWritten >= 0);
            bytesRead -= bytesWritten;
            offset += bytesWritten;
        }
    }

    free(buffer);
    return 0;
}

#endif // !IL2CPP_HAVE_FCOPYFILE

int32_t SystemNative_CopyFile(intptr_t sourceFd, intptr_t destinationFd)
{
#if IL2CPP_HAVE_CUSTOM_COPYFILE
    return pal_custom_copy_file(sourceFd, destinationFd);
#else
    int inFd, outFd;

    inFd = il2cpp::os::File::IsHandleOpenFileHandle(sourceFd) ? reinterpret_cast<il2cpp::os::FileHandle*>(sourceFd)->fd : static_cast<int>(sourceFd);
    outFd = il2cpp::os::File::IsHandleOpenFileHandle(destinationFd) ? reinterpret_cast<il2cpp::os::FileHandle*>(destinationFd)->fd : static_cast<int>(destinationFd);

#if IL2CPP_HAVE_FCOPYFILE
    // If fcopyfile is available (OS X), try to use it, as the whole copy
    // can be performed in the kernel, without lots of unnecessary copying.
    // Copy data and metadata.
    return fcopyfile(inFd, outFd, NULL, COPYFILE_ALL) == 0 ? 0 : -1;
#else
    // Get the stats on the source file.
    int ret;
    struct stat_ sourceStat;
    bool copied = false;

    // First, stat the source file.
    while ((ret = fstat_(inFd, &sourceStat)) < 0 && errno == EINTR)
        ;
    if (ret != 0)
    {
        // If we can't stat() it, then we likely don't have permission to read it.
        return -1;
    }

    // Copy permissions.  This fchmod() needs to happen prior to writing anything into
    // the file to avoid possibly leaking any private data.
    while ((ret = fchmod(outFd, sourceStat.st_mode & (S_IRWXU | S_IRWXG | S_IRWXO))) < 0 && errno == EINTR)
        ;
#if !IL2CPP_CANNOT_MODIFY_FILE_PERMISSIONS
    if (ret != 0)
    {
        return -1;
    }
#endif

#if IL2CPP_HAVE_SENDFILE_4
    // If sendfile is available (Linux), try to use it, as the whole copy
    // can be performed in the kernel, without lots of unnecessary copying.

    // On 32-bit, if you use 64-bit offsets, the last argument of `sendfile' will be a
    // `size_t' a 32-bit integer while the `st_size' field of the stat structure will be off64_t.
    // So `size' will have to be `uint64_t'. In all other cases, it will be `size_t'.
    uint64_t size = (uint64_t)sourceStat.st_size;

    // Note that per man page for large files, you have to iterate until the
    // whole file is copied (Linux has a limit of 0x7ffff000 bytes copied).
    while (size > 0)
    {
        ssize_t sent = sendfile(outFd, inFd, NULL, (size >= SSIZE_MAX ? SSIZE_MAX : (size_t)size));
        if (sent < 0)
        {
            if (errno != EINVAL && errno != ENOSYS)
            {
                return -1;
            }
            else
            {
                break;
            }
        }
        else
        {
            IL2CPP_ASSERT((size_t)sent <= size);
            size -= (size_t)sent;
        }
    }
    if (size == 0)
    {
        copied = true;
    }
    // sendfile couldn't be used; fall back to a manual copy below. This could happen
    // if we're on an old kernel, for example, where sendfile could only be used
    // with sockets and not regular files.
#endif // IL2CPP_HAVE_SENDFILE_4

    // Manually read all data from the source and write it to the destination.
    if (!copied && CopyFile_ReadWrite(inFd, outFd) != 0)
    {
        return -1;
    }

    // Now that the data from the file has been copied, copy over metadata
    // from the source file.  First copy the file times.
    // If futimes nor futimes are available on this platform, file times will
    // not be copied over.
#if IL2CPP_HAVE_FUTIMENS
    // futimens is prefered because it has a higher resolution.
    struct timespec origTimes[2];
    origTimes[0].tv_sec = (time_t)sourceStat.st_atime;
    origTimes[0].tv_nsec = ST_ATIME_NSEC(&sourceStat);
    origTimes[1].tv_sec = (time_t)sourceStat.st_mtime;
    origTimes[1].tv_nsec = ST_MTIME_NSEC(&sourceStat);
    while ((ret = futimens(outFd, origTimes)) < 0 && errno == EINTR)
        ;
#elif IL2CPP_HAVE_FUTIMES
    struct timeval origTimes[2];
    origTimes[0].tv_sec = sourceStat.st_atime;
    origTimes[0].tv_usec = ST_ATIME_NSEC(&sourceStat) / 1000;
    origTimes[1].tv_sec = sourceStat.st_mtime;
    origTimes[1].tv_usec = ST_MTIME_NSEC(&sourceStat) / 1000;
    while ((ret = futimes(outFd, origTimes)) < 0 && errno == EINTR)
        ;
#endif

#if !IL2CPP_CANNOT_MODIFY_FILE_PERMISSIONS
    if (ret != 0)
    {
        return -1;
    }
#endif

    return 0;
#endif // IL2CPP_HAVE_FCOPYFILE
#endif // IL2CPP_HAVE_CUSTOM_COPYFILE
}

int32_t SystemNative_LChflagsCanSetHiddenFlag(void)
{
#if defined(UF_HIDDEN) && defined(IL2CPP_HAVE_STAT_FLAGS) && defined(IL2CPP_HAVE_LCHFLAGS)
    return true;
#else
    return false;
#endif
}

#endif
