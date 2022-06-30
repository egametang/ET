#pragma once

#include <stdint.h>
#include <string>
#include "il2cpp-config.h"

struct MethodInfo;
struct PropertyInfo;
struct ParameterInfo;

struct Il2CppString;
struct Il2CppType;
struct Il2CppClass;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Method
    {
    public:
        static const Il2CppType* GetReturnType(const MethodInfo* method);
        static const char* GetName(const MethodInfo *method);
        static std::string GetNameWithGenericTypes(const MethodInfo* method);
        static std::string GetFullName(const MethodInfo* method);
        static bool IsGeneric(const MethodInfo *method);
        static bool IsInflated(const MethodInfo *method);
        static bool IsInstance(const MethodInfo *method);
        static bool IsGenericInstance(const MethodInfo *method);
        static bool IsGenericInstanceMethod(const MethodInfo *method);
        static bool IsDefaultInterfaceMethodOnGenericInstance(const MethodInfo* method);
        static uint32_t GetParamCount(const MethodInfo *method);
        static uint32_t GetGenericParamCount(const MethodInfo *method);
        static const Il2CppType* GetParam(const MethodInfo *method, uint32_t index);
        static Il2CppClass* GetClass(const MethodInfo *method);
        static bool HasAttribute(const MethodInfo *method, Il2CppClass *attr_class);
        static Il2CppClass *GetDeclaringType(const MethodInfo* method);
        static uint32_t GetImplementationFlags(const MethodInfo *method);
        static uint32_t GetFlags(const MethodInfo *method);
        static uint32_t GetToken(const MethodInfo *method);
        static const char* GetParamName(const MethodInfo *method, uint32_t index);
        static bool IsSameOverloadSignature(const MethodInfo* method1, const MethodInfo* method2);
        static bool IsSameOverloadSignature(const PropertyInfo* property1, const PropertyInfo* property2);
        static int CompareOverloadSignature(const PropertyInfo* property1, const PropertyInfo* property2);
        static const char* GetParameterDefaultValue(const MethodInfo *method, int32_t parameterPosition, const Il2CppType** type, bool* isExplicitySetNullDefaultValue);
        static uint32_t GetParameterToken(const MethodInfo* method, int32_t parameterPosition);
        static const MethodInfo* GetAmbiguousMethodInfo();
        static const MethodInfo* GetEntryPointNotFoundMethodInfo();
        static bool IsAmbiguousMethodInfo(const MethodInfo* method);
        static bool IsEntryPointNotFoundMethodInfo(const MethodInfo* method);
        static bool HasFullGenericSharingSignature(const MethodInfo* method);
        static Il2CppMethodPointer GetVirtualCallMethodPointer(const MethodInfo* method);
    };
} /* namespace vm */
} /* namespace il2cpp */
