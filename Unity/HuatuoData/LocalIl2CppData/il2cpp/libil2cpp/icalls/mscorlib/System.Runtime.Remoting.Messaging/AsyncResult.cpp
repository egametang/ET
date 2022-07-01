#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-api.h"
#include "AsyncResult.h"
#include "vm/Runtime.h"
#include "vm/WaitHandle.h"
#include "vm/ThreadPoolMs.h"
#include "os/Event.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Runtime
{
namespace Remoting
{
namespace Messaging
{
    Il2CppObject* AsyncResult::Invoke(Il2CppObject* _this)
    {
#if IL2CPP_TINY
        IL2CPP_NOT_IMPLEMENTED_ICALL(AsyncResult::Invoke);
        return NULL;
#else
        Il2CppAsyncCall *ac;
        Il2CppObject *res;
        Il2CppAsyncResult *ares = (Il2CppAsyncResult*)_this;

        IL2CPP_ASSERT(ares);
        IL2CPP_ASSERT(ares->async_delegate);

        ac = (Il2CppAsyncCall*)ares->object_data;
        if (!ac)
        {
            res = vm::Runtime::DelegateInvoke(ares->async_delegate, (void**)&ares->async_state, NULL);
        }
        else
        {
            il2cpp::os::EventHandle *wait_event = NULL;

            IL2CPP_OBJECT_SETREF(ac->msg, exc, NULL);
            res = il2cpp::vm::ThreadPoolMs::MessageInvoke((Il2CppObject*)ares->async_delegate->target, ac->msg, &ac->msg->exc, &ac->out_args);
            IL2CPP_OBJECT_SETREF(ac, res, res);

            il2cpp_monitor_enter((Il2CppObject*)ares);
            ares->completed = 1;
            if (ares->handle)
                wait_event = (il2cpp::os::EventHandle*)il2cpp::vm::WaitHandle::GetPlatformHandle(ares->handle);

            il2cpp_monitor_exit((Il2CppObject*)ares);

            if (wait_event != NULL)
                wait_event->Get().Set();

            Il2CppException* completionException = NULL;

            if (ac->cb_method)
                vm::Runtime::Invoke(ac->cb_method, ac->cb_target, (void**)&ares, &completionException);

            if (completionException != NULL)
                vm::Exception::Raise(completionException);
        }

        return res;
#endif
    }
} // namespace Messaging
} // namespace Remoting
} // namespace Runtime
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
