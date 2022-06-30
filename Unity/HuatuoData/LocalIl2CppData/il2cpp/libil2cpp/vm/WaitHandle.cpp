#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "vm/WaitHandle.h"

#include "vm/Class.h"
#include "vm/Field.h"
#include "vm/Object.h"
#include "vm/Runtime.h"

namespace il2cpp
{
namespace vm
{
    Il2CppWaitHandle* WaitHandle::NewManualResetEvent(bool initialState)
    {
        static const MethodInfo* constructor = NULL;
        if (!constructor)
            constructor = Class::GetMethodFromName(il2cpp_defaults.manualresetevent_class, ".ctor", 1);

        Il2CppObject* instance = Object::New(il2cpp_defaults.manualresetevent_class);
        void* args[1] = { &initialState };
        // NOTE: passing NULL here as Mono does, as long as the WaitHandle ctor will never throw an exception.
        Runtime::Invoke(constructor, instance, args, NULL);

        return reinterpret_cast<Il2CppWaitHandle*>(instance);
    }

    os::Handle* WaitHandle::GetPlatformHandle(Il2CppWaitHandle* waitHandle)
    {
        static FieldInfo *s_osHandle;
        static FieldInfo *s_safeHandle;

        if (!s_osHandle && !s_safeHandle)
        {
            s_osHandle = vm::Class::GetFieldFromName(il2cpp_defaults.manualresetevent_class, "Handle");
            s_safeHandle = vm::Class::GetFieldFromName(il2cpp_defaults.manualresetevent_class, "safeWaitHandle");
        }

        os::Handle* retval;
        if (s_osHandle)
        {
            intptr_t osHandle;
            vm::Field::GetValue((Il2CppObject*)waitHandle, s_osHandle, &osHandle);
            retval = reinterpret_cast<os::Handle*>(osHandle);
        }
        else
        {
            Il2CppSafeHandle *sh;
            vm::Field::GetValue((Il2CppObject*)waitHandle, s_safeHandle, &sh);
            retval = static_cast<os::Handle*>(sh->handle);
        }

        return static_cast<os::Handle*>(retval);
    }
} /* namespace vm */
} /* namespace il2cpp */
