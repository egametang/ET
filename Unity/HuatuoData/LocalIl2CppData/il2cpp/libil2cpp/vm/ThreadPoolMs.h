#pragma once


#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API ThreadPoolMs
    {
    public:
        static Il2CppAsyncResult* DelegateBeginInvoke(Il2CppDelegate* delegate, void** params, Il2CppDelegate* asyncCallback, Il2CppObject* state);
        static Il2CppObject* DelegateEndInvoke(Il2CppAsyncResult* asyncResult, void **out_args);
        static Il2CppObject* MessageInvoke(Il2CppObject *target, Il2CppMethodMessage *msg, Il2CppObject **exc, Il2CppArray **out_args);
        static void Suspend();
        static void Resume();
    };
} /* namespace vm */
} /* namespace il2cpp */
