#pragma once

#include <stdint.h>
#include "il2cpp-metadata.h"
#include "metadata/Il2CppTypeVector.h"

struct Il2CppGenericClass;
struct Il2CppGenericContext;
struct Il2CppGenericInst;
struct Il2CppGenericMethod;
union Il2CppRGCTXData;
struct Il2CppRGCTXDefinition;
struct Il2CppType;
struct MethodInfo;
struct ParameterInfo;
struct Il2CppClass;

namespace il2cpp
{
namespace metadata
{
    class GenericMetadata
    {
    public:
        static ParameterInfo* InflateParameters(const ParameterInfo* parameters, uint8_t parameterCount, const Il2CppGenericContext* context, bool inflateMethodVars);
        static Il2CppGenericClass* GetGenericClass(const Il2CppClass* elementClass, const Il2CppGenericInst* inst);
        static Il2CppGenericClass* GetGenericClass(const Il2CppType* elementType, const Il2CppGenericInst* inst);

        static const MethodInfo* Inflate(const MethodInfo* methodDefinition, const Il2CppGenericContext* context);
        static const Il2CppGenericMethod* Inflate(const Il2CppGenericMethod* genericMethod, const Il2CppGenericContext* context);

        static Il2CppRGCTXData* InflateRGCTX(const Il2CppImage* image, uint32_t token, const Il2CppGenericContext* context);

        // temporary while we generate generics
        static void RegisterGenericClasses(Il2CppGenericClass* const* genericClasses, int32_t genericClassesCount);

        static const Il2CppType* InflateIfNeeded(const Il2CppType* type, const Il2CppGenericContext* context, bool inflateMethodVars);

        typedef void(*GenericClassWalkCallback)(Il2CppClass* type, void* context);
        static void WalkAllGenericClasses(GenericClassWalkCallback callback, void* context);

        static int GetMaximumRuntimeGenericDepth();
        static void SetMaximumRuntimeGenericDepth(int depth);

        static void Clear();
    };
} /* namespace vm */
} /* namespace il2cpp */
