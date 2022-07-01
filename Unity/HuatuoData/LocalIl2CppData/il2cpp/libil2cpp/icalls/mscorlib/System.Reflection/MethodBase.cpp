#include "il2cpp-config.h"
#include <stddef.h>
#include "icalls/mscorlib/System.Reflection/MethodBase.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-api.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/GenericClass.h"
#include "vm/Reflection.h"
#include "vm/StackTrace.h"
#include "vm/MetadataCache.h"
#include "metadata/GenericMetadata.h"

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
    static Il2CppClass* il2cpp_class_get_generic_type_definition(Il2CppClass *klass)
    {
        return klass->generic_class ? il2cpp::vm::GenericClass::GetTypeDefinition(klass->generic_class) : klass;
    }

    Il2CppReflectionMethod * MethodBase::GetMethodFromHandleInternalType(intptr_t method, intptr_t type)
    {
        Il2CppClass *klass = NULL;
        MethodInfo* methodInfo = (MethodInfo*)method;
        if (type)
        {
            klass = vm::Class::FromIl2CppType((Il2CppType*)type);
            if (il2cpp_class_get_generic_type_definition(methodInfo->klass) != il2cpp_class_get_generic_type_definition(klass))
                return NULL;

            // See the VerifyTwoArgumentGetMethodFromHandleWithGenericType for the failing test. Once we have support for inflating methods
            // we can implement this case as well and make that test pass.
            if (methodInfo->klass != klass)
                IL2CPP_NOT_IMPLEMENTED_ICALL(MethodBase::GetMethodFromHandleInternalType);
        }
        else
        {
            klass = methodInfo->klass;
        }
        return il2cpp::vm::Reflection::GetMethodObject(methodInfo, klass);
    }

    Il2CppReflectionMethod* MethodBase::GetCurrentMethod()
    {
        NOT_SUPPORTED_IL2CPP(MethodBase::GetCurrentMethod, "This icall is not supported by il2cpp. Use the il2cpp_codegen_get_method_object intrinsic instead.");

        return NULL;
    }

    void* /* System.Reflection.MethodBody */ MethodBase::GetMethodBodyInternal(intptr_t handle)
    {
        NOT_SUPPORTED_IL2CPP(MethodBase::GetMethodBodyInternal, "This icall is not supported by il2cpp.");

        return 0;
    }

    static const MethodInfo* il2cpp_method_get_equivalent_method(const MethodInfo *method, Il2CppClass *klass)
    {
        int offset = -1, i;

        if (method->is_inflated)
        {
            const Il2CppGenericContext *context = il2cpp::vm::MetadataCache::GetMethodGenericContext(method);
            if (context->method_inst)
            {
                const MethodInfo *result;

                //MonoMethodInflated *inflated = (MonoMethodInflated*)method;
                //method is inflated, we should inflate it on the other class
                Il2CppGenericContext newCtx;
                newCtx.method_inst = context->method_inst;
                newCtx.class_inst = context->class_inst;
                if (klass->generic_class)
                {
                    newCtx.class_inst = klass->generic_class->context.class_inst;
                }
                else if (klass->genericContainerHandle != NULL)
                {
                    IL2CPP_NOT_IMPLEMENTED(il2cpp_method_get_equivalent_method: generic_container_case);
                    //const Il2CppGenericContainer *genericContainer = il2cpp::vm::MetadataCache::GetGenericContainerFromIndex(klass->genericContainerIndex);
                    //newCtx.class_inst = genericContainer->context.class_inst;
                }

                result = il2cpp::metadata::GenericMetadata::Inflate(method, &newCtx);
                return result;
            }
        }

        il2cpp::vm::Class::SetupMethods(method->klass);

        for (i = 0; i < method->klass->method_count; ++i)
        {
            if (method->klass->methods[i] == method)
            {
                offset = i;
                break;
            }
        }

        il2cpp::vm::Class::SetupMethods(klass);

        IL2CPP_ASSERT(offset >= 0 && offset < klass->method_count);
        return klass->methods[offset];
    }

    Il2CppReflectionMethod* MethodBase::GetMethodFromHandleInternalType_native(intptr_t method_handle, intptr_t type_handle, bool genericCheck)
    {
        Il2CppReflectionMethod *res = NULL;
        Il2CppClass *klass;
        const MethodInfo *method = (const MethodInfo*)method_handle;
        if (type_handle && genericCheck)
        {
            klass = il2cpp_class_from_il2cpp_type((Il2CppType*)type_handle);
            if (il2cpp_class_get_generic_type_definition(method->klass) != il2cpp_class_get_generic_type_definition(klass))
                return NULL;

            if (method->klass != klass)
            {
                method = il2cpp_method_get_equivalent_method(method, klass);
                if (!method)
                    return NULL;
            }
        }
        else if (type_handle)
            klass = il2cpp_class_from_il2cpp_type((Il2CppType*)type_handle);
        else
            klass = method->klass;

        res = il2cpp_method_get_object(method, klass);
        return res;
    }
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
