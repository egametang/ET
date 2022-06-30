#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX

#include "FileHandle.h"

#if IL2CPP_USE_POSIX_FILE_PLATFORM_CONFIG
#include "FilePlatformConfig.h"
#endif

#include "os/ConsoleExtension.h"
#include "os/ErrorCodes.h"
#include "os/File.h"
#include "os/Mutex.h"
#include "os/Posix/Error.h"
#include "utils/Expected.h"
#include "utils/Il2CppError.h"
#include "utils/PathUtils.h"

#if IL2CPP_SUPPORT_THREADS
#include "Baselib.h"
#include "Cpp/ReentrantLock.h"
#endif

#include <fcntl.h>
#include <stdint.h>
#include <unistd.h>
#include <utime.h>
#include <sys/errno.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <string>

#define INVALID_FILE_HANDLE     (FileHandle*)-1
#define INVALID_FILE_ATTRIBUTES (UnityPalFileAttributes)((uint32_t)-1)
#define TIME_ZERO               116444736000000000ULL

namespace il2cpp
{
namespace os
{
// Head and tail of linked list.
    static FileHandle* s_fileHandleHead = NULL;
    static FileHandle* s_fileHandleTail = NULL;
#if IL2CPP_SUPPORT_THREADS
    static baselib::ReentrantLock s_fileHandleMutex;
#endif

    static void AddFileHandle(FileHandle *fileHandle)
    {
#if IL2CPP_SUPPORT_THREADS
        FastAutoLock autoLock(&s_fileHandleMutex);
#endif

        if (s_fileHandleHead == NULL)
        {
            IL2CPP_ASSERT(s_fileHandleTail == NULL);

            s_fileHandleHead = fileHandle;
            s_fileHandleTail = fileHandle;
        }
        else
        {
            IL2CPP_ASSERT(s_fileHandleTail != NULL);
            IL2CPP_ASSERT(s_fileHandleTail->next == NULL);

            s_fileHandleTail->next = fileHandle;
            fileHandle->prev = s_fileHandleTail;
            s_fileHandleTail = fileHandle;
        }
    }

    static void RemoveFileHandle(il2cpp::os::FileHandle *fileHandle)
    {
#if IL2CPP_SUPPORT_THREADS
        FastAutoLock autoLock(&s_fileHandleMutex);
#endif

        if (s_fileHandleHead == fileHandle)
            s_fileHandleHead = fileHandle->next;

        if (s_fileHandleTail == fileHandle)
            s_fileHandleTail = fileHandle->prev;

        if (fileHandle->prev)
            fileHandle->prev->next = fileHandle->next;

        if (fileHandle->next)
            fileHandle->next->prev = fileHandle->prev;
    }

    static const FileHandle* FindFileHandle(const struct stat& statBuf)
    {
#if IL2CPP_SUPPORT_THREADS
        FastAutoLock autoLock(&s_fileHandleMutex);
#endif

        const dev_t device = statBuf.st_dev;
        const ino_t inode = statBuf.st_ino;

        for (FileHandle *handle = s_fileHandleHead; handle != NULL; handle = handle->next)
        {
            if (handle->device == device && handle->inode == inode)
                return handle;
        }

        return NULL;
    }

    bool File::IsHandleOpenFileHandle(intptr_t lookup)
    {
#if IL2CPP_SUPPORT_THREADS
        FastAutoLock autoLock(&s_fileHandleMutex);
#endif

        for (FileHandle *handle = s_fileHandleHead; handle != NULL; handle = handle->next)
        {
            if (reinterpret_cast<intptr_t>(handle) == lookup)
                return true;
        }

        return false;
    }

// NOTE:
// Checking for file sharing violations only works for the current process.
//
// Mono implements this feature across processes by storing the file handles as
// a look up table in a shared file.

    static bool ShareAllowOpen(const struct stat& statBuf, int shareMode, int accessMode)
    {
        const FileHandle *fileHandle = FindFileHandle(statBuf);

        if (fileHandle == NULL) // File is not open
            return true;

        if (fileHandle->shareMode == kFileShareNone || shareMode == kFileShareNone)
            return false;

        if (((fileHandle->shareMode == kFileShareRead)  && (accessMode != kFileAccessRead)) ||
            ((fileHandle->shareMode == kFileShareWrite) && (accessMode != kFileAccessWrite)))
        {
            return false;
        }

        return true;
    }

