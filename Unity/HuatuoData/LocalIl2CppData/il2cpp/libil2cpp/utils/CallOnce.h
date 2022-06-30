#pragma once

#include "NonCopyable.h"
#include "../os/Mutex.h"

#include "Baselib.h"
#include "Cpp/Atomic.h"
#include "Cpp/ReentrantLock.h"

namespace il2cpp
{
namespace utils
{
    typedef void (*CallOnceFunc) (void* arg);

    struct OnceFlag : NonCopyable
    {
        OnceFlag() : m_IsSet(false)
        {
        }

        friend void CallOnce(OnceFlag& flag, CallOnceFunc func, void* arg);

        bool IsSet()
        {
            return m_IsSet;
        }

    private:
        baselib::atomic<bool> m_IsSet;
        baselib::ReentrantLock m_Mutex;
    };

    inline void CallOnce(OnceFlag& flag, CallOnceFunc func, void* arg)
    {
        if (!flag.m_IsSet)
        {
            os::FastAutoLock lock(&flag.m_Mutex);
            if (!flag.m_IsSet)
            {
                func(arg);
                flag.m_IsSet = true;
            }
        }
    }
}
}
