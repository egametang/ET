#pragma once

#include <stdint.h>
#include "Assembly.h"
#include "MetadataCache.h"
#include "StackTrace.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-config.h"
#include "metadata/Il2CppTypeVector.h"
#include "os/Mutex.h"
#include "utils/dynamic_array.h"

struct MethodInfo;
struct Il2CppClass;
struct Il2CppGenericContext;
struct Il2CppGenericInst;
struct Il2CppGenericMethod;
struct Il2CppType;
struct Il2CppString;

// ==={{ huatuo
struct Il2CppMethodDefinition;
struct Il2CppFieldDefinition;
struct Il2CppTypeDefinition;
struct Il2CppParameterDefinition;

typedef struct Il2CppImageGlobalMetadata
{
    TypeDefinitionIndex typeStart;
    TypeDefinitionIndex exportedTypeStart;
    CustomAttributeIndex customAttributeStart;
    MethodIndex entryPointIndex;
    const Il2CppImage* image;
} Il2CppImageGlobalMetadata;


// ===}} huatuo

namespace il2cpp
{
namespace vm
{
// ==={{ huatuo
    enum PackingSize
    {
        Zero,
        One,
        Two,
        Four,
        Eight,
        Sixteen,
        ThirtyTwo,
        SixtyFour,
        OneHundredTwentyEight
    };

    const int kBitIsValueType = 1;
    const int kBitIsEnum = 2;
    const int kBitHasFinalizer = 3;
    const int kBitHasStaticConstructor = 4;
    const int kBitIsBlittable = 5;
    const int kBitIsImportOrWindowsRuntime = 6;
    const int kPackingSize = 7;     // This uses 4 bits from bit 7 to bit 10
    const int kPackingSizeIsDefault = 11;
    const int kClassSizeIsDefault = 12; // 此参数只用于反射查询，并无实际意义
    const int kSpecifiedPackingSize = 13; // This uses 4 bits from bit 13 to bit 16 。此参数目前只用于反射查询，并无直接用处
// ===}} huatuo

    class GlobalMetadata
    {
    public:
#if __ENABLE_UNITY_PLUGIN__
        static bool il2cpp_plugin_init();
#endif
        static void Register(const Il2CppCodeRegistration* const codeRegistration, const Il2CppMetadataRegistration* const metadataRegistration, const Il2CppCodeGenOptions* const codeGenOptions);
        static bool Initialize(int32_t* imagesCount, int32_t* assembliesCount);

        static void InitializeAllMethodMetadata();
        static void* InitializeRuntimeMetadata(uintptr_t* metadataPointer, bool throwOnError);
        static void InitializeStringLiteralTable();
        static void InitializeWindowsRuntimeTypeNamesTables(WindowsRuntimeTypeNameToClassMap& windowsRuntimeTypeNameToClassMap, ClassToWindowsRuntimeTypeNameMap& classToWindowsRuntimeTypeNameMap);
        static void InitializeUnresolvedSignatureTable(Il2CppUnresolvedSignatureMap& unresolvedSignatureMap);
        static void InitializeGenericMethodTable(Il2CppMethodTableMap& methodTableMap);

        static void BuildIl2CppImage(Il2CppImage* image, ImageIndex imageIndex, AssemblyIndex* imageAssemblyIndex);
        static void BuildIl2CppAssembly(Il2CppAssembly* assembly, AssemblyIndex assemblyIndex, ImageIndex* assemblyImageIndex);

        static void Clear();

        static const MethodInfo* GetAssemblyEntryPoint(const Il2CppImage* image);
        static Il2CppMetadataTypeHandle GetAssemblyTypeHandle(const Il2CppImage* image, AssemblyTypeIndex index);
        static const Il2CppAssembly* GetReferencedAssembly(const Il2CppAssembly* assembly, int32_t referencedAssemblyTableIndex, const Il2CppAssembly assembliesTable[], int assembliesCount);
        static Il2CppMetadataTypeHandle GetAssemblyExportedTypeHandle(const Il2CppImage* image, AssemblyExportedTypeIndex index);

