#include "il2cpp-config.h"

#include "icalls/System/System.Net.Sockets/SocketException.h"

#include "os/Error.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace System
{
namespace Net
{
namespace Sockets
{
    int32_t SocketException::WSAGetLastError_icall()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(SocketException::WSAGetLastError, "Ignore this for now");

        return (int32_t)os::Error::GetLastError();
    }
} /* namespace Sockets */
} /* namespace Net */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
