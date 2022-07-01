#include "il2cpp-config.h"
#include <vector>
#include <algorithm>
#include "icalls/mscorlib/System.Threading/WaitHandle.h"
#include "os/Mutex.h"
#include "os/Thread.h"
#include "vm/Thread.h"
#include "vm/Exception.h"
#include "vm/Array.h"
#include "vm/WaitHandle.h"

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
    std::vector<os::Handle*> ExtractOsWaitHandles(Il2CppArray* handles)
    {
        uint32_t numberOfHandles = vm::Array::GetLength(handles);

        std::vector<os::Handle*> osWaitHandles(numberOfHandles);

        for (uint32_t i = 0; i < numberOfHandles; ++i)
        {
            Il2CppObject* handle = il2cpp_array_get(handles, Il2CppObject*, i);
            Il2CppWaitHandle* waitHandle = (Il2CppWaitHandle*)handle;
            osWaitHandles[i] = vm::WaitHandle::GetPlatformHandle(waitHandle);
        }

        return osWaitHandles;
    }

    int32_t WaitHandle::WaitAny_internal(Il2CppArray* handles, int32_t ms, bool exitContext)
    {
        std::vector<os::Handle*> osWaitHandles = ExtractOsWaitHandles(handles);

        vm::ThreadStateSetter state(vm::kThreadStateWaitSleepJoin);

        return os::Handle::WaitAny(osWaitHandles, ms);
    }

    bool WaitHandle::WaitOne_internal(Il2CppObject* unused, intptr_t handlePtr, int32_t ms, bool exitContext)
    {
        il2cpp::os::Handle* handle = (il2cpp::os::Handle*)handlePtr;

        vm::ThreadStateSetter state(vm::kThreadStateWaitSleepJoin);

        // Check for pending interrupt before we go waiting.
        il2cpp::vm::Thread::CheckCurrentThreadForInterruptAndThrowIfNecessary();

        if (ms == -1)
            return handle->Wait();

        return handle->Wait((uint32_t)ms);
    }

    bool WaitHandle::WaitAll_internal(Il2CppArray* handles, int32_t ms, bool exitContext)
    {
        std::vector<os::Handle*> osWaitHandles = ExtractOsWaitHandles(handles);

        vm::ThreadStateSetter state(vm::kThreadStateWaitSleepJoin);

        return os::Handle::WaitAll(osWaitHandles, ms);
    }

    bool WaitHandle::SignalAndWait_Internal(intptr_t toSignal, intptr_t toWaitOn, int32_t ms, bool exitContext)
    {
        os::Handle* toSignalOsHandle = (os::Handle*)toSignal;
        toSignalOsHandle->Signal();

        return WaitOne_internal(NULL, toWaitOn, ms, exitContext);
    }

    int32_t WaitHandle::SignalAndWait_Internal40(intptr_t toSignal, intptr_t toWaitOn, int32_t ms)
    {
        return SignalAndWait_Internal(toSignal, toWaitOn, ms, false) ? 0 : 1;
    }

#define MANAGED_WAIT_FAILED 0x7fffffff
#define MANAGED_WAIT_OBJECT_0 0x00000000
#define MANAGED_WAIT_TIMEOUT 258
#define MANAGED_WAIT_ABANDONED_0 0x00000080L

    static int32_t map_native_wait_result_to_managed(UnityPalWaitStatus val, int32_t numobjects)
    {
        switch (val)
        {
            case kWaitStatusSuccess:
                return MANAGED_WAIT_OBJECT_0 + val;
            case  kWaitStatusAbandoned:
                return MANAGED_WAIT_ABANDONED_0 + val;
            case  kWaitStatusFailure:
                return MANAGED_WAIT_FAILED;
            case  kWaitStatusTimeout:
                return MANAGED_WAIT_TIMEOUT;
            default:
                return MANAGED_WAIT_FAILED;
        }
    }

    int32_t WaitHandle::Wait_internal(void* *handles, int32_t numhandles, bool  waitall, int32_t timeouts)
    {
        vm::ThreadStateSetter state(vm::kThreadStateWaitSleepJoin);

        if (numhandles == 1)
        {
            os::WaitStatus status = ((os::Handle**)handles)[0]->Wait((uint32_t)timeouts, true);

            // The managed code expects either success or timeout from this function, so
            // throw away any other error codes.
            if (status != kWaitStatusSuccess)
                status = kWaitStatusTimeout;
            return map_native_wait_result_to_managed(status, 1);
        }

        std::vector<os::Handle*> osWaitHandles((os::Handle**)handles, (os::Handle**)handles + numhandles);

        if (waitall)
        {
            if (os::Handle::WaitAll(osWaitHandles, timeouts))
            {
                return map_native_wait_result_to_managed(kWaitStatusSuccess, numhandles);
            }
            else
            {
                return map_native_wait_result_to_managed(kWaitStatusTimeout, numhandles);
            }
        }
        else
            return os::Handle::WaitAny(osWaitHandles, timeouts);
    }
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
