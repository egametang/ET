#pragma once
#include "il2cpp-config.h"
struct Il2CppObject;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Monitor
    {
    public:
        static void Enter(Il2CppObject* object);
        static bool TryEnter(Il2CppObject* object, uint32_t timeout);
        static void Exit(Il2CppObject* object);
        static void Pulse(Il2CppObject* object);
        static void PulseAll(Il2CppObject* object);
        static void Wait(Il2CppObject* object);
        static bool TryWait(Il2CppObject* object, uint32_t timeout);
        static bool IsAcquired(Il2CppObject* object);
    };

#if !IL2CPP_SUPPORT_THREADS

    inline void Monitor::Enter(Il2CppObject* object)
    {
    }

    inline bool Monitor::TryEnter(Il2CppObject* object, uint32_t timeout)
    {
        return true;
    }

    inline void Monitor::Exit(Il2CppObject* object)
    {
    }

    inline void Monitor::Pulse(Il2CppObject* object)
    {
    }

    inline void Monitor::PulseAll(Il2CppObject* object)
    {
    }

    inline void Monitor::Wait(Il2CppObject* object)
    {
    }

    inline bool Monitor::TryWait(Il2CppObject* object, uint32_t timeout)
    {
        return true;
    }

    inline bool Monitor::IsAcquired(Il2CppObject* object)
    {
        return true;
    }

#endif

    struct MonitorHolder
    {
        MonitorHolder(Il2CppObject* obj) :
            m_Object(obj)
        {
            Monitor::Enter(obj);
        }

        ~MonitorHolder()
        {
            Monitor::Exit(m_Object);
        }

    private:
        Il2CppObject* m_Object;
    };
} /* namespace vm */
} /* namespace il2cpp */
