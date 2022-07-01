#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "Assembly.h"
#include "il2cpp-class-internals.h"
#include "utils/dynamic_array.h"
#include "utils/HashUtils.h"
#include "utils/Il2CppHashMap.h"
#include "utils/StringUtils.h"
#include "metadata/Il2CppGenericContextCompare.h"
#include "metadata/Il2CppGenericContextHash.h"
#include "metadata/Il2CppGenericInstCompare.h"
#include "metadata/Il2CppGenericInstHash.h"
#include "metadata/Il2CppGenericMethodCompare.h"
#include "metadata/Il2CppGenericMethodHash.h"
#include "metadata/Il2CppSignature.h"
#include "metadata/Il2CppTypeCompare.h"
#include "metadata/Il2CppTypeHash.h"
#include "metadata/Il2CppTypeVector.h"
#include "metadata/Il2CppTypeVector.h"
#include "vm-utils/VmStringUtils.h"
#include "os/Mutex.h"

namespace il2cpp
{
namespace vm
{
    struct RGCTXCollection
    {
        int32_t count;
        const Il2CppRGCTXDefinition* items;
    };

    typedef Il2CppHashMap<const char*, Il2CppClass*, il2cpp::utils::StringUtils::StringHasher<const char*>, il2cpp::utils::VmStringUtils::CaseSensitiveComparer> WindowsRuntimeTypeNameToClassMap;
    typedef Il2CppHashMap<const Il2CppClass*, const char*, il2cpp::utils::PointerHash<Il2CppClass> > ClassToWindowsRuntimeTypeNameMap;
    typedef Il2CppHashMap<il2cpp::metadata::Il2CppSignature, Il2CppMethodPointer, il2cpp::metadata::Il2CppSignatureHash, il2cpp::metadata::Il2CppSignatureCompare> Il2CppUnresolvedSignatureMap;
    typedef Il2CppHashMap<const Il2CppGenericMethod*, const Il2CppGenericMethodIndices*, il2cpp::metadata::Il2CppGenericMethodHash, il2cpp::metadata::Il2CppGenericMethodCompare> Il2CppMethodTableMap;

    class LIBIL2CPP_CODEGEN_API MetadataCache
    {
    public:

        static void Register(const Il2CppCodeRegistration * const codeRegistration, const Il2CppMetadataRegistration * const metadataRegistration, const Il2CppCodeGenOptions* const codeGenOptions);

        static bool Initialize();
        static void InitializeGCSafe();

        static void Clear();

        static void ExecuteEagerStaticClassConstructors();
        static void ExecuteModuleInitializers();

        static Il2CppClass* GetGenericInstanceType(Il2CppClass* genericTypeDefinition, const il2cpp::metadata::Il2CppTypeVector& genericArgumentTypes);
        static const MethodInfo* GetGenericInstanceMethod(const MethodInfo* genericMethodDefinition, const il2cpp::metadata::Il2CppTypeVector& genericArgumentTypes);
        static const Il2CppGenericContext* GetMethodGenericContext(const MethodInfo* method);
        static const MethodInfo* GetGenericMethodDefinition(const MethodInfo* method);

        static Il2CppClass* GetPointerType(Il2CppClass* type);
        static Il2CppClass* GetWindowsRuntimeClass(const char* fullName);
        static const char* GetWindowsRuntimeClassName(const Il2CppClass* klass);
        static Il2CppMethodPointer GetWindowsRuntimeFactoryCreationFunction(const char* fullName);
        static Il2CppClass* GetClassForGuid(const Il2CppGuid* guid);
        static void AddPointerType(Il2CppClass* type, Il2CppClass* pointerType);

        static const Il2CppGenericInst* GetGenericInst(const Il2CppType* const* types, uint32_t typeCount);
        static const Il2CppGenericInst* GetGenericInst(const il2cpp::metadata::Il2CppTypeVector& types);
        static const Il2CppGenericMethod* GetGenericMethod(const MethodInfo* methodDefinition, const Il2CppGenericInst* classInst, const Il2CppGenericInst* methodInst);

        static InvokerMethod GetInvokerMethodPointer(const MethodInfo* methodDefinition, const Il2CppGenericContext* context);
        static Il2CppMethodPointer GetMethodPointer(const MethodInfo* methodDefinition, const Il2CppGenericContext* context);

        static const MethodInfo* GetMethodInfoFromVTableSlot(const Il2CppClass* klass, int32_t vTableSlot);

        static const Il2CppType* GetTypeFromRgctxDefinition(const Il2CppRGCTXDefinition* rgctxDef);
        static const Il2CppGenericMethod* GetGenericMethodFromRgctxDefinition(const Il2CppRGCTXDefinition* rgctxDef);