        static Il2CppClass* GetTypeInfoFromType(const Il2CppType* type);
        static Il2CppClass* GetTypeInfoFromHandle(Il2CppMetadataTypeHandle handle);
        static const Il2CppType* GetInterfaceFromOffset(const Il2CppClass* klass, TypeInterfaceIndex offset);
        static Il2CppInterfaceOffsetInfo GetInterfaceOffsetInfo(const Il2CppClass* klass, TypeInterfaceOffsetIndex index);
        static Il2CppMetadataTypeHandle GetTypeHandleFromIndex(TypeDefinitionIndex typeIndex);
        static Il2CppMetadataTypeHandle GetTypeHandleFromType(const Il2CppType* type);
        static bool TypeIsNested(Il2CppMetadataTypeHandle handle);
        static bool TypeIsValueType(Il2CppMetadataTypeHandle handle);
        static bool StructLayoutPackIsDefault(Il2CppMetadataTypeHandle handle);
        static int32_t StructLayoutPack(Il2CppMetadataTypeHandle handle);
        static bool StructLayoutSizeIsDefault(Il2CppMetadataTypeHandle handle);
        static std::pair<const char*, const char*> GetTypeNamespaceAndName(Il2CppMetadataTypeHandle handle);

        static Il2CppClass* GetNestedTypeFromOffset(const Il2CppClass* klass, TypeNestedTypeIndex offset);
        static Il2CppMetadataTypeHandle GetNestedTypes(Il2CppMetadataTypeHandle handle, void** iter);

        static CustomAttributesCache* GenerateCustomAttributesCache(const Il2CppImage* image, uint32_t token);
        static CustomAttributesCache* GenerateCustomAttributesCache(Il2CppMetadataCustomAttributeHandle handle);
        static Il2CppMetadataCustomAttributeHandle GetCustomAttributeTypeToken(const Il2CppImage* image, uint32_t token);
        static bool HasAttribute(Il2CppMetadataCustomAttributeHandle token, Il2CppClass* attribute);
        static bool HasAttribute(const Il2CppImage* image, uint32_t token, Il2CppClass* attribute);

        static const MethodInfo* GetMethodInfoFromMethodHandle(Il2CppMetadataMethodDefinitionHandle handle);
        static const MethodInfo* GetMethodInfoFromVTableSlot(const Il2CppClass* klass, int32_t vTableSlot);

        // ==={{ huatuo 
        static Il2CppMetadataGenericContainerHandle GetGenericContainerFromIndex(GenericContainerIndex index);
        static const Il2CppMethodDefinition* GetMethodDefinitionFromIndex(MethodIndex index);
        static const Il2CppType* GetInterfaceFromOffset(const Il2CppTypeDefinition* def, TypeInterfaceIndex offset);
        static Il2CppInterfaceOffsetInfo GetInterfaceOffsetInfo(const Il2CppTypeDefinition* typeDefine, TypeInterfaceOffsetIndex index);
        static const Il2CppMethodDefinition* GetMethodDefinitionFromVTableSlot(const Il2CppTypeDefinition* typeDefine, int32_t vTableSlot);
        //static const Il2CppMethodDefinition* GetMethodDefinitionFromVTableSlot(Il2CppClass* typeDefine, int32_t vTableSlot);
        static void InitializeTypeHandle(Il2CppType* type);
        static Il2CppClass* GetTypeInfoFromTypeDefinitionIndex(TypeDefinitionIndex index);
        static Il2CppClass* FromTypeDefinition(TypeDefinitionIndex index);
        static uint8_t ConvertPackingSizeEnumToValue(PackingSize packingSize);
        static PackingSize ConvertPackingSizeToEnum(uint8_t packingSize);

        static Il2CppMetadataGenericParameterHandle GetGenericParameterFromIndexInternal(GenericParameterIndex index);
        static const Il2CppFieldDefinition* GetFieldDefinitionFromTypeDefAndFieldIndex(const Il2CppTypeDefinition* typeDef, FieldIndex index);

        static const char* GetStringFromIndex(StringIndex index);
        static TypeDefinitionIndex GetIndexForTypeDefinition(const Il2CppClass* klass);
        static const Il2CppParameterDefinition* GetParameterDefinitionFromIndex(const Il2CppImage* image, ParameterIndex index);
        static const Il2CppParameterDefinition* GetParameterDefinitionFromIndex(const Il2CppMethodDefinition* methodDef, ParameterIndex index);

        // ===}} huatuo

