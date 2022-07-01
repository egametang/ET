#include "il2cpp-config.h"
#include <stddef.h>
#include <string>
#include "gc/WriteBarrier.h"
#include "icalls/mscorlib/System.Reflection/MonoMethod.h"
#include "il2cpp-tabledefs.h"
#include "il2cpp-class-internals.h"
#include "metadata/Il2CppTypeVector.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/GenericContainer.h"
#include "vm/MetadataCache.h"
#include "vm/Method.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Reflection.h"
#include "vm/Type.h"
#include "vm/GenericClass.h"

using il2cpp::metadata::Il2CppTypeVector;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Reflection
{
    Il2CppReflectionMethod* MonoMethod::get_base_definition(Il2CppReflectionMethod *m)
    {
        const MethodInfo *method = m->method;
        Il2CppClass *klass = method->klass;

        if (klass == NULL)
            return m;

        if (!(method->flags & METHOD_ATTRIBUTE_VIRTUAL) || vm::Class::IsInterface(klass) || method->flags & METHOD_ATTRIBUTE_NEW_SLOT)
            return m;

        /*if(klass->generic_class)
            klass = klass->generic_class->container_class;*/

        for (Il2CppClass* parent = klass->parent; parent != NULL; parent = parent->parent)
        {
            if (parent->vtable_count <= method->slot)
                break;
            klass = parent;
        }

        if (klass == method->klass)
            return m;

        il2cpp::vm::Class::Init(klass);

        const MethodInfo *result = klass->vtable[method->slot].method;

        if (result == NULL)
        {
            void *iterator = NULL;

            for (result = vm::Class::GetMethods(klass, &iterator); result != NULL; result = vm::Class::GetMethods(klass, &iterator))
                if (result->slot == method->slot)
                    break;
        }

        if (result == NULL)
            return m;

        return il2cpp::vm::Reflection::GetMethodObject(result, klass);
    }

    bool MonoMethod::get_IsGenericMethod(Il2CppReflectionMethod* method)
    {
        // if we are a generic method definition
        if (method->method->is_generic)
            return true;

        // is_inflated is true when a method is a generic instance or it's declaring type is a generic instance type.
        // Only return true here if we are a generic instance method
        if (method->method->is_inflated)
        {
            const Il2CppGenericContext* context = vm::MetadataCache::GetMethodGenericContext(method->method);
            return context != NULL && context->method_inst != NULL;
        }

        return false;
    }

    Il2CppString *  MonoMethod::get_name(Il2CppReflectionMethod * m)
    {
        const MethodInfo *method = m->method;

        IL2CPP_OBJECT_SETREF(m, name, vm::String::New(method->name));
        return m->name;
    }

    mscorlib_System_Runtime_InteropServices_DllImportAttribute * MonoMethod::GetDllImportAttribute(intptr_t)
    {
        //Todo: [DllImport] is an pseudo attribute. it doesn't exist in the metadata as an attribute, but as a flag on a method.
        //however, if you use reflection to ask for attributes, it does get reported, so what needs to happen is we create an attribute
        //instanec on the fly and populate it with the data in the metadata. Turns out that if you call GetCustomAttributes() that ends
        //up calling this function. For now, we will just return an attribute, but not yet populate it with the correct data.

        Il2CppClass* typeInfo = vm::Class::FromName(il2cpp_defaults.corlib, "System.Runtime.InteropServices", "DllImportAttribute");
        IL2CPP_ASSERT(typeInfo != NULL);

        return (mscorlib_System_Runtime_InteropServices_DllImportAttribute*)il2cpp::vm::Object::New(typeInfo);
    }

    Il2CppArray* MonoMethod::GetGenericArguments(Il2CppReflectionMethod* method)
    {
        uint32_t count = 0;
        Il2CppArray* res = NULL;
        const MethodInfo* methodInfo = method->method;
        if (methodInfo->is_inflated)
        {
            const Il2CppGenericContext* context = vm::MetadataCache::GetMethodGenericContext(methodInfo);
            if (context && context->method_inst)
            {
                const Il2CppGenericInst *inst = context->method_inst;
                count = inst->type_argc;
                res = vm::Array::New(il2cpp_defaults.systemtype_class, count);

                for (uint32_t i = 0; i < count; i++)
                    il2cpp_array_setref(res, i, il2cpp::vm::Reflection::GetTypeObject(inst->type_argv[i]));

                return res;
            }

            // method is inflated because it's owner is a generic instance type, extract method definition out of the method
            IL2CPP_ASSERT(methodInfo->is_generic || methodInfo->is_inflated);
            methodInfo = methodInfo->genericMethod->methodDefinition;
        }

        Il2CppMetadataGenericContainerHandle containerHandle = vm::MetadataCache::GetGenericContainerFromMethod(methodInfo->methodMetadataHandle);

        count = vm::MetadataCache::GetGenericContainerCount(containerHandle);
        res = vm::Array::New(il2cpp_defaults.systemtype_class, count);

        for (uint32_t i = 0; i < count; i++)
        {
            Il2CppMetadataGenericParameterHandle  param = vm::GenericContainer::GetGenericParameter(containerHandle, i);
            Il2CppClass *pklass = vm::Class::FromGenericParameter(param);
            il2cpp_array_setref(res, i, il2cpp::vm::Reflection::GetTypeObject(&pklass->byval_arg));
        }

        return res;
    }

    Il2CppObject * MonoMethod::InternalInvoke(Il2CppReflectionMethod * method, Il2CppObject * thisPtr, Il2CppArray * params, Il2CppException * * exc)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(MonoMethod::InternalInvoke, "Audit and look over commented code. Work in progress.");
/*
     * Invoke from reflection is supposed to always be a virtual call (the API
     * is stupid), mono_runtime_invoke_*() calls the provided method, allowing
     * greater flexibility.
     */
        const MethodInfo *m = method->method;
        int pcount;
        void *obj = thisPtr;

        *exc = NULL;

        if (!(m->flags & METHOD_ATTRIBUTE_STATIC))
        {
            if (thisPtr)
            {
                //if (!mono_class_vtable_full (mono_object_domain (method), m->klass, FALSE)) {
                //  mono_gc_wbarrier_generic_store (exc, (MonoObject*) mono_class_get_exception_for_failure (m->klass));
                //  return NULL;
                //}

                if (!vm::Object::IsInst(thisPtr, m->klass))
                {
                    IL2CPP_ASSERT(0);
                    //mono_gc_wbarrier_generic_store (exc, (MonoObject*) mono_exception_from_name_msg (mono_defaults.corlib, "System.Reflection", "TargetException", "Object does not match target type."));
                    return NULL;
                }

                m = vm::Object::GetVirtualMethod(thisPtr, m);
            }
            else
#if IL2CPP_ENABLE_MONO_BUG_EMULATION    // Mono doesn't throw on null 'this' if it's an instance constructor, and class libs depend on this behaviour
            if (strcmp(m->name, ".ctor"))
#endif
            {
                vm::Exception::Raise(vm::Exception::GetTargetException("Non-static method requires a target"));
            }
        }

        pcount = params ? il2cpp::vm::Array::GetLength(params) : 0;
        if (pcount != m->parameters_count)
        {
            IL2CPP_ASSERT(0);
            //mono_gc_wbarrier_generic_store (exc, (MonoObject*) mono_exception_from_name (mono_defaults.corlib, "System.Reflection", "TargetParameterCountException"));
            return NULL;
        }

        // TODO: Add check for abstract once types have flags
        //if ((m->klass->flags & TYPE_ATTRIBUTE_ABSTRACT) && !strcmp (m->name, ".ctor") && !this) {
        //  mono_gc_wbarrier_generic_store (exc, (MonoObject*) mono_exception_from_name_msg (mono_defaults.corlib, "System.Reflection", "TargetException", "Cannot invoke constructor of an abstract class."));
        //  return NULL;
        //}

        if (m->klass->rank && !strcmp(m->name, ".ctor"))
        {
            int i;
            il2cpp_array_size_t *lengths;
            il2cpp_array_size_t *lower_bounds;
            pcount = il2cpp::vm::Array::GetLength(params);
            lengths = (il2cpp_array_size_t*)alloca(sizeof(il2cpp_array_size_t) * pcount);
            for (i = 0; i < pcount; ++i)
                lengths[i] = *(il2cpp_array_size_t*)((char*)il2cpp_array_get(params, void*, i) + sizeof(Il2CppObject));

            if (m->klass->rank == pcount)
            {
                /* Only lengths provided. */
                lower_bounds = NULL;
            }
            else
            {
                IL2CPP_ASSERT(pcount == (m->klass->rank * 2));
                /* lower bounds are first. */
                lower_bounds = lengths;
                lengths += m->klass->rank;
            }

            return (Il2CppObject*)il2cpp::vm::Array::NewFull(m->klass, lengths, lower_bounds);
        }

        // If a managed exception was thrown, we need raise it here because Runtime::Invoke catches the exception and returns a pointer to it.
        Il2CppException* exception = NULL;

        Il2CppObject *result = il2cpp::vm::Runtime::InvokeArray(m, obj, params, &exception);

        if (exception)
        {
            *exc = exception;
            vm::Exception::Raise(exception);
        }

        return result;
    }

    bool MonoMethod::get_IsGenericMethodDefinition(Il2CppReflectionMethod* method)
    {
        return method->method->is_generic;
    }

    static std::string FormatExceptionMessageForNonConstructableGenericMethod(const MethodInfo* method, const Il2CppTypeVector& genericArguments)
    {
        std::string message;
        message += "Failed to construct generic method '";
        message += vm::Type::GetName(&method->klass->byval_arg, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
        message += "::";
        message += vm::Method::GetName(method);
        message += "' with generic arguments [";
        for (Il2CppTypeVector::const_iterator iter = genericArguments.begin(); iter != genericArguments.end(); ++iter)
        {
            if (iter != genericArguments.begin())
                message += ", ";
            message += vm::Type::GetName(*iter, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
        }
        message += "] at runtime.";

        return message;
    }

    static std::string FormatExceptionMessageForNonGenericMethod(const MethodInfo* method)
    {
        std::string message;
        message += "The method '";
        message += vm::Type::GetName(&method->klass->byval_arg, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
        message += "::";
        message += vm::Method::GetName(method);
        message += "' is not a generic method.";

        return message;
    }

    Il2CppReflectionMethod* MonoMethod::MakeGenericMethod_impl(Il2CppReflectionMethod* method, Il2CppArray* genericArgumentTypes)
    {
        const MethodInfo* genericMethodDefinition = method->method;

        if (!genericMethodDefinition->is_generic)
            vm::Exception::Raise(vm::Exception::GetInvalidOperationException(FormatExceptionMessageForNonGenericMethod(genericMethodDefinition).c_str()));

        uint32_t arrayLength = vm::Array::GetLength(genericArgumentTypes);
        Il2CppTypeVector genericArguments;
        genericArguments.reserve(arrayLength);

        for (uint32_t i = 0; i < arrayLength; i++)
        {
            Il2CppReflectionType* genericArgumentType = il2cpp_array_get(genericArgumentTypes, Il2CppReflectionType*, i);
            genericArguments.push_back(genericArgumentType->type);
        }

        const MethodInfo* genericInstanceMethod = vm::MetadataCache::GetGenericInstanceMethod(genericMethodDefinition, genericArguments);

        if (!genericInstanceMethod)
        {
            vm::Exception::Raise(vm::Exception::GetNotSupportedException(FormatExceptionMessageForNonConstructableGenericMethod(genericMethodDefinition, genericArguments).c_str()));
            return NULL;
        }

        return il2cpp::vm::Reflection::GetMethodObject(genericInstanceMethod, NULL);
    }

    Il2CppReflectionMethod* MonoMethod::GetGenericMethodDefinition_impl(Il2CppReflectionMethod* method)
    {
        if (method->method->is_generic)
            return method;

        if (!method->method->is_inflated)
            return NULL;

        const MethodInfo* methodDefinition = vm::MetadataCache::GetGenericMethodDefinition(method->method);
        IL2CPP_ASSERT(methodDefinition);

        if (!methodDefinition->is_generic)
            return NULL;


        const Il2CppGenericContext* methodContext = vm::MetadataCache::GetMethodGenericContext(method->method);
        IL2CPP_ASSERT(methodContext);

        if (methodContext->class_inst)
        {
            IL2CPP_NOT_IMPLEMENTED_ICALL(MonoMethod::GetGenericMethodDefinition_impl);
        }

        return il2cpp::vm::Reflection::GetMethodObject(const_cast<MethodInfo*>(methodDefinition), NULL);
    }

    int32_t MonoMethod::get_core_clr_security_level(Il2CppObject* _this)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(MonoMethod::get_core_clr_security_level);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    Il2CppReflectionMethod* MonoMethod::get_base_method(Il2CppReflectionMethod* method, bool definition)
    {
        // On the C# side, MonoMethod.GetBaseDefinition is the only caller of this icall that passes definition=true,
        // so we can do the equivalent call that net20 would do, which would be to call get_base_definition.
        //
        // When definition=false.  This is called by MonoMethod.GetBaseMethod, which is internal, and seems to
        // only be called by some GetCustomAttributes(true) calls.  There is only a small difference in the behavior of this method
        // when definition is false.
        const MethodInfo *method2 = method->method;
        Il2CppClass *klass = method2->klass;

        if (klass == NULL)
            return method;

        if (!(method2->flags & METHOD_ATTRIBUTE_VIRTUAL) || vm::Class::IsInterface(klass) || method2->flags & METHOD_ATTRIBUTE_NEW_SLOT)
            return method;

        /*if(klass->generic_class)
        klass = klass->generic_class->container_class;*/

        const MethodInfo *result;
        bool found = true;

        do
        {
            if (definition)
            {
                for (Il2CppClass* parent = klass->parent; parent != NULL; parent = parent->parent)
                {
                    if (parent->vtable_count <= method2->slot)
                        break;

                    klass = parent;
                }
            }
            else
            {
                if (!klass->parent)
                {
                    IL2CPP_ASSERT(klass == il2cpp_defaults.object_class);
                    return method;
                }

                klass = klass->parent;
            }

            if (klass == method2->klass)
                return method;

            il2cpp::vm::Class::Init(klass);

            result = klass->vtable[method2->slot].method;

            if (result == NULL)
            {
                void *iterator = NULL;
                found = false;

                for (result = vm::Class::GetMethods(klass, &iterator); result != NULL; result = vm::Class::GetMethods(klass, &iterator))
                {
                    if (result->slot == method2->slot)
                    {
                        found = true;
                        break;
                    }
                }

                IL2CPP_ASSERT(!(definition && !found));
            }
        }
        while (!found);

        return il2cpp::vm::Reflection::GetMethodObject(result, klass);
    }

    void MonoMethod::GetPInvoke(Il2CppReflectionMethod* _this, int32_t* flags, Il2CppString** entryPoint, Il2CppString** dllName)
    {
        // we don't keep these around in metadata
        *flags = 0;
        gc::WriteBarrier::GenericStore(dllName, vm::String::Empty());
        gc::WriteBarrier::GenericStore(entryPoint, vm::String::Empty());
    }
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
