#include "il2cpp-config.h"
#include "gc/GarbageCollector.h"
#include "Interlocked.h"
#include <ctype.h>
#include "os/Atomic.h"
#include "os/Mutex.h"
#include "vm/Exception.h"

#if !IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
#include "Baselib.h"
#include "Cpp/ReentrantLock.h"
#endif

union LongDoubleUnion
{
    int64_t l_val;
    double d_val;
};

union IntFloatUnion
{
    int32_t i_val;
    float f_val;
};

#if RUNTIME_TINY
namespace tiny
#else
namespace il2cpp
#endif
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Threading
{
#if !IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
    static baselib::ReentrantLock m_Atomic64Mutex;
#endif

    void* Interlocked::CompareExchange_T(void** location, void* value, void* comparand)
    {
        void* retval = il2cpp::os::Atomic::CompareExchangePointer(location, value, comparand);
        il2cpp::gc::GarbageCollector::SetWriteBarrier(location);
        return retval;
    }

    void Interlocked::CompareExchangeObject(void** location, void** value, void** comparand, void** res)
    {
        *res = il2cpp::os::Atomic::CompareExchangePointer(location, *value, *comparand);
        il2cpp::gc::GarbageCollector::SetWriteBarrier(location);
    }

    intptr_t Interlocked::CompareExchangeIntPtr(intptr_t* location, intptr_t value, intptr_t comparand)
    {
        return reinterpret_cast<intptr_t>(il2cpp::os::Atomic::CompareExchangePointer(reinterpret_cast<void**>(location), reinterpret_cast<void*>(value), reinterpret_cast<void*>(comparand)));
    }

    int32_t Interlocked::CompareExchange(int32_t* location, int32_t value, int32_t comparand)
    {
        return il2cpp::os::Atomic::CompareExchange(location, value, comparand);
    }

    int64_t Interlocked::CompareExchange64(int64_t* location, int64_t value, int64_t comparand)
    {
#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
        return il2cpp::os::Atomic::CompareExchange64(location, value, comparand);
#else
        il2cpp::os::FastAutoLock lock(&m_Atomic64Mutex);
        int64_t orig = *location;
        if (*location == comparand)
            *location = value;

        return orig;
#endif
    }

    int32_t Interlocked::Add(int32_t* location1, int32_t value)
    {
        return il2cpp::os::Atomic::Add(location1, value);
    }

    int64_t Interlocked::Add64(int64_t* location1, int64_t value)
    {
#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
        return il2cpp::os::Atomic::Add64(location1, value);
#else
        il2cpp::os::FastAutoLock lock(&m_Atomic64Mutex);
        return *location1 += value;
#endif
    }

    double Interlocked::CompareExchangeDouble(double* location1, double value, double comparand)
    {
        LongDoubleUnion val, ret, cmp;

        cmp.d_val = comparand;
        val.d_val = value;
        ret.l_val = (int64_t)il2cpp::os::Atomic::CompareExchange64((int64_t*)location1, val.l_val, cmp.l_val);

        return ret.d_val;
    }

    float Interlocked::CompareExchangeSingle(float* location1, float value, float comparand)
    {
        IntFloatUnion val, ret, cmp;

        cmp.f_val = comparand;
        val.f_val = value;
        ret.i_val = (int32_t)il2cpp::os::Atomic::CompareExchange((int32_t*)location1, val.i_val, cmp.i_val);

        return ret.f_val;
    }

    int32_t Interlocked::Increment(int32_t* value)
    {
        return il2cpp::os::Atomic::Increment(value);
    }

    int64_t Interlocked::Increment64(int64_t* location)
    {
#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
        return il2cpp::os::Atomic::Increment64(location);
#else
        il2cpp::os::FastAutoLock lock(&m_Atomic64Mutex);
        return ++(*location);
#endif
    }

    int32_t Interlocked::Decrement(int32_t* location)
    {
        return il2cpp::os::Atomic::Decrement(location);
    }

    int64_t Interlocked::Decrement64(int64_t* location)
    {
#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
        return il2cpp::os::Atomic::Decrement64(location);
#else
        il2cpp::os::FastAutoLock lock(&m_Atomic64Mutex);
        return --(*location);
#endif
    }

    double Interlocked::ExchangeDouble(double* location1, double value)
    {
#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
        LongDoubleUnion val, ret;

        val.d_val = value;
        ret.l_val = (int64_t)il2cpp::os::Atomic::Exchange64((int64_t*)location1, val.l_val);

        return ret.d_val;
#else
        il2cpp::os::FastAutoLock lock(&m_Atomic64Mutex);
        double orig = *location1;
        *location1 = value;
        return orig;
#endif
    }

    intptr_t Interlocked::ExchangeIntPtr(intptr_t* location, intptr_t value)
    {
        return reinterpret_cast<intptr_t>(il2cpp::os::Atomic::ExchangePointer(reinterpret_cast<void**>(location), reinterpret_cast<void*>(value)));
    }

    int32_t Interlocked::Exchange(int32_t* location1, int32_t value)
    {
        return il2cpp::os::Atomic::Exchange(location1, value);
    }

    int64_t Interlocked::Exchange64(int64_t* location1, int64_t value)
    {
#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
        return il2cpp::os::Atomic::Exchange64(location1, value);
#else
        il2cpp::os::FastAutoLock lock(&m_Atomic64Mutex);
        int64_t orig = *location1;
        *location1 = value;
        return orig;
#endif
    }

    void* Interlocked::ExchangePointer(void** location1, void* value)
    {
        void* retval = il2cpp::os::Atomic::ExchangePointer(location1, value);
        il2cpp::gc::GarbageCollector::SetWriteBarrier(location1);
        return retval;
    }

    void Interlocked::ExchangeObject(void** location1, void** value, void** res)
    {
        *res = il2cpp::os::Atomic::ExchangePointer(location1, *value);
        il2cpp::gc::GarbageCollector::SetWriteBarrier(location1);
    }

    float Interlocked::ExchangeSingle(float* location1, float value)
    {
        IntFloatUnion val, ret;

        val.f_val = value;
        ret.i_val = (int32_t)il2cpp::os::Atomic::Exchange((int32_t*)location1, val.i_val);

        return ret.f_val;
    }

    int64_t Interlocked::Read(int64_t* location)
    {
#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
        return il2cpp::os::Atomic::Read64(location);
#else
        il2cpp::os::FastAutoLock lock(&m_Atomic64Mutex);
        return *location;
#endif
    }

    int32_t Interlocked::CompareExchange(int32_t* location1, int32_t value, int32_t comparand, bool* succeeded)
    {
        int32_t result = CompareExchange(location1, value, comparand);
        *succeeded = result == comparand;
        return result;
    }

    void Interlocked::MemoryBarrierProcessWide()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Interlocked::MemoryBarrierProcessWide);
        IL2CPP_UNREACHABLE;
    }
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace tiny */
