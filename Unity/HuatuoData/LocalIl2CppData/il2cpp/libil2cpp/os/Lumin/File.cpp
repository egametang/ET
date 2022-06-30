#include "il2cpp-config.h"

#if IL2CPP_TARGET_LUMIN

#include "os/Lumin/File.h"
#include "os/Lumin/Lifecycle.h"
#include "os/Posix/FileHandle.h"
#include "utils/PathUtils.h"

#include <fcntl.h>
#include <sys/errno.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <string>


namespace il2cpp
{
namespace os
{
namespace lumin
{
    int s_StandardErrDescriptor = -1;
    int s_StandardOutDescriptor = -1;

    void InitializeFileHandles()
    {
        std::string base = GetPackageTempPath();
        std::string errPath = PathForErrorLog();
        std::string outPath = PathForOutputLog();

        s_StandardErrDescriptor = creat(errPath.c_str(), S_IRUSR | S_IWUSR);
        s_StandardOutDescriptor = creat(outPath.c_str(), S_IRUSR | S_IWUSR);
    }

    void CleanupFileHandles()
    {
        if (s_StandardErrDescriptor >= 0)
        {
            fdatasync(s_StandardErrDescriptor);
            close(s_StandardErrDescriptor);
        }
        if (s_StandardOutDescriptor >= 0)
        {
            fdatasync(s_StandardOutDescriptor);
            close(s_StandardOutDescriptor);
        }
    }

    std::string PathForErrorLog()
    {
        return il2cpp::utils::PathUtils::Combine(GetPackageTempPath(), std::string("stderr.log"));
    }

    std::string PathForOutputLog()
    {
        return il2cpp::utils::PathUtils::Combine(GetPackageTempPath(), std::string("stdout.log"));
    }
}

    FileHandle* File::GetStdError()
    {
        static FileHandle* s_handle = NULL;
        if (s_handle)
            return s_handle;

        s_handle = new FileHandle();
        s_handle->fd = lumin::s_StandardErrDescriptor;
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
        s_handle->fd = lumin::s_StandardOutDescriptor;
        s_handle->type = kFileTypeChar;
        s_handle->options = 0;
        s_handle->accessMode = kFileAccessReadWrite;
        s_handle->shareMode = -1; // Only used for files

        return s_handle;
    }
}
}

#endif
