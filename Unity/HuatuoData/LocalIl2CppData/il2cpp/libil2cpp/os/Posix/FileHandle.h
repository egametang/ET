#pragma once

#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX

#include <string>
#include <sys/stat.h>
#include <sys/types.h>
#include "os/File.h"
#include "os/c-api/OSGlobalEnums.h"

namespace il2cpp
{
namespace os
{
    struct FileHandle
    {
        int fd;
        FileType type;
        std::string path;
        int options;
        int shareMode;
        int accessMode;

        // The defaukt value of this field should be false,
        // meaning we _do_ own the file descriptor, and therefore
        // can close it. Zero-allocating this struct is something
        // we want to support, so make sure the default is 0.
        bool doesNotOwnFd;

        // device and inode are used as key for finding file handles
        dev_t device;
        ino_t inode;

        // Linked list of file handles
        FileHandle *prev;
        FileHandle *next;

        FileHandle() : prev(NULL), next(NULL)
        {
        }
    };
}
}

#endif