    static UnityPalFileAttributes StatToFileAttribute(const std::string& path, struct stat& pathStat, struct stat* linkStat)
    {
        uint32_t fileAttributes = 0;

        if (S_ISSOCK(pathStat.st_mode))
            pathStat.st_mode &= ~S_IFSOCK; // don't consider socket protection

#if defined(__APPLE__) && defined(__MACH__)
        if ((pathStat.st_flags & UF_IMMUTABLE) || (pathStat.st_flags & SF_IMMUTABLE))
            fileAttributes |= kFileAttributeReadOnly;
#endif

        const std::string filename(il2cpp::utils::PathUtils::Basename(path));

        if (S_ISDIR(pathStat.st_mode))
        {
            fileAttributes = kFileAttributeDirectory;

            if (!(pathStat.st_mode & S_IWUSR) && !(pathStat.st_mode & S_IWGRP) && !(pathStat.st_mode & S_IWOTH))
                fileAttributes |= kFileAttributeReadOnly;

            if (filename[0] == '.')
                fileAttributes |= kFileAttributeHidden;
        }
        else
        {
            if (!(pathStat.st_mode & S_IWUSR) && !(pathStat.st_mode & S_IWGRP) && !(pathStat.st_mode & S_IWOTH))
            {
                fileAttributes = kFileAttributeReadOnly;

                if (filename[0] == '.')
                    fileAttributes |= kFileAttributeHidden;
            }
            else if (filename[0] == '.')
                fileAttributes = kFileAttributeHidden;
            else
                fileAttributes = kFileAttributeNormal;
        }

        if (linkStat != NULL && S_ISLNK(linkStat->st_mode))
            fileAttributes |= kFileAttributeReparse_point;

        return (UnityPalFileAttributes)fileAttributes;
    }

    static int GetStatAndLinkStat(const std::string& path, struct stat& pathStat, struct stat& linkStat)
    {
        const int statResult = stat(path.c_str(), &pathStat);

        if (statResult == -1 && errno == ENOENT && lstat(path.c_str(), &pathStat) != 0) // Might be a dangling symlink...
            return PathErrnoToErrorCode(path, errno);

        if (lstat(path.c_str(), &linkStat) != 0)
            return PathErrnoToErrorCode(path, errno);

        return kErrorCodeSuccess;
    }

    static uint64_t TimeToTicks(time_t timeval)
    {
        return ((uint64_t)timeval * 10000000) + TIME_ZERO;
    }

    static time_t TicksToTime(uint64_t ticks)
    {
        return (ticks - TIME_ZERO) / 10000000;
    }

    static bool InternalCopyFile(int srcFd, int destFd, const struct stat& srcStat, int *error)
    {
        const blksize_t preferedBlockSize = srcStat.st_blksize;
        const blksize_t bufferSize = preferedBlockSize < 8192 ? 8192 : (preferedBlockSize > 65536 ? 65536 : preferedBlockSize);

        char *buffer = new char[bufferSize];

        ssize_t readBytes;

        while ((readBytes = read(srcFd, buffer, bufferSize)) > 0)
        {
            char* writeBuffer = buffer;
            ssize_t writeBytes = readBytes;

            while (writeBytes > 0)
            {
                const ssize_t writtenBytes = write(destFd, writeBuffer, writeBytes);

                if (writtenBytes < 0)
                {
                    if (errno == EINTR)
                        continue;

                    delete[] buffer;

                    *error = FileErrnoToErrorCode(errno);
                    return false;
                }

                writeBytes -= writtenBytes;
                writeBuffer += writtenBytes;
            }
        }

        delete[] buffer;

        if (readBytes < 0)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        IL2CPP_ASSERT(readBytes == 0);

        return true;
    }

    utils::Expected<bool> File::Isatty(FileHandle* fileHandle)
    {
        return isatty(fileHandle->fd) == 1;
    }

#if !IL2CPP_PLATFORM_OVERRIDES_STD_FILE_HANDLES
    FileHandle* File::GetStdError()
    {
        static FileHandle* s_handle = NULL;
        if (s_handle)
            return s_handle;

        s_handle = new FileHandle();
        s_handle->fd = 2;
        s_handle->type = kFileTypeChar;
        s_handle->options = 0;
        s_handle->accessMode = kFileAccessReadWrite;
        s_handle->shareMode = -1; // Only used for files

        return s_handle;
    }