        static void InitializeAllMethodMetadata();
        static void* InitializeRuntimeMetadata(uintptr_t* metadataPointer);

        static Il2CppMethodPointer GetAdjustorThunk(const Il2CppImage* image, uint32_t token);
        static Il2CppMethodPointer GetMethodPointer(const Il2CppImage* image, uint32_t token);
        static InvokerMethod GetMethodInvoker(const Il2CppImage* image, uint32_t token);
        static const Il2CppInteropData* GetInteropDataForType(const Il2CppType* type);
        static Il2CppMethodPointer GetReversePInvokeWrapper(const Il2CppImage* image, const MethodInfo* method);

        static Il2CppMethodPointer GetUnresolvedVirtualCallStub(const MethodInfo* method);

        static const Il2CppAssembly* GetAssemblyByName(const char* nameToFind);

        static Il2CppClass* GetTypeInfoFromType(const Il2CppType* type);

        static Il2CppMetadataGenericContainerHandle GetGenericContainerFromGenericClass(const Il2CppImage* image, const Il2CppGenericClass* genericClass);
        static Il2CppMetadataGenericContainerHandle GetGenericContainerFromMethod(Il2CppMetadataMethodDefinitionHandle handle);

        static Il2CppMetadataGenericParameterHandle GetGenericParameterFromType(const Il2CppType* type);
        static Il2CppMetadataGenericParameterHandle GetGenericParameterFromIndex(Il2CppMetadataGenericContainerHandle handle, GenericContainerParameterIndex index);
        static Il2CppClass* GetContainerDeclaringType(Il2CppMetadataGenericContainerHandle handle);
        static Il2CppClass* GetParameterDeclaringType(Il2CppMetadataGenericParameterHandle handle);

        static const Il2CppType* GetGenericParameterConstraintFromIndex(Il2CppMetadataGenericParameterHandle handle, GenericParameterConstraintIndex index);
        static Il2CppClass* GetNestedTypeFromOffset(const Il2CppClass* klass, TypeNestedTypeIndex offset);
        static const Il2CppType* GetInterfaceFromOffset(const Il2CppClass* klass, TypeInterfaceIndex index);
        static Il2CppInterfaceOffsetInfo GetInterfaceOffsetInfo(const Il2CppClass* klass, TypeInterfaceOffsetIndex index);
        static RGCTXCollection GetRGCTXs(const Il2CppImage* image, uint32_t token);
        static const uint8_t* GetFieldDefaultValue(const FieldInfo* field, const Il2CppType** type);
        static const uint8_t* GetParameterDefaultValue(const MethodInfo* method, const ParameterInfo* parameter, const Il2CppType** type, bool* isExplicitySetNullDefaultValue);
        static int GetFieldMarshaledSizeForField(const FieldInfo* field);
        static const MethodInfo* GetMethodInfoFromMethodHandle(Il2CppMetadataMethodDefinitionHandle handle);

        // returns the compiler computed field offset for type definition fields
        static int32_t GetFieldOffsetFromIndexLocked(const Il2CppClass* klass, int32_t fieldIndexInType, FieldInfo* field, const il2cpp::os::FastAutoLock& lock);
        static int32_t GetThreadLocalStaticOffsetForField(FieldInfo* field);
        static void AddThreadLocalStaticOffsetForFieldLocked(FieldInfo* field, int32_t offset, const il2cpp::os::FastAutoLock& lock);

        static const Il2CppAssembly* GetReferencedAssembly(const Il2CppAssembly* assembly, int32_t referencedAssemblyTableIndex);

        static Il2CppMetadataCustomAttributeHandle GetCustomAttributeTypeToken(const Il2CppImage* image, uint32_t token);
        static CustomAttributesCache* GenerateCustomAttributesCache(Il2CppMetadataCustomAttributeHandle token);
        static CustomAttributesCache* GenerateCustomAttributesCache(const Il2CppImage* image, uint32_t token);
        static bool HasAttribute(Il2CppMetadataCustomAttributeHandle token, Il2CppClass* attribute);
        static bool HasAttribute(const Il2CppImage* image, uint32_t token, Il2CppClass* attribute);

        typedef void(*WalkTypesCallback)(Il2CppClass* type, void* context);
        static void WalkPointerTypes(WalkTypesCallback callback, void* context);

        static Il2CppMetadataTypeHandle GetTypeHandleFromType(const Il2CppType* type);
        static bool TypeIsNested(Il2CppMetadataTypeHandle handle);
        static bool TypeIsValueType(Il2CppMetadataTypeHandle handle);
        static bool StructLayoutPackIsDefault(Il2CppMetadataTypeHandle handle);
        static int32_t StructLayoutPack(Il2CppMetadataTypeHandle handle);
        static bool StructLayoutSizeIsDefault(Il2CppMetadataTypeHandle handle);

