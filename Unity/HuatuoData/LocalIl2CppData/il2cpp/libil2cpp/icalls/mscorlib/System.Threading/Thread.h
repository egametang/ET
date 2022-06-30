#pragma once

#include <stdint.h>
#include "il2cpp-object-internals.h"
#include "il2cpp-config.h"

struct Il2CppString;
struct Il2CppThread;
struct mscorlib_System_Globalization_CultureInfo;
struct Il2CppDelegate;
struct mscorlib_System_Threading_Thread;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Threading
{
    class LIBIL2CPP_CODEGEN_API Thread
    {
    public:
        static bool JoinInternal(Il2CppThread* thisPtr, int32_t millisecondsTimeout);
        static bool Thread_internal(Il2CppThread* thisPtr, Il2CppDelegate* start);
        static bool YieldInternal();
        static Il2CppArray* ByteArrayToCurrentDomain(Il2CppArray* arr);
        static Il2CppArray* ByteArrayToRootDomain(Il2CppArray* arr);
        static int32_t GetDomainID();
        static int32_t GetPriorityNative(Il2CppThread* thisPtr);
        static int32_t SystemMaxStackStize();
        static int8_t VolatileRead_1(volatile void* address);
        static double VolatileRead_Double(volatile double* address);
        static int16_t VolatileRead_2(volatile void* address);
        static int32_t VolatileRead_4(volatile void* address);
        static int64_t VolatileRead_8(volatile void* address);
        static intptr_t VolatileRead_IntPtr(volatile void* address);
        static Il2CppObject* VolatileRead_Object(volatile void* address);
        static float VolatileRead_Float(volatile float* address);
        static Il2CppObject* GetAbortExceptionState(Il2CppThread* thisPtr);
        static Il2CppString* GetName_internal(Il2CppInternalThread* thread);
        static int32_t GetState(Il2CppInternalThread* thread);
        static void Abort_internal(Il2CppInternalThread* thread, Il2CppObject* stateInfo);
        static void ClrState(Il2CppInternalThread* thread, int32_t clr);
        static void ConstructInternalThread(Il2CppThread* thisPtr);
        static void GetCurrentThread_icall(volatile Il2CppThread** thread);
        static void GetStackTraces(Il2CppArray** threads, Il2CppArray** stack_frames);
        static void InterruptInternal(Il2CppThread* thisPtr);
        static void MemoryBarrier();
        static void ResetAbortNative(Il2CppThread* thisPtr);
        static void ResumeInternal(Il2CppThread* thisPtr);
        static void SetName_icall(Il2CppInternalThread* thread, Il2CppChar* name, int32_t nameLength);
        static void SetPriorityNative(Il2CppThread* thisPtr, int32_t priority);
        static void SetState(Il2CppInternalThread* thread, int32_t set);
        static void SleepInternal(int32_t millisecondsTimeout);
        static void SpinWait_nop();
        static void SuspendInternal(Il2CppThread* thisPtr);
        static void VolatileWrite_1(volatile void* address, int8_t value);
        static void VolatileWrite_Double(volatile void* address, double value);
        static void VolatileWrite_Float(volatile void* address, float value);
        static void VolatileWrite_2(volatile void* address, int16_t value);
        static void VolatileWrite_4(volatile void* address, int32_t value);
        static void VolatileWrite_8(volatile void* address, int64_t value);
        static void VolatileWrite_IntPtr(volatile void* address, intptr_t value);
        static void VolatileWrite_Object(volatile void* address, Il2CppObject* value);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
