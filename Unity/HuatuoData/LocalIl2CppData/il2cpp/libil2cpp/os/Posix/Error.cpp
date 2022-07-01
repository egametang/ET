#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX

#include "Error.h"
#include "utils/PathUtils.h"

#include <cassert>
#include <errno.h>
#include <unistd.h>

namespace il2cpp
{
namespace os
{
    ErrorCode SocketErrnoToErrorCode(int32_t code)
    {
        ErrorCode result = (ErrorCode) - 1;

        switch (code)
        {
            case 0: result = kErrorCodeSuccess; break;
            case EACCES: result = kWSAeacces; break;
    #ifdef EADDRINUSE
            case EADDRINUSE: result = kWSAeaddrinuse; break;
    #endif
    #ifdef EAFNOSUPPORT
            case EAFNOSUPPORT: result = kWSAeafnosupport; break;
    #endif
    #if EAGAIN != EWOULDBLOCK
            case EAGAIN: result = kWSAewouldblock; break;
    #endif
    #ifdef EALREADY
            case EALREADY: result = kWSAealready; break;
    #endif
            case EBADF: result = kWSAenotsock; break;
    #ifdef ECONNABORTED
            case ECONNABORTED: result = kWSAenetdown; break;
    #endif
    #ifdef ECONNREFUSED
            case ECONNREFUSED: result = kWSAeconnrefused; break;
    #endif
    #ifdef ECONNRESET
            case ECONNRESET: result = kWSAeconnreset; break;
    #endif
            case EFAULT: result = kWSAefault; break;
    #ifdef EHOSTUNREACH
            case EHOSTUNREACH: result = kWSAehostunreach; break;
    #endif
    #ifdef EINPROGRESS
            case EINPROGRESS: result = kWSAeinprogress; break;
    #endif
            case EINTR: result = kWSAeintr; break;
            case EINVAL: result = kWSAeinval; break;
                // FIXME: case EIO: result = WSAE????; break;
    #ifdef EISCONN
            case EISCONN: result = kWSAeisconn; break;
    #endif
            // FIXME: case ELOOP: result = WSA????; break;
            case EMFILE: result = kWSAemfile; break;
    #ifdef EMSGSIZE
            case EMSGSIZE: result = kWSAemsgsize; break;
    #endif
                // FIXME: case ENAMETOOLONG: result = kWSAeacces; break;
    #ifdef ENETUNREACH
            case ENETUNREACH: result = kWSAenetunreach; break;
    #endif
    #ifdef ENOBUFS
            case ENOBUFS: result = kWSAenobufs; break;
    #endif
            // case ENOENT: result = WSAE????; break;
            case ENOMEM: result = kWSAenobufs; break;
    #ifdef ENOPROTOOPT
            case ENOPROTOOPT: result = kWSAenoprotoopt; break;
    #endif
    #ifdef ENOSR
            case ENOSR: result = kWSAenetdown; break;
    #endif
    #ifdef ENOTCONN
            case ENOTCONN: result = kWSAenotconn; break;
    #endif
                // FIXME: case ENOTDIR: result = WSAE????; break;
    #ifdef ENOTSOCK
            case ENOTSOCK: result = kWSAenotsock; break;
    #endif
            case ENOTTY: result = kWSAenotsock; break;
    #ifdef EOPNOTSUPP
            case EOPNOTSUPP: result = kWSAeopnotsupp; break;
    #endif
            case EPERM: result = kWSAeacces; break;
            case EPIPE: result = kWSAeshutdown; break;
    #ifdef EPROTONOSUPPORT
            case EPROTONOSUPPORT: result = kWSAeprotonosupport; break;
    #endif
    #if ERESTARTSYS
            case ERESTARTSYS: result = kWSAenetdown; break;
    #endif
                // FIXME: case EROFS: result = WSAE????; break;
    #ifdef ESOCKTNOSUPPORT
            case ESOCKTNOSUPPORT: result = kWSAesocktnosupport; break;
    #endif
    #ifdef ETIMEDOUT
            case ETIMEDOUT: result = kWSAetimedout; break;
    #endif
    #ifdef EWOULDBLOCK
            case EWOULDBLOCK: result = kWSAewouldblock; break;
    #endif
    #ifdef EADDRNOTAVAIL
            case EADDRNOTAVAIL: result = kWSAeaddrnotavail; break;
    #endif
            case ENOENT: result = kWSAeconnrefused; break;
    #ifdef EDESTADDRREQ
            case EDESTADDRREQ: result = kWSAedestaddrreq; break;
    #endif
            case ENODEV: result = kWSAenetdown; break;
    #ifdef EHOSTDOWN
            case EHOSTDOWN: result = kWSAehostdown; break;
    #endif
    #ifdef ENXIO
            case ENXIO: result = kWSAhostNotFound; break;
    #endif
            default:
                result = kWSAsyscallfailure;
                break;
        }

        return result;
    }

    ErrorCode FileErrnoToErrorCode(int32_t code)
    {
        ErrorCode ret;
        /* mapping ideas borrowed from wine. they may need some work */

        switch (code)
        {
#if !RUNTIME_TINY
            case EACCES: case EPERM: case EROFS:
                ret = kErrorCodeAccessDenied;
                break;

            case EAGAIN:
                ret = kErrorCodeSharingViolation;
                break;

            case EBUSY:
                ret = kErrorCodeLockViolation;
                break;

            case EEXIST:
                ret = kErrorCodeFileExists;
                break;

            case EINVAL: case ESPIPE:
                ret = kErrorSeek;
                break;

            case EISDIR:
                ret = kErrorCodeCannotMake;
                break;

            case ENFILE: case EMFILE:
                ret = kErrorCodeTooManyOpenFiles;
                break;

            case ENOENT: case ENOTDIR:
                ret = kErrorCodeFileNotFound;
                break;

            case ENOSPC:
                ret = kErrorCodeHandleDiskFull;
                break;

            case ENOTEMPTY:
                ret = kErrorCodeDirNotEmpty;
                break;

            case ENOEXEC:
                ret = kErrorBadFormat;
                break;

            case ENAMETOOLONG:
                ret = kErrorCodeFileNameExcedRange;
                break;

#ifdef EINPROGRESS
            case EINPROGRESS:
                ret = kErrorIoPending;
                break;
#endif

            case ENOSYS:
                ret = kErrorNotSupported;
                break;

            case EBADF:
                ret = kErrorCodeInvalidHandle;
                break;

            case EIO:
                ret = kErrorCodeInvalidHandle;
                break;

            case EINTR:
                ret = kErrorIoPending;
                break;

            case EPIPE:
                ret = kErrorCodeWriteFault;
                break;
#endif

            default:
                ret = kErrorCodeGenFailure;
                break;
        }

        return ret;
    }

    ErrorCode PathErrnoToErrorCode(const std::string& path, int32_t code)
    {
        if (code == ENOENT)
        {
            const std::string dirname(il2cpp::utils::PathUtils::DirectoryName(path));
#if !IL2CPP_TARGET_PS4 && !IL2CPP_TARGET_PSP2  && !IL2CPP_HAS_NOACCESS
            if (access(dirname.c_str(), F_OK) == 0)
                return kErrorCodeFileNotFound;
            else
#endif
            return kErrorCodePathNotFound;
        }
        else
            return FileErrnoToErrorCode(code);
    }
}
}

#endif