    FileHandle* File::GetStdInput()
    {
        static FileHandle* s_handle = NULL;
        if (s_handle)
            return s_handle;

        s_handle = new FileHandle();
        s_handle->fd = 0;
        s_handle->type = kFileTypeChar;
        s_handle->options = 0;
        s_handle->accessMode = kFileAccessRead;
        s_handle->shareMode = -1; // Only used for files

        return s_handle;
    }

    FileHandle* File::GetStdOutput()
    {
        static FileHandle* s_handle = NULL;
        if (s_handle)
            return s_handle;

        s_handle = new FileHandle();
        s_handle->fd = 1;
        s_handle->type = kFileTypeChar;
        s_handle->options = 0;
        s_handle->accessMode = kFileAccessReadWrite;
        s_handle->shareMode = -1; // Only used for files

        return s_handle;
    }

#endif

    utils::Expected<bool> File::CreatePipe(FileHandle** read_handle, FileHandle** write_handle)
    {
        int error;
        return File::CreatePipe(read_handle, write_handle, &error);
    }

    utils::Expected<bool> File::CreatePipe(FileHandle** read_handle, FileHandle** write_handle, int* error)
    {
        int fds[2];

        const int ret = pipe(fds);

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        FileHandle *input = new FileHandle();
        input->fd = fds[0];
        input->type = kFileTypePipe;
        input->options = 0;
        input->accessMode = kFileAccessRead;
        input->shareMode = -1; // Only used for files

        FileHandle *output = new FileHandle();
        output->fd = fds[1];
        output->type = kFileTypePipe;
        output->options = 0;
        output->accessMode = kFileAccessReadWrite;
        output->shareMode = -1; // Only used for files

        *read_handle = input;
        *write_handle = output;

        return true;
    }

    UnityPalFileAttributes File::GetFileAttributes(const std::string& path, int *error)
    {
        struct stat pathStat, linkStat;

        *error = GetStatAndLinkStat(path, pathStat, linkStat);

        if (*error != kErrorCodeSuccess)
            return INVALID_FILE_ATTRIBUTES;

        return StatToFileAttribute(path, pathStat, &linkStat);
    }

    bool File::SetFileAttributes(const std::string& path, UnityPalFileAttributes attributes, int* error)
    {
        struct stat pathStat;

        int ret = stat(path.c_str(), &pathStat);

        if (ret != 0)
        {
            *error = PathErrnoToErrorCode(path, errno);
            return false;
        }

        if (attributes & kFileAttributeReadOnly)
            ret = chmod(path.c_str(), pathStat.st_mode & ~(S_IWUSR | S_IWOTH | S_IWGRP));
        else
            ret = chmod(path.c_str(), pathStat.st_mode | S_IWUSR);

        if (ret != 0)
        {
            *error = PathErrnoToErrorCode(path, errno);
            return false;
        }

        // Mono ignores all other attributes

        if (attributes & kFileAttributeInternalMonoExecutable)
        {
            mode_t exec_mask = 0;

            if ((pathStat.st_mode & S_IRUSR) != 0)
                exec_mask |= S_IXUSR;

            if ((pathStat.st_mode & S_IRGRP) != 0)
                exec_mask |= S_IXGRP;

            if ((pathStat.st_mode & S_IROTH) != 0)
                exec_mask |= S_IXOTH;

            ret = chmod(path.c_str(), pathStat.st_mode | exec_mask);

            if (ret != 0)
            {
                *error = PathErrnoToErrorCode(path, errno);
                return false;
            }
        }

        return true;
    }

