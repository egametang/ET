#include "LastError.h"
#include "Thread.h"
#include "os/LastError.h"

namespace il2cpp
{
namespace vm
{
    int32_t LastError::s_LastErrorThreadLocalStorageOffset = -1;

    uint32_t LastError::GetLastError()
    {
        if (s_LastErrorThreadLocalStorageOffset == -1)
            return 0;

        return *(uint32_t*)Thread::GetThreadStaticData(s_LastErrorThreadLocalStorageOffset);
    }

    void LastError::StoreLastError()
    {
        // Get the last error first, before any other calls (so that we don't stomp on it).
        uint32_t lastError = os::LastError::GetLastError();

        uint32_t* lastErrorTls = (uint32_t*)Thread::GetThreadStaticData(s_LastErrorThreadLocalStorageOffset);
        *lastErrorTls = lastError;
    }

    void LastError::InitializeLastErrorThreadStatic()
    {
        if (s_LastErrorThreadLocalStorageOffset == -1)
            s_LastErrorThreadLocalStorageOffset = Thread::AllocThreadStaticData(sizeof(uint32_t));
    }
} /* namespace vm */
} /* namespace il2cpp */
