#include "il2cpp-config.h"
#include "vm/Method.h"
#include "il2cpp-tabledefs.h"
#include "il2cpp-class-internals.h"
#include "vm/Class.h"
#include "vm/MetadataCache.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/Type.h"

namespace il2cpp
{
namespace vm
{
    const Il2CppType* Method::GetReturnType(const MethodInfo* method)
    {
        return method->return_type;
    }

    Il2CppClass *Method::GetDeclaringType(const MethodInfo* method)
    {
        return method->klass;
    }

    const char* Method::GetName(const MethodInfo *method)
    {
        return method->name;
    }

    std::string Method::GetNameWithGenericTypes(const MethodInfo* method)
    {
        std::string str;

        str += method->name;

        if (method->is_inflated && method->genericMethod->context.method_inst)
        {
            const Il2CppGenericInst *inst = method->genericMethod->context.method_inst;

            str += '<';

            for (unsigned int i = 0; i < inst->type_argc; ++i)
            {
                str += Type::GetName(inst->type_argv[i], IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
                if (i < inst->type_argc - 1)
                    str += ",";
            }

            str += '>';
        }

        return str;
    }

    bool Method::IsGeneric(const MethodInfo *method)
    {
        return method->is_generic;
    }

    bool Method::IsInflated(const MethodInfo *method)
    {
        return method->is_inflated;
    }

    bool Method::IsGenericInstance(const MethodInfo *method)
    {
        return method->is_inflated && !method->is_generic;
    }

    bool Method::IsInstance(const MethodInfo *method)
    {
        return !(method->flags & METHOD_ATTRIBUTE_STATIC);
    }

    uint32_t Method::GetParamCount(const MethodInfo *method)
    {
        return method->parameters_count;
    }

    uint32_t Method::GetGenericParamCount(const MethodInfo *method)
    {
        if (IsGeneric(method) && method->genericContainerHandle != NULL)
            return MetadataCache::GetGenericContainerCount(method->genericContainerHandle);
        return 0;
    }

    const Il2CppType* Method::GetParam(const MethodInfo *method, uint32_t index)
    {
        if (index < method->parameters_count)
            return method->parameters[index].parameter_type;
        else
            return NULL;
    }

    const char* Method::GetParamName(const MethodInfo *method, uint32_t index)
    {
        IL2CPP_ASSERT(method != NULL && "Method::GetParamName cannot be invoked with a NULL MethodInfo.");

        if (index >= method->parameters_count)
            return NULL;

        return method->parameters[index].name;
    }

    Il2CppClass* Method::GetClass(const MethodInfo *method)
    {
        return method->klass;
    }

    bool Method::HasAttribute(const MethodInfo *method, Il2CppClass *attr_class)
    {
        return Reflection::HasAttribute(method, attr_class);
    }

    uint32_t Method::GetImplementationFlags(const MethodInfo *method)
    {
        return method->iflags;
    }

    uint32_t Method::GetFlags(const MethodInfo *method)
    {
        return method->flags;
    }

    uint32_t Method::GetToken(const MethodInfo *method)
    {
        return method->token;
    }

// From ECMA-335, I.10.2 Overloading
// Methods and properties can be overloaded by:
//  * Number of parameters
//  * Type of any parameter
//  * Calling convention <------ not stored in our metadata yet
//  * Custom modifiers   <------ not supported by il2cpp
//  * Whether a parameter is passed by value or by reference
    static bool AreParametersSame(const ParameterInfo* params1, const ParameterInfo* params2, int count)
    {
        for (int i = 0; i < count; i++)
        {
            const Il2CppType* param1 = params1[i].parameter_type;
            const Il2CppType* param2 = params2[i].parameter_type;

            if (param1->byref != param2->byref)
            {
                return false;
            }

            if (Class::FromIl2CppType(param1) != Class::FromIl2CppType(param2))
            {
                return false;
            }
        }

        return true;
    }

    static int CompareParameters(const ParameterInfo* params1, const ParameterInfo* params2, int count)
    {
        for (int i = 0; i < count; i++)
        {
            const Il2CppType* param1 = params1[i].parameter_type;
            const Il2CppType* param2 = params2[i].parameter_type;

            if (param1->byref == param2->byref)
            {
                return Class::FromIl2CppType(param1) < Class::FromIl2CppType(param2);
            }

            return param1->byref < param2->byref;
        }

        return true;
    }

    bool Method::IsSameOverloadSignature(const MethodInfo* method1, const MethodInfo* method2)
    {
        if (method1->parameters_count != method2->parameters_count)
        {
            return false;
        }

        return AreParametersSame(method1->parameters, method2->parameters, method1->parameters_count);
    }

    bool Method::IsSameOverloadSignature(const PropertyInfo* property1, const PropertyInfo* property2)
    {
        uint8_t parameterCount1, parameterCount2;
        const ParameterInfo* parameters1;
        const ParameterInfo* parameters2;

        if (property1->get != NULL)
        {
            parameterCount1 = property1->get->parameters_count;
            parameters1 = property1->get->parameters;
        }
        else
        {
            // In set method, value is the last parameter, so we just don't care about it
            parameterCount1 = property1->set->parameters_count - 1;
            parameters1 = property1->set->parameters;
        }

        if (property2->get != NULL)
        {
            parameterCount2 = property2->get->parameters_count;
            parameters2 = property2->get->parameters;
        }
        else
        {
            parameterCount2 = property2->set->parameters_count - 1;
            parameters2 = property2->set->parameters;
        }

        if (parameterCount1 != parameterCount2)
        {
            return false;
        }

        return AreParametersSame(parameters1, parameters2, parameterCount1);
    }

    int Method::CompareOverloadSignature(const PropertyInfo* property1, const PropertyInfo* property2)
    {
        uint8_t parameterCount1, parameterCount2;
        const ParameterInfo* parameters1;
        const ParameterInfo* parameters2;

        if (property1->get != NULL)
        {
            parameterCount1 = property1->get->parameters_count;
            parameters1 = property1->get->parameters;
        }
        else
        {
            // In set method, value is the last parameter, so we just don't care about it
            parameterCount1 = property1->set->parameters_count - 1;
            parameters1 = property1->set->parameters;
        }

        if (property2->get != NULL)
        {
            parameterCount2 = property2->get->parameters_count;
            parameters2 = property2->get->parameters;
        }
        else
        {
            parameterCount2 = property2->set->parameters_count - 1;
            parameters2 = property2->set->parameters;
        }

        if (parameterCount1 == parameterCount2)
        {
            return CompareParameters(parameters1, parameters2, parameterCount1);
        }

        return parameterCount1 < parameterCount2;
    }

    const char* Method::GetParameterDefaultValue(const MethodInfo* method, const ParameterInfo *parameter, const Il2CppType** type, bool* isExplicitySetNullDefaultValue)
    {
        return reinterpret_cast<const char*>(MetadataCache::GetParameterDefaultValue(method, parameter, type, isExplicitySetNullDefaultValue));
    }

    std::string Method::GetFullName(const MethodInfo* method)
    {
        std::string str;
        str += Type::GetName(&method->klass->byval_arg, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
        str += "::";
        str += Method::GetNameWithGenericTypes(method);

        return str;
    }
} /* namespace vm */
} /* namespace il2cpp */