        static const uint8_t* GetParameterDefaultValue(const MethodInfo* method, const ParameterInfo* parameter, const Il2CppType** type, bool* isExplicitySetNullDefaultValue);
        static const uint8_t* GetFieldDefaultValue(const FieldInfo* field, const Il2CppType** type);
        static uint32_t GetFieldOffset(const Il2CppClass* klass, int32_t fieldIndexInType, FieldInfo* field);
        static int GetFieldMarshaledSizeForField(const FieldInfo* field);

        static Il2CppMetadataFieldInfo GetFieldInfo(const Il2CppClass* klass, TypeFieldIndex index);
        static Il2CppMetadataMethodInfo GetMethodInfo(const Il2CppClass* klass, TypeMethodIndex index);
        static Il2CppMetadataParameterInfo GetParameterInfo(const Il2CppClass* klass, Il2CppMetadataMethodDefinitionHandle handle, MethodParameterIndex index);
        static Il2CppMetadataPropertyInfo GetPropertyInfo(const Il2CppClass* klass, TypePropertyIndex index);
        static Il2CppMetadataEventInfo GetEventInfo(const Il2CppClass* klass, TypeEventIndex index);

        static Il2CppMetadataGenericContainerHandle GetGenericContainerFromGenericClass(const Il2CppGenericClass* genericClass);
        static Il2CppMetadataGenericContainerHandle GetGenericContainerFromMethod(Il2CppMetadataMethodDefinitionHandle handle);
        static Il2CppMetadataGenericParameterHandle GetGenericParameterFromType(const Il2CppType* type);
        static const MethodInfo* GetGenericInstanceMethod(const MethodInfo* genericMethodDefinition, const Il2CppGenericContext* context);
        static const Il2CppType* GetTypeFromRgctxDefinition(const Il2CppRGCTXDefinition* rgctxDef);
        static const Il2CppGenericMethod* GetGenericMethodFromRgctxDefinition(const Il2CppRGCTXDefinition* rgctxDef);
        static Il2CppClass* GetContainerDeclaringType(Il2CppMetadataGenericContainerHandle handle);
        static Il2CppClass* GetParameterDeclaringType(Il2CppMetadataGenericParameterHandle handle);
        static Il2CppMetadataGenericParameterHandle GetGenericParameterFromIndex(Il2CppMetadataGenericContainerHandle handle, GenericContainerParameterIndex index);
        static const Il2CppType* GetGenericParameterConstraintFromIndex(Il2CppMetadataGenericParameterHandle handle, GenericParameterConstraintIndex index);
        static void MakeGenericArgType(Il2CppMetadataGenericContainerHandle containerHandle, Il2CppMetadataGenericParameterHandle paramHandle, Il2CppType* arg);
        static uint32_t GetGenericContainerCount(Il2CppMetadataGenericContainerHandle handle);
        static bool GetGenericContainerIsMethod(Il2CppMetadataGenericContainerHandle handle);
        static const char* GetGenericParameterName(Il2CppMetadataGenericParameterHandle handle);
        static Il2CppGenericParameterInfo GetGenericParameterInfo(Il2CppMetadataGenericParameterHandle handle);
        static uint16_t GetGenericParameterFlags(Il2CppMetadataGenericContainerHandle handle, GenericContainerParameterIndex index);
        static int16_t GetGenericConstraintCount(Il2CppMetadataGenericParameterHandle handle);
        static const Il2CppGenericMethod* GetGenericMethodFromTokenMethodTuple(const Il2CppTokenIndexMethodTuple* tuple);

        static const MethodInfo* GetMethodInfoFromCatchPoint(const Il2CppCatchPoint* cp);
        static const MethodInfo* GetMethodInfoFromSequencePoint(const Il2CppSequencePoint* cp);
        static Il2CppClass* GetTypeInfoFromTypeSourcePair(const Il2CppTypeSourceFilePair* pair);

        static Il2CppClass* GetTypeInfoFromTypeIndex(TypeIndex index, bool throwOnError = true);
        static const Il2CppType* GetIl2CppTypeFromIndex(TypeIndex index);

        template<typename T>
        static inline bool IsRuntimeMetadataInitialized(T item)
        {
            // Runtime metadata items are initialized to an encoded token with the low bit set
            // on startup and when intialized are a pointer to an runtime metadata item
            // So we can rely on pointer allignment being > 1 on our supported platforms
            return !((uintptr_t)item & 1);
        }

#if IL2CPP_ENABLE_NATIVE_STACKTRACES
        static void GetAllManagedMethods(std::vector<MethodDefinitionKey>& managedMethods);
#endif
    };
}
}