        static std::pair<const char*, const char*> GetTypeNamespaceAndName(Il2CppMetadataTypeHandle handle);
        static Il2CppMetadataTypeHandle GetNestedTypes(Il2CppClass* klass, void** iter);
        static Il2CppMetadataTypeHandle GetNestedTypes(Il2CppMetadataTypeHandle handle, void** iter);

        static Il2CppMetadataFieldInfo GetFieldInfo(const Il2CppClass* klass, TypeFieldIndex index);
        static Il2CppMetadataMethodInfo GetMethodInfo(const Il2CppClass* klass, TypeMethodIndex index);
        static Il2CppMetadataParameterInfo GetParameterInfo(const Il2CppClass* klass, Il2CppMetadataMethodDefinitionHandle handle, MethodParameterIndex index);
        static Il2CppMetadataPropertyInfo GetPropertyInfo(const Il2CppClass* klass, TypePropertyIndex index);
        static Il2CppMetadataEventInfo GetEventInfo(const Il2CppClass* klass, TypeEventIndex index);

        static void MakeGenericArgType(Il2CppMetadataGenericContainerHandle containerHandle, Il2CppMetadataGenericParameterHandle paramHandle, Il2CppType* arg);
        static uint32_t GetGenericContainerCount(Il2CppMetadataGenericContainerHandle handle);
        static bool GetGenericContainerIsMethod(Il2CppMetadataGenericContainerHandle handle);
        static const char* GetGenericParameterName(Il2CppMetadataGenericParameterHandle handle);
        static Il2CppGenericParameterInfo GetGenericParameterInfo(Il2CppMetadataGenericParameterHandle handle);

        static uint16_t GetGenericParameterFlags(Il2CppMetadataGenericContainerHandle handle, GenericContainerParameterIndex index);
        static int16_t GetGenericConstraintCount(Il2CppMetadataGenericParameterHandle handle);

        static Il2CppClass* GetTypeInfoFromHandle(Il2CppMetadataTypeHandle handle);
        static const MethodInfo* GetAssemblyEntryPoint(const Il2CppImage* image);

        static Il2CppMetadataTypeHandle GetAssemblyTypeHandle(const Il2CppImage* image, AssemblyTypeIndex index);
        static Il2CppMetadataTypeHandle GetAssemblyExportedTypeHandle(const Il2CppImage* image, AssemblyExportedTypeIndex index);

        static const MethodInfo* GetMethodInfoFromCatchPoint(const Il2CppImage* image, const Il2CppCatchPoint* cp);
        static const MethodInfo* GetMethodInfoFromSequencePoint(const Il2CppImage* image, const Il2CppSequencePoint* cp);
        static Il2CppClass* GetTypeInfoFromTypeSourcePair(const Il2CppImage* image, const Il2CppTypeSourceFilePair* pair);

        // The following methods still expose indexes - but only need to be public for debugger support (in il2cpp-stubs.cpp & mono/mini/debugger-agent.c)
        // Called from il2cpp_get_type_from_index
        static const Il2CppType* GetIl2CppTypeFromIndex(const Il2CppImage* image, TypeIndex index);
        // Called from il2cpp_get_class_from_index
        static Il2CppClass* GetTypeInfoFromTypeIndex(const Il2CppImage* image, TypeIndex index);

    private:
        static void InitializeUnresolvedSignatureTable();
        static void InitializeGenericMethodTable();
        static void InitializeGuidToClassTable();
        // ==={{ huatuo begin
    public:
        // ===}} huatuo end
        static Il2CppImage* GetImageFromIndex(ImageIndex index);
        static const Il2CppAssembly* GetAssemblyFromIndex(AssemblyIndex index);
        static Il2CppMetadataTypeHandle GetTypeHandleFromIndex(const Il2CppImage* image, TypeDefinitionIndex typeIndex);

        // ==={{ huatuo
        static const Il2CppAssembly* LoadAssemblyByName(const char* assemblyPath);
        static const Il2CppAssembly* GetOrLoadAssemblyByName(const char* assemblyNameOrPath, bool tryLoad);
        static const Il2CppAssembly* LoadAssemblyFromBytes(const char* assemblyBytes, size_t length);
        static const Il2CppGenericMethod* FindGenericMethod(std::function<bool(const Il2CppGenericMethod*)> predic);
        static void FixThreadLocalStaticOffsetForFieldLocked(FieldInfo* field, int32_t offset, const il2cpp::os::FastAutoLock& lock);
        // ===}} huatuo

    };
} // namespace vm
} // namespace il2cpp