    bool File::GetFileStat(const std::string& path, il2cpp::os::FileStat * stat, int* error)
    {
        struct stat pathStat, linkStat;

        *error = GetStatAndLinkStat(path, pathStat, linkStat);

        if (*error != kErrorCodeSuccess)
            return false;

        const std::string filename(il2cpp::utils::PathUtils::Basename(path));
        const time_t creationTime = pathStat.st_mtime < pathStat.st_ctime ? pathStat.st_mtime : pathStat.st_ctime;

        stat->name = filename;

        stat->attributes = StatToFileAttribute(path, pathStat, &linkStat);

        stat->length = (stat->attributes & kFileAttributeDirectory) > 0 ? 0 : pathStat.st_size;

        stat->creation_time = TimeToTicks(creationTime);
        stat->last_access_time = TimeToTicks(pathStat.st_atime);
        stat->last_write_time = TimeToTicks(pathStat.st_mtime);

        return true;
    }

    FileType File::GetFileType(FileHandle* handle)
    {
        return ((FileHandle*)handle)->type;
    }

    bool File::DeleteFile(const std::string& path, int *error)
    {
        const UnityPalFileAttributes attributes = GetFileAttributes(path, error);

        if (*error != kErrorCodeSuccess)
        {
            return false;
        }

        if (attributes & kFileAttributeReadOnly)
        {
            *error = kErrorCodeAccessDenied;
            return false;
        }

        const int ret = unlink(path.c_str());

        if (ret == -1)
        {
            *error = PathErrnoToErrorCode(path, errno);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool File::CopyFile(const std::string& src, const std::string& dest, bool overwrite, int* error)
    {
        const int srcFd = open(src.c_str(), O_RDONLY, 0);

        if (srcFd < 0)
        {
            *error = PathErrnoToErrorCode(src, errno);
            return false;
        }

        struct stat srcStat;

        if (fstat(srcFd, &srcStat) < 0)
        {
            *error = FileErrnoToErrorCode(errno);
            close(srcFd);
            return false;
        }

        int destFd;

        if (!overwrite)
        {
            destFd = open(dest.c_str(), O_WRONLY | O_CREAT | O_EXCL, srcStat.st_mode);
        }
        else
        {
            destFd = open(dest.c_str(), O_WRONLY | O_TRUNC, srcStat.st_mode);

            if (destFd < 0)
                destFd = open(dest.c_str(), O_WRONLY | O_CREAT | O_TRUNC, srcStat.st_mode);
            else
                *error = kErrorCodeAlreadyExists; // Apparently this error is set if we overwrite the dest file
        }

        if (destFd < 0)
        {
            *error = FileErrnoToErrorCode(errno);
            close(srcFd);
            return false;
        }

        const bool ret = InternalCopyFile(srcFd, destFd, srcStat, error);

        close(srcFd);
        close(destFd);

        return ret;
    }

    bool File::MoveFile(const std::string& src, const std::string& dest, int* error)
    {
        struct stat srcStat, destStat;

        if (stat(src.c_str(), &srcStat) < 0)
        {
            *error = PathErrnoToErrorCode(src.c_str(), errno);
            return false;
        }

        // In C# land we check for the existence of src, but not for dest.
        // We check it here and return the failure if dest exists and is not
        // the same file as src.
        if (stat(dest.c_str(), &destStat) == 0) // dest exists
        {
            if (destStat.st_dev != srcStat.st_dev || destStat.st_ino != srcStat.st_ino)
            {
                *error = kErrorCodeAlreadyExists;
                return false;
            }
        }

        if (!ShareAllowOpen(srcStat, kFileShareNone, kFileAccessWrite))
        {
            *error = kErrorCodeSuccess;
            return false;
        }

        const int ret = rename(src.c_str(), dest.c_str());

        if (ret == -1)
        {
            if (errno == EEXIST)
            {
                *error = kErrorCodeAlreadyExists;
                return false;
            }
            else if (errno == EXDEV)
            {
                if (S_ISDIR(srcStat.st_mode))
                {
                    *error = kErrorCodeNotSameDevice;
                    return false;
                }

                if (!CopyFile(src, dest, true, error))
                {
                    // CopyFile sets the error
                    return false;
                }

                return DeleteFile(src, error); // DeleteFile sets the error
            }
            else
            {
                *error = PathErrnoToErrorCode(src.c_str(), errno);
                return false;
            }
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool File::ReplaceFile(const std::string& sourceFileName, const std::string& destinationFileName, const std::string& destinationBackupFileName, bool ignoreMetadataErrors, int* error)
    {
        const bool backupFile = !destinationBackupFileName.empty();

        // Open the backup file for read so we can restore the file if an error occurs.
        const int backupFd = backupFile ? open(destinationBackupFileName.c_str(), O_RDONLY, 0) : -1;

        // dest -> backup
        if (backupFile)
        {
            const int retDest = rename(destinationFileName.c_str(), destinationBackupFileName.c_str());

            if (retDest == -1)
            {
                if (backupFd != -1)
                    close(backupFd);

                *error = PathErrnoToErrorCode(destinationFileName.c_str(), errno);
                return false;
            }
        }

        // source -> dest
        const int restSource = rename(sourceFileName.c_str(), destinationFileName.c_str());

        if (restSource == -1)
        {
            // backup -> dest
            if (backupFile)
                rename(destinationBackupFileName.c_str(), destinationFileName.c_str());

            // Copy backup data -> dest
            struct stat backupStat;
            if (backupFd != -1 && fstat(backupFd, &backupStat) == 0)
            {
                const int destFd = open(destinationBackupFileName.c_str(), O_WRONLY | O_CREAT | O_TRUNC, backupStat.st_mode);

                if (destFd != -1)
                {
                    int unusedCopyFileError;
                    InternalCopyFile(backupFd, destFd, backupStat, &unusedCopyFileError);
                    close(destFd);
                }
            }

            if (backupFd != -1)
                close(backupFd);

            *error = PathErrnoToErrorCode(sourceFileName.c_str(), errno);
            return false;
        }

        if (backupFd != -1)
            close(backupFd);

        *error = kErrorCodeSuccess;

        return true;
    }

    static int ConvertFlags(int fileaccess, int createmode)
    {
        int flags;

        switch (fileaccess)
        {
            case kFileAccessRead:
                flags = O_RDONLY;
                break;
            case kFileAccessWrite:
                flags = O_WRONLY;
                break;
            case kFileAccessReadWrite:
                flags = O_RDWR;
                break;
            default:
                flags = 0;
                break;
        }

        switch (createmode)
        {
            case kFileModeCreateNew:
                flags |= O_CREAT | O_EXCL;
                break;
            case kFileModeCreate:
                flags |= O_CREAT | O_TRUNC;
                break;
            case kFileModeOpen:
                break;
            case kFileModeOpenOrCreate:
            case kFileModeAppend:
                flags |= O_CREAT;
                break;
            case kFileModeTruncate:
                flags |= O_TRUNC;
                break;
            default:
                flags = 0;
                break;
        }

        return flags;
    }

#ifndef S_ISFIFO
#define S_ISFIFO(m) ((m & S_IFIFO) != 0)
#endif

    FileHandle* File::Open(const std::string& path, int mode, int accessMode, int shareMode, int options, int *error)
    {
        const int flags = ConvertFlags(accessMode, mode);

        /* we don't use sharemode, because that relates to sharing of
         * the file when the file is open and is already handled by
         * other code, perms instead are the on-disk permissions and
         * this is a sane default.
         */
        const mode_t perms = options & kFileOptionsTemporary ? 0600 : 0666;

        int fd = open(path.c_str(), flags, perms);

        /* If we were trying to open a directory with write permissions
         * (e.g. O_WRONLY or O_RDWR), this call will fail with
         * EISDIR. However, this is a bit bogus because calls to
         * manipulate the directory (e.g. SetFileTime) will still work on
         * the directory because they use other API calls
         * (e.g. utime()). Hence, if we failed with the EISDIR error, try
         * to open the directory again without write permission.
         */

        // Try again but don't try to make it writable
        if (fd == -1)
        {
            if (errno == EISDIR)
            {
                fd = open(path.c_str(), flags & ~(O_RDWR | O_WRONLY), perms);

                if (fd == -1)
                {
                    *error = PathErrnoToErrorCode(path, errno);
                    return INVALID_FILE_HANDLE;
                }
            }
            else
            {
                *error = PathErrnoToErrorCode(path, errno);
                return INVALID_FILE_HANDLE;
            }
        }

        struct stat statbuf;
        const int ret = fstat(fd, &statbuf);

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            close(fd);
            return INVALID_FILE_HANDLE;
        }

        if (!ShareAllowOpen(statbuf, shareMode, accessMode))
        {
            *error = kErrorCodeSharingViolation;
            close(fd);
            return INVALID_FILE_HANDLE;
        }

        FileHandle* fileHandle = new FileHandle();
        fileHandle->fd = fd;
        fileHandle->path = path;
        fileHandle->options = options;
        fileHandle->accessMode = accessMode;
        fileHandle->shareMode = shareMode;

        fileHandle->device = statbuf.st_dev;
        fileHandle->inode = statbuf.st_ino;

        // Add to linked list
        AddFileHandle(fileHandle);

#ifdef HAVE_POSIX_FADVISE
        if (options & kFileOptionsSequentialScan)
            posix_fadvise(fd, 0, 0, POSIX_FADV_SEQUENTIAL);
        if (options & kFileOptionsRandomAccess)
            posix_fadvise(fd, 0, 0, POSIX_FADV_RANDOM);
#endif

        if (S_ISFIFO(statbuf.st_mode))
            fileHandle->type = kFileTypePipe;
        else if (S_ISCHR(statbuf.st_mode))
            fileHandle->type = kFileTypeChar;
        else
            fileHandle->type = kFileTypeDisk;

        *error = kErrorCodeSuccess;

        return fileHandle;
    }

    bool File::Close(FileHandle* handle, int *error)
    {
        if (handle->type == kFileTypeDisk && handle->options & kFileOptionsDeleteOnClose)
            unlink(handle->path.c_str());

        close(handle->fd);

        // Remove from linked list
        RemoveFileHandle(handle);

        delete handle;

        *error = kErrorCodeSuccess;

        return true;
    }

    bool File::SetFileTime(FileHandle* handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, int* error)
    {
        if ((handle->accessMode & kFileAccessWrite) == 0)
        {
            *error = kErrorCodeAccessDenied;
            return false;
        }

        struct stat statbuf;
        const int ret = fstat(handle->fd, &statbuf);

        if (ret == -1)
        {
            *error = kErrorCodeInvalidParameter;
            return false;
        }

        struct utimbuf utbuf;

        // Setting creation time is not implemented in Mono and not supported by utime.

        if (last_access_time >= 0)
        {
            if (last_access_time < TIME_ZERO)
            {
                *error = kErrorCodeInvalidParameter;
                return false;
            }

            utbuf.actime = TicksToTime(last_access_time);
        }
        else
        {
            utbuf.actime = statbuf.st_atime;
        }

        if (last_write_time >= 0)
        {
            if (last_write_time < TIME_ZERO)
            {
                *error = kErrorCodeInvalidParameter;
                return false;
            }

            utbuf.modtime = TicksToTime(last_write_time);
        }
        else
        {
            utbuf.modtime = statbuf.st_mtime;
        }

        const int utimeRet = utime(handle->path.c_str(), &utbuf);

        if (utimeRet == -1)
        {
            *error = kErrorCodeInvalidParameter;
            return false;
        }

        *error = kErrorCodeSuccess;

        return true;
    }

    int64_t File::GetLength(FileHandle* handle, int *error)
    {
        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        struct stat statbuf;

        const int ret = fstat(handle->fd, &statbuf);

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return -1;
        }

        *error = kErrorCodeSuccess;

        return statbuf.st_size;
    }

    bool File::SetLength(FileHandle* handle, int64_t length, int *error)
    {
        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        // Save current position
        const off_t currentPosition = lseek(handle->fd, 0, SEEK_CUR);

        if (currentPosition == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        const off_t setLength = lseek(handle->fd, length, SEEK_SET);

        if (setLength == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        int ret = 0;

        do
        {
            ret = ftruncate(handle->fd, length);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        const off_t oldPosition = lseek(handle->fd, currentPosition, SEEK_SET);

        if (oldPosition == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        *error = kErrorCodeSuccess;

        return true;
    }

#if !IL2CPP_USE_GENERIC_FILE
    bool File::Truncate(FileHandle* handle, int *error)
    {
        off_t currentPosition = lseek(handle->fd, (off_t)0, SEEK_CUR);
        int32_t ret = 0;
        *error = kErrorCodeSuccess;

        if (currentPosition == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        do
        {
            ret = ftruncate(handle->fd, currentPosition);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        return true;
    }

#endif // IL2CPP_USE_GENERIC_FILE

    int64_t File::Seek(FileHandle* handle, int64_t offset, int origin, int *error)
    {
        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        int whence;

        switch (origin)
        {
            case kFileSeekOriginBegin:
                whence = SEEK_SET;
                break;
            case kFileSeekOriginCurrent:
                whence = SEEK_CUR;
                break;
            case kFileSeekOriginEnd:
                whence = SEEK_END;
                break;
            default:
            {
                *error = kErrorCodeInvalidParameter;
                return -1;
            }
        }

        const off_t position = lseek(handle->fd, offset, whence);

        if (position == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return -1;
        }

        *error = kErrorCodeSuccess;

        return position;
    }

    int File::Read(FileHandle* handle, char *dest, int count, int *error)
    {
        if (handle == NULL || handle == INVALID_FILE_HANDLE)
        {
            *error = kErrorCodeInvalidHandle;
            return 0;
        }

        if ((handle->accessMode & kFileAccessRead) == 0)
        {
            *error = kErrorCodeAccessDenied;
            return 0;
        }

        int ret;

        do
        {
            ret = (int)read(handle->fd, dest, count);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return 0;
        }

        return ret;
    }

    int32_t File::Write(FileHandle* handle, const char* buffer, int count, int *error)
    {
        if ((handle->accessMode & kFileAccessWrite) == 0)
        {
            *error = kErrorCodeAccessDenied;
            return -1;
        }

        int ret;

        do
        {
            ret = (int32_t)write(handle->fd, buffer, count);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return -1;
        }

#if IL2CPP_SUPPORTS_CONSOLE_EXTENSION
        if (handle == GetStdOutput() || handle == GetStdError())
            os::ConsoleExtension::Write(buffer);
#endif
        return ret;
    }

    bool File::Flush(FileHandle* handle, int* error)
    {
        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        const int ret = fsync(handle->fd);

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    void File::Lock(FileHandle* handle, int64_t position, int64_t length, int* error)
    {
        struct flock lock_data;
        int ret;

        lock_data.l_type = F_WRLCK;
        lock_data.l_whence = SEEK_SET;
        lock_data.l_start = position;
        lock_data.l_len = length;

        do
        {
            ret = fcntl(handle->fd, F_SETLK, &lock_data);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == -1)
        {
            /*
             * if locks are not available (NFS for example),
             * ignore the error
             */
            if (errno == ENOLCK
#ifdef EOPNOTSUPP
                || errno == EOPNOTSUPP
#endif
#ifdef ENOTSUP
                || errno == ENOTSUP
#endif
            )
            {
                *error = kErrorCodeSuccess;
                return;
            }

            *error = FileErrnoToErrorCode(errno);
            return;
        }

        *error = kErrorCodeSuccess;
    }

    void File::Unlock(FileHandle* handle, int64_t position, int64_t length, int* error)
    {
        struct flock lock_data;
        int ret;

        lock_data.l_type = F_UNLCK;
        lock_data.l_whence = SEEK_SET;
        lock_data.l_start = position;
        lock_data.l_len = length;

        do
        {
            ret = fcntl(handle->fd, F_SETLK, &lock_data);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == -1)
        {
            /*
             * if locks are not available (NFS for example),
             * ignore the error
             */
            if (errno == ENOLCK
#ifdef EOPNOTSUPP
                || errno == EOPNOTSUPP
#endif
#ifdef ENOTSUP
                || errno == ENOTSUP
#endif
            )
            {
                *error = kErrorCodeSuccess;
                return;
            }

            *error = FileErrnoToErrorCode(errno);
            return;
        }

        *error = kErrorCodeSuccess;
    }

    bool File::DuplicateHandle(FileHandle* source_process_handle, FileHandle* source_handle, FileHandle* target_process_handle,
        FileHandle** target_handle, int access, int inhert, int options, int* error)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(File::DuplicateHandle);
        return false;
    }

    utils::Expected<bool> File::IsExecutable(const std::string& path)
    {
#if IL2CPP_CAN_CHECK_EXECUTABLE
        return access(path.c_str(), X_OK) == 0;
#else
        return utils::Il2CppError(utils::NotSupported, "This platform cannot check for executable permissions.");
#endif
    }

    bool File::Cancel(FileHandle* handle)
    {
        return false;
    }
}
}

#endif
