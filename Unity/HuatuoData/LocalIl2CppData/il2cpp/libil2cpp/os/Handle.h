#pragma once

#include <stdint.h>
#include <vector>
#include "utils/NonCopyable.h"
#include "os/WaitStatus.h"

namespace il2cpp
{
namespace os
{
    class Handle : public il2cpp::utils::NonCopyable
    {
    public:
        virtual ~Handle() {}
        virtual bool Wait() = 0;
        virtual bool Wait(uint32_t ms) = 0;
        virtual WaitStatus Wait(bool interruptible) = 0;
        virtual WaitStatus Wait(uint32_t ms, bool interruptible) = 0;
        virtual void Signal() = 0;
        virtual void* GetOSHandle() = 0;

        static int32_t WaitAny(const std::vector<Handle*>& handles, int32_t ms);
        static bool WaitAll(std::vector<Handle*>& handles, int32_t ms);
    private:
        static const int m_waitIntervalMs = 10;
    };
}
}
