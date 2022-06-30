#include "il2cpp-config.h"

#include "os/Error.h"
#include "os/ThreadLocalValue.h"

// Note: for now the implementation is not platform depentent.

namespace il2cpp
{
namespace os
{
    static ThreadLocalValue s_LastError;

    ErrorCode Error::GetLastError()
    {
        void* value = 0;

        s_LastError.GetValue(&value);

        return (ErrorCode)(int64_t)value;
    }

    void Error::SetLastError(ErrorCode code)
    {
        s_LastError.SetValue((void*)((int64_t)code));
    }
}
}
