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

    void LastError::SetLastError(uint32_t error)
    {
        IL2CPP_ASSERT(s_LastErrorThreadLocalStorageOffset != -1);

        uint32_t* lastErrorTls = (uint32_t*)Thread::GetThreadStaticData(s_LastErrorThreadLocalStorageOffset);
        *lastErrorTls = error;
    }

    void LastError::StoreLastError()
    {
        SetLastError(os::LastError::GetLastError());
    }

    void LastError::InitializeLastErrorThreadStatic()
    {
        if (s_LastErrorThreadLocalStorageOffset == -1)
            s_LastErrorThreadLocalStorageOffset = Thread::AllocThreadStaticData(sizeof(uint32_t));
    }
} /* namespace vm */
} /* namespace il2cpp */
