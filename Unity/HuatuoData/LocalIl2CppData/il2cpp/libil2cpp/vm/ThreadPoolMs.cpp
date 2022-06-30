#include "il2cpp-api.h"
#include "il2cpp-config.h"
#include "utils/dynamic_array.h"
#include "vm/ThreadPoolMs.h"
#include "vm/Domain.h"
#include "vm/Array.h"
#include "vm/Object.h"
#include "vm/Runtime.h"
#include "os/Atomic.h"
#include "gc/WriteBarrier.h"
#include "mono/ThreadPool/threadpool-ms.h"

namespace il2cpp
{
namespace vm
{
    Il2CppAsyncResult* ThreadPoolMs::DelegateBeginInvoke(Il2CppDelegate* delegate, void** params, Il2CppDelegate* asyncCallback, Il2CppObject* state)
    {
#if IL2CPP_TINY
        IL2CPP_ASSERT(0 && "ThreadPoolMs::DelegateBeginInvoke should not be called with the Tiny profile.");
        return NULL;
#else
        int numParams = delegate->method->parameters_count;
        il2cpp::utils::dynamic_array<void*> newParams(numParams + 2);
        for (int i = 0; i < numParams; ++i)
            newParams[i] = params[i];

        newParams[numParams] = asyncCallback;
        newParams[numParams + 1] = state;

        return threadpool_ms_begin_invoke(il2cpp::vm::Domain::GetCurrent(), (Il2CppObject*)delegate, const_cast<MethodInfo*>(delegate->method), newParams.data());
#endif
    }

    Il2CppObject* ThreadPoolMs::DelegateEndInvoke(Il2CppAsyncResult* asyncResult, void **out_args)
    {
#if IL2CPP_TINY
        IL2CPP_ASSERT(0 && "ThreadPoolMs::DelegateEndInvoke should not be called with the Tiny profile.");
        return NULL;
#else
        Il2CppArray *arrayOutArgs;
        Il2CppObject *exc, *retVal;

        retVal = threadpool_ms_end_invoke(asyncResult, &arrayOutArgs, &exc);

        if (exc)
            il2cpp_raise_exception((Il2CppException*)exc);

        if (out_args)
        {
            const MethodInfo *method = asyncResult->async_delegate->method;
            void** outArgsPtr = (void**)il2cpp_array_addr(arrayOutArgs, Il2CppObject*, 0);

            il2cpp_array_size_t arrayOutArgsIndex = 0;
            for (size_t methodParameterIndex = 0; methodParameterIndex < method->parameters_count; methodParameterIndex++)
            {
                const Il2CppType* paramType = method->parameters[methodParameterIndex];

                // Assume that arrayOutArgs only contains parameters that are passed by reference.
                if (!paramType->byref)
                    continue;
                IL2CPP_ASSERT(arrayOutArgsIndex < arrayOutArgs->max_length);
                Il2CppClass *paramClass = il2cpp_class_from_type(paramType);

                if (paramClass->byval_arg.valuetype)
                {
                    IL2CPP_ASSERT(paramClass->native_size > 0 && "EndInvoke: Invalid native_size found when trying to copy a value type in the out_args.");

                    // NOTE(gab): in case of value types, we need to copy the data over.
                    memcpy(out_args[arrayOutArgsIndex], il2cpp::vm::Object::Unbox((Il2CppObject*)outArgsPtr[arrayOutArgsIndex]), paramClass->native_size);
                }
                else
                {
                    *((void**)out_args[arrayOutArgsIndex]) = outArgsPtr[arrayOutArgsIndex];
                }
                arrayOutArgsIndex++;
            }
        }

        return retVal;
#endif
    }

    Il2CppObject* ThreadPoolMs::MessageInvoke(Il2CppObject *target, Il2CppMethodMessage *msg, Il2CppObject **exc, Il2CppArray **out_args)
    {
        static Il2CppClass *object_array_klass = NULL;
        MethodInfo *method;
        Il2CppObject *ret;
        Il2CppArray *arr;
        int i, j, outarg_count = 0;

        method = (MethodInfo*)msg->method->method;

        for (i = 0; i < method->parameters_count; i++)
        {
            if (method->parameters[i]->byref)
                outarg_count++;
        }

        if (!object_array_klass)
        {
            Il2CppClass *klass;

            klass = il2cpp_array_class_get(il2cpp_defaults.object_class, 1);
            IL2CPP_ASSERT(klass);

            os::Atomic::FullMemoryBarrier();
            object_array_klass = klass;
        }

        arr = il2cpp_array_new(object_array_klass, outarg_count);

        il2cpp::gc::WriteBarrier::GenericStore(out_args, arr);
        il2cpp::gc::WriteBarrier::GenericStoreNull(exc);

        ret = vm::Runtime::InvokeArray(method, method->klass->byval_arg.valuetype ? il2cpp_object_unbox(target) : target, method->parameters_count > 0 ? msg->args : NULL, (Il2CppException**)exc);

        for (i = 0, j = 0; i < method->parameters_count; i++)
        {
            if (method->parameters[i]->byref)
            {
                Il2CppObject* arg;
                arg = (Il2CppObject*)il2cpp_array_get(msg->args, void*, i);
                il2cpp_array_setref(*out_args, j, arg);
                j++;
            }
        }

        return ret;
    }

    void ThreadPoolMs::Suspend()
    {
        threadpool_ms_suspend();
    }

    void ThreadPoolMs::Resume()
    {
        threadpool_ms_resume();
    }
} /* namespace vm */
} /* namespace il2cpp */
