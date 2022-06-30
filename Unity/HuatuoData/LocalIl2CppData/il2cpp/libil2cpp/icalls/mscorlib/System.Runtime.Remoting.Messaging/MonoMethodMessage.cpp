#include "il2cpp-config.h"
#include "il2cpp-api.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-tabledefs.h"
#include "icalls/mscorlib/System.Runtime.Remoting.Messaging/MonoMethodMessage.h"
#include "gc/WriteBarrier.h"
#include "os/Atomic.h"
#include "vm/Array.h"
#include "vm/Exception.h"
#include "vm/Method.h"
#include "vm/String.h"
#include "utils/dynamic_array.h"

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
    void MonoMethodMessage::InitMessage(Il2CppMethodMessage *this_obj, Il2CppReflectionMethod *method, Il2CppArray *out_args)
    {
        static Il2CppClass *object_array_klass;
        static Il2CppClass *byte_array_klass;
        static Il2CppClass *string_array_klass;
        Il2CppString *name;
        Il2CppArray *arr;
        int i, j;
        unsigned char arg_type;

        if (!object_array_klass)
        {
            Il2CppClass *klass;

            klass = il2cpp_array_class_get(il2cpp_defaults.byte_class, 1);
            IL2CPP_ASSERT(klass);
            byte_array_klass = klass;

            klass = il2cpp_array_class_get(il2cpp_defaults.string_class, 1);
            IL2CPP_ASSERT(klass);
            string_array_klass = klass;

            klass = il2cpp_array_class_get(il2cpp_defaults.object_class, 1);
            IL2CPP_ASSERT(klass);

            il2cpp::os::Atomic::ExchangePointer(&object_array_klass, klass);
        }

        IL2CPP_OBJECT_SETREF(this_obj, method, method);

        arr = il2cpp_array_new_specific(object_array_klass, method->method->parameters_count);

        IL2CPP_OBJECT_SETREF(this_obj, args, arr);

        arr = il2cpp_array_new_specific(byte_array_klass, method->method->parameters_count);

        IL2CPP_OBJECT_SETREF(this_obj, arg_types, arr);

        this_obj->async_result = NULL;
        this_obj->call_type = Il2Cpp_CallType_Sync;

        il2cpp::utils::dynamic_array<const char*> names(method->method->parameters_count);

        for (int i = 0; i < method->method->parameters_count; ++i)
            names[i] = vm::Method::GetParamName(method->method, i);

        arr = il2cpp_array_new_specific(string_array_klass, method->method->parameters_count);

        IL2CPP_OBJECT_SETREF(this_obj, names, arr);

        for (i = 0; i < method->method->parameters_count; i++)
        {
            name = il2cpp::vm::String::New(names[i]);
            il2cpp_array_setref(this_obj->names, i, name);
        }

        for (i = 0, j = 0; i < method->method->parameters_count; i++)
        {
            if (method->method->parameters[i]->byref)
            {
                if (out_args)
                {
                    Il2CppObject* arg = (Il2CppObject*)il2cpp_array_get(out_args, void*, j);
                    il2cpp_array_setref(this_obj->args, i, arg);
                    j++;
                }

                arg_type = 2;
                if (!(method->method->parameters[i]->attrs & PARAM_ATTRIBUTE_OUT))
                    arg_type |= 1;
            }
            else
            {
                arg_type = 1;
                if (method->method->parameters[i]->attrs & PARAM_ATTRIBUTE_OUT)
                    arg_type |= 4;
            }

            il2cpp_array_set(this_obj->arg_types, unsigned char, i, arg_type);
        }
    }
} /* namespace Messaging */
} /* namespace Remoting */
} /* namespace Runtime */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
