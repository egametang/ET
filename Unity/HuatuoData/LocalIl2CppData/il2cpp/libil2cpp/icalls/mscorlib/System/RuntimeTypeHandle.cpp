#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "mono-structs.h"

#include "MonoType.h"
#include "RuntimeTypeHandle.h"
#include "Type.h"

#include "vm/Class.h"
#include "vm/Image.h"
#include "vm/Reflection.h"
#include "vm/MetadataCache.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    bool RuntimeTypeHandle::HasInstantiation(Il2CppReflectionRuntimeType* typeObject)
    {
        const Il2CppType* type = typeObject->type.type;

        if (type->byref)
            return false;

        Il2CppClass* klass = vm::Class::FromIl2CppType(type);

        return vm::Class::IsGeneric(klass) || vm::Class::IsInflated(klass);
    }

    bool RuntimeTypeHandle::HasReferences(Il2CppReflectionRuntimeType* type)
    {
        return vm::Class::FromIl2CppType(type->type.type)->has_references;
    }

    bool RuntimeTypeHandle::IsArray(Il2CppReflectionRuntimeType* type)
    {
        return Type::IsArrayImpl(&type->type);
    }

    bool RuntimeTypeHandle::IsByRef(Il2CppReflectionRuntimeType* type)
    {
        return MonoType::IsByRefImpl(&type->type);
    }

    bool RuntimeTypeHandle::IsComObject(Il2CppReflectionRuntimeType* type)
    {
        return false; // il2cpp does not support COM objects, so this is always false
    }

    bool RuntimeTypeHandle::IsGenericTypeDefinition(Il2CppReflectionRuntimeType* type)
    {
        return Type::get_IsGenericTypeDefinition(&type->type);
    }

    bool RuntimeTypeHandle::IsGenericVariable(Il2CppReflectionRuntimeType* type)
    {
        return MonoType::get_IsGenericParameter(&type->type);
    }

    bool RuntimeTypeHandle::IsInstanceOfType(Il2CppReflectionRuntimeType* type, Il2CppObject* o)
    {
        return Type::IsInstanceOfType(&type->type, o);
    }

    bool RuntimeTypeHandle::IsPointer(Il2CppReflectionRuntimeType* type)
    {
        return MonoType::IsPointerImpl(&type->type);
    }

    bool RuntimeTypeHandle::IsPrimitive(Il2CppReflectionRuntimeType* type)
    {
        return MonoType::IsPrimitiveImpl(&type->type);
    }

    bool RuntimeTypeHandle::type_is_assignable_from(Il2CppReflectionType* a, Il2CppReflectionType* b)
    {
        return Type::type_is_assignable_from(a, b);
    }

    int32_t RuntimeTypeHandle::GetArrayRank(Il2CppReflectionRuntimeType* type)
    {
        return MonoType::GetArrayRank(&type->type);
    }

    int32_t RuntimeTypeHandle::GetMetadataToken(Il2CppReflectionRuntimeType* type)
    {
        return vm::Class::FromSystemType(&type->type)->token;
    }

    Il2CppReflectionAssembly* RuntimeTypeHandle::GetAssembly(Il2CppReflectionRuntimeType* type)
    {
        return vm::Reflection::GetAssemblyObject(vm::Image::GetAssembly(vm::Class::GetImage(vm::Class::FromIl2CppType(type->type.type))));
    }

    Il2CppReflectionModule* RuntimeTypeHandle::GetModule(Il2CppReflectionRuntimeType* type)
    {
        return vm::Reflection::GetModuleObject(vm::Class::GetImage(vm::Class::FromIl2CppType(type->type.type)));
    }

    int32_t RuntimeTypeHandle::GetAttributes(Il2CppReflectionRuntimeType* type)
    {
        return MonoType::get_attributes(&type->type);
    }

    Il2CppReflectionRuntimeType* RuntimeTypeHandle::GetBaseType(Il2CppReflectionRuntimeType* type)
    {
        return reinterpret_cast<Il2CppReflectionRuntimeType*>(MonoType::get_BaseType(&type->type));
    }

    Il2CppReflectionRuntimeType* RuntimeTypeHandle::GetElementType(Il2CppReflectionRuntimeType* type)
    {
        return reinterpret_cast<Il2CppReflectionRuntimeType*>(MonoType::GetElementType(&type->type));
    }

    Il2CppReflectionType* RuntimeTypeHandle::GetGenericTypeDefinition_impl(Il2CppReflectionRuntimeType* type)
    {
        return Type::GetGenericTypeDefinition_impl(&type->type);
    }

    intptr_t RuntimeTypeHandle::GetGenericParameterInfo(Il2CppReflectionRuntimeType* type)
    {
        intptr_t retVal = 0;

        const Il2CppType *thisType = type->type.type;
        if ((thisType->type == IL2CPP_TYPE_VAR) || (thisType->type == IL2CPP_TYPE_MVAR))
        {
            Il2CppMetadataGenericParameterHandle param = il2cpp::vm::MetadataCache::GetGenericParameterFromType(thisType);
            if (param)
            {
                Il2CppGenericParameterInfo paramInfo = il2cpp::vm::MetadataCache::GetGenericParameterInfo(param);

                MonoGenericParameterInfo *monoParam = (MonoGenericParameterInfo*)il2cpp::vm::Reflection::GetMonoGenericParameterInfo(param);
                if (monoParam)
                {
                    retVal = reinterpret_cast<intptr_t>(monoParam);
                }
                else
                {
                    monoParam = (MonoGenericParameterInfo*)IL2CPP_MALLOC(sizeof(MonoGenericParameterInfo));
                    monoParam->flags = paramInfo.flags;
                    monoParam->token = paramInfo.num;
                    monoParam->name = paramInfo.name;
                    monoParam->pklass = NULL;
                    if (paramInfo.containerHandle)
                        monoParam->pklass = il2cpp::vm::MetadataCache::GetContainerDeclaringType(paramInfo.containerHandle);

                    int16_t constraintsCount = il2cpp::vm::MetadataCache::GetGenericConstraintCount(param);
                    monoParam->constraints = (Il2CppClass**)IL2CPP_MALLOC(sizeof(Il2CppClass*) * (constraintsCount + 1));
                    for (int i = 0; i < constraintsCount; ++i)
                    {
                        const Il2CppType *constraintType = il2cpp::vm::MetadataCache::GetGenericParameterConstraintFromIndex(param, i);
                        monoParam->constraints[i] = il2cpp::vm::Class::FromIl2CppType(constraintType);
                    }

                    monoParam->constraints[constraintsCount] = NULL;

                    il2cpp::vm::Reflection::SetMonoGenericParameterInfo(param, monoParam);
                    retVal = reinterpret_cast<intptr_t>(monoParam);
                }
            }
        }

        return retVal;
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
