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
        static int32_t GetDomainID();
        static Il2CppThread * CurrentThread_internal();
        static void ResetAbort_internal();
        static void MemoryBarrier_();
        static void SpinWait_nop();
        static void Abort_internal(Il2CppThread* thisPtr, Il2CppObject* stateInfo);
        static void ClrState(Il2CppThread* thisPtr, uint32_t clr);
        static void FreeLocalSlotValues(int32_t slot, bool use_thread_local);
        static Il2CppObject* GetAbortExceptionState(void* /* System.Threading.Thread */ self);
        static mscorlib_System_Globalization_CultureInfo * GetCachedCurrentCulture(Il2CppThread* thisPtr);
        static mscorlib_System_Globalization_CultureInfo* GetCachedCurrentUICulture(Il2CppThread* thisPtr);
        static Il2CppString* GetName_internal(Il2CppThread* thisPtr);
        static void SetName_internal(Il2CppThread* thisPtr, Il2CppString* name);
        static int32_t GetNewManagedId_internal();
        static uint32_t GetState(Il2CppThread * thisPtr);
        static void Interrupt_internal(Il2CppThread* thisPtr);
        static bool Join_internal(Il2CppThread * thisPtr, int32_t ms, void* thread);
        static void Resume_internal(void* /* System.Threading.Thread */ self);
        static void SetCachedCurrentCulture(Il2CppThread *thisPtr, Il2CppObject* culture);
        static void SetCachedCurrentUICulture(Il2CppThread* thisPtr, Il2CppObject* culture);
        static void SetState(Il2CppThread * thisPtr, uint32_t state);
        static void Sleep_internal(int32_t milliseconds);
        static void Suspend_internal(void* /* System.Threading.Thread */ self);
        static void Thread_init(Il2CppThread* thisPtr);
        static intptr_t Thread_internal(Il2CppThread* thisPtr, Il2CppDelegate * start);
        static int8_t VolatileReadInt8(volatile void* address);
        static int16_t VolatileReadInt16(volatile void* address);
        static int32_t VolatileReadInt32(volatile void* address);
        static int64_t VolatileReadInt64(volatile void* address);
        static float VolatileReadFloat(volatile void* address);
        static double VolatileReadDouble(volatile void* address);
        static void* VolatileReadPtr(volatile void* address);
        static intptr_t VolatileReadIntPtr(volatile void* address);
        static void VolatileWriteInt8(volatile void* address, int8_t value);
        static void VolatileWriteInt16(volatile void* address, int16_t value);
        static void VolatileWriteInt32(volatile void* address, int32_t value);
        static void VolatileWriteInt64(volatile void* address, int64_t value);
        static void VolatileWriteFloat(volatile void* address, float value);
        static void VolatileWriteDouble(volatile void* address, double value);
        static void VolatileWritePtr(volatile void* address, void* value);
        static void VolatileWriteIntPtr(volatile void* address, intptr_t value);

        static Il2CppArray* ByteArrayToCurrentDomain(Il2CppArray* arr);
        static Il2CppArray* ByteArrayToRootDomain(Il2CppArray* arr);
        static bool YieldInternal();
        static bool JoinInternal(Il2CppThread* _this, int32_t millisecondsTimeout);
        static int32_t GetPriorityNative(Il2CppThread* _this);
        static int32_t SystemMaxStackStize();
        static Il2CppString* GetName_internal40(Il2CppInternalThread* thread);
        static Il2CppInternalThread* CurrentInternalThread_internal();
        static int32_t GetState40(Il2CppInternalThread* thread);
        static void Abort_internal40(Il2CppInternalThread* thread, Il2CppObject* stateInfo);
        static void ClrState40(Il2CppInternalThread* thread, uint32_t clr);
        static void ConstructInternalThread(Il2CppThread* _this);
        static void GetStackTraces(Il2CppArray** threads, Il2CppArray** stack_frames);
        static void InterruptInternal(Il2CppThread* _this);
        static void ResetAbortNative(Il2CppObject* _this);
        static void ResumeInternal(Il2CppObject* _this);
        static void SetName_internal40(Il2CppInternalThread* thread, Il2CppString* name);
        static void SetPriorityNative(Il2CppThread* _this, int32_t priority);
        static void SetState40(Il2CppInternalThread* thread, uint32_t set);
        static void SleepInternal(int32_t millisecondsTimeout);
        static void SuspendInternal(Il2CppObject* _this);

        static Il2CppThread* GetCurrentThread();
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
