#include "il2cpp-config.h"
#include "MetadataCache.h"
#include "GlobalMetadata.h"

#include <map>
#include <limits>
#include "il2cpp-tabledefs.h"
#include "il2cpp-runtime-stats.h"
#include "gc/GarbageCollector.h"
#include "metadata/ArrayMetadata.h"
#include "metadata/GenericMetadata.h"
#include "metadata/GenericMethod.h"
#include "os/Atomic.h"
#include "os/Mutex.h"
#include "utils/CallOnce.h"
#include "utils/Collections.h"
#include "utils/Il2CppHashSet.h"
#include "utils/Memory.h"
#include "utils/PathUtils.h"
#include "vm/Assembly.h"
#include "vm/Class.h"
#include "vm/ClassInlines.h"
#include "vm/GenericClass.h"
#include "vm/MetadataAlloc.h"
#include "vm/MetadataLoader.h"
#include "vm/MetadataLock.h"
#include "vm/Method.h"
#include "vm/Object.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Type.h"
#include "vm-utils/NativeSymbol.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

// ==={{ huatuo
#include "huatuo/metadata/Assembly.h"
#include "huatuo/metadata/MetadataModule.h"
// ===}} huatuo

typedef std::map<Il2CppClass*, Il2CppClass*> PointerTypeMap;

typedef Il2CppHashSet<const Il2CppGenericMethod*, il2cpp::metadata::Il2CppGenericMethodHash, il2cpp::metadata::Il2CppGenericMethodCompare> Il2CppGenericMethodSet;
typedef Il2CppGenericMethodSet::const_iterator Il2CppGenericMethodSetIter;
static Il2CppGenericMethodSet s_GenericMethodSet;

struct Il2CppMetadataCache
{
    baselib::ReentrantLock m_CacheMutex;
    PointerTypeMap m_PointerTypes;
};

static Il2CppMetadataCache s_MetadataCache;
static int32_t s_ImagesCount = 0;
static Il2CppImage* s_ImagesTable = NULL;
static int32_t s_AssembliesCount = 0;
static Il2CppAssembly* s_AssembliesTable = NULL;


typedef Il2CppHashSet<const Il2CppGenericInst*, il2cpp::metadata::Il2CppGenericInstHash, il2cpp::metadata::Il2CppGenericInstCompare> Il2CppGenericInstSet;
static Il2CppGenericInstSet s_GenericInstSet;

typedef il2cpp::vm::Il2CppMethodTableMap::const_iterator Il2CppMethodTableMapIter;
static il2cpp::vm::Il2CppMethodTableMap s_MethodTableMap;

typedef il2cpp::vm::Il2CppUnresolvedSignatureMap::const_iterator Il2CppUnresolvedSignatureMapIter;
static il2cpp::vm::Il2CppUnresolvedSignatureMap *s_pUnresolvedSignatureMap;

typedef Il2CppHashMap<FieldInfo*, int32_t, il2cpp::utils::PointerHash<FieldInfo> > Il2CppThreadLocalStaticOffsetHashMap;
typedef Il2CppThreadLocalStaticOffsetHashMap::iterator Il2CppThreadLocalStaticOffsetHashMapIter;
static Il2CppThreadLocalStaticOffsetHashMap s_ThreadLocalStaticOffsetMap;

static const Il2CppCodeRegistration * s_Il2CppCodeRegistration;
static const Il2CppMetadataRegistration* s_MetadataCache_Il2CppMetadataRegistration;
static const Il2CppCodeGenOptions* s_Il2CppCodeGenOptions;

static il2cpp::vm::WindowsRuntimeTypeNameToClassMap s_WindowsRuntimeTypeNameToClassMap;
static il2cpp::vm::ClassToWindowsRuntimeTypeNameMap s_ClassToWindowsRuntimeTypeNameMap;

struct InteropDataToTypeConverter
{
    inline const Il2CppType* operator()(const Il2CppInteropData& interopData) const
    {
        return interopData.type;
    }
};

typedef il2cpp::utils::collections::ArrayValueMap<const Il2CppType*, Il2CppInteropData, InteropDataToTypeConverter, il2cpp::metadata::Il2CppTypeLess, il2cpp::metadata::Il2CppTypeEqualityComparer> InteropDataMap;
static InteropDataMap s_InteropData;

struct WindowsRuntimeFactoryTableEntryToTypeConverter
{
    inline const Il2CppType* operator()(const Il2CppWindowsRuntimeFactoryTableEntry& entry) const
    {
        return entry.type;
    }
};

typedef il2cpp::utils::collections::ArrayValueMap<const Il2CppType*, Il2CppWindowsRuntimeFactoryTableEntry, WindowsRuntimeFactoryTableEntryToTypeConverter, il2cpp::metadata::Il2CppTypeLess, il2cpp::metadata::Il2CppTypeEqualityComparer> WindowsRuntimeFactoryTable;
static WindowsRuntimeFactoryTable s_WindowsRuntimeFactories;

template<typename K, typename V>
struct PairToKeyConverter
{
    inline const K& operator()(const std::pair<K, V>& pair) const
    {
        return pair.first;
    }
};

typedef il2cpp::utils::collections::ArrayValueMap<const Il2CppGuid*, std::pair<const Il2CppGuid*, Il2CppClass*>, PairToKeyConverter<const Il2CppGuid*, Il2CppClass*> > GuidToClassMap;
static GuidToClassMap s_GuidToNonImportClassMap;

// ==={{ huatuo 
static il2cpp::utils::dynamic_array<Il2CppAssembly*> s_cliAssemblies;
// ===}} huatuo

void il2cpp::vm::MetadataCache::Register(const Il2CppCodeRegistration* const codeRegistration, const Il2CppMetadataRegistration* const metadataRegistration, const Il2CppCodeGenOptions* const codeGenOptions)
{
    il2cpp::vm::GlobalMetadata::Register(codeRegistration, metadataRegistration, codeGenOptions);

    s_Il2CppCodeRegistration = codeRegistration;
    s_MetadataCache_Il2CppMetadataRegistration = metadataRegistration;
    s_Il2CppCodeGenOptions = codeGenOptions;
}

Il2CppClass* il2cpp::vm::MetadataCache::GetTypeInfoFromTypeIndex(const Il2CppImage *image, TypeIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetTypeInfoFromTypeIndex(index);
}

const MethodInfo* il2cpp::vm::MetadataCache::GetAssemblyEntryPoint(const Il2CppImage* image)
{
    return il2cpp::vm::GlobalMetadata::GetAssemblyEntryPoint(image);
}

Il2CppMetadataTypeHandle il2cpp::vm::MetadataCache::GetAssemblyTypeHandle(const Il2CppImage* image, AssemblyTypeIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetAssemblyTypeHandle(image, index);
}

Il2CppMetadataTypeHandle il2cpp::vm::MetadataCache::GetAssemblyExportedTypeHandle(const Il2CppImage* image, AssemblyExportedTypeIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetAssemblyExportedTypeHandle(image, index);
}

const MethodInfo* il2cpp::vm::MetadataCache::GetMethodInfoFromMethodHandle(Il2CppMetadataMethodDefinitionHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetMethodInfoFromMethodHandle(handle);
}

bool il2cpp::vm::MetadataCache::Initialize()
{
    if (!il2cpp::vm::GlobalMetadata::Initialize(&s_ImagesCount, &s_AssembliesCount))
    {
        return false;
    }

    il2cpp::metadata::GenericMetadata::RegisterGenericClasses(s_MetadataCache_Il2CppMetadataRegistration->genericClasses, s_MetadataCache_Il2CppMetadataRegistration->genericClassesCount);
    il2cpp::metadata::GenericMetadata::SetMaximumRuntimeGenericDepth(s_Il2CppCodeGenOptions->maximumRuntimeGenericDepth);

    s_GenericInstSet.resize(s_MetadataCache_Il2CppMetadataRegistration->genericInstsCount);
    for (int32_t i = 0; i < s_MetadataCache_Il2CppMetadataRegistration->genericInstsCount; i++)
        s_GenericInstSet.insert(s_MetadataCache_Il2CppMetadataRegistration->genericInsts[i]);

    s_InteropData.assign_external(s_Il2CppCodeRegistration->interopData, s_Il2CppCodeRegistration->interopDataCount);
    s_WindowsRuntimeFactories.assign_external(s_Il2CppCodeRegistration->windowsRuntimeFactoryTable, s_Il2CppCodeRegistration->windowsRuntimeFactoryCount);

    // Pre-allocate these arrays so we don't need to lock when reading later.
    // These arrays hold the runtime metadata representation for metadata explicitly
    // referenced during conversion. There is a corresponding table of same size
    // in the converted metadata, giving a description of runtime metadata to construct.
    s_ImagesTable = (Il2CppImage*)IL2CPP_CALLOC(s_ImagesCount, sizeof(Il2CppImage));
    s_AssembliesTable = (Il2CppAssembly*)IL2CPP_CALLOC(s_AssembliesCount, sizeof(Il2CppAssembly));

    // setup all the Il2CppImages. There are not many and it avoid locks later on
    for (int32_t imageIndex = 0; imageIndex < s_ImagesCount; imageIndex++)
    {
        Il2CppImage* image = s_ImagesTable + imageIndex;

        AssemblyIndex imageAssemblyIndex;
        il2cpp::vm::GlobalMetadata::BuildIl2CppImage(image, imageIndex, &imageAssemblyIndex);

        image->assembly = const_cast<Il2CppAssembly*>(GetAssemblyFromIndex(imageAssemblyIndex));

        std::string nameNoExt = il2cpp::utils::PathUtils::PathNoExtension(image->name);
        image->nameNoExt = (char*)IL2CPP_CALLOC(nameNoExt.size() + 1, sizeof(char));
        strcpy(const_cast<char*>(image->nameNoExt), nameNoExt.c_str());

        for (uint32_t codeGenModuleIndex = 0; codeGenModuleIndex < s_Il2CppCodeRegistration->codeGenModulesCount; ++codeGenModuleIndex)
        {
            if (strcmp(image->name, s_Il2CppCodeRegistration->codeGenModules[codeGenModuleIndex]->moduleName) == 0)
                image->codeGenModule = s_Il2CppCodeRegistration->codeGenModules[codeGenModuleIndex];
        }
        IL2CPP_ASSERT(image->codeGenModule);
        image->dynamic = false;
    }

    // setup all the Il2CppAssemblies.
    for (int32_t assemblyIndex = 0; assemblyIndex < s_ImagesCount; assemblyIndex++)
    {
        Il2CppAssembly* assembly = s_AssembliesTable + assemblyIndex;

        ImageIndex assemblyImageIndex;
        il2cpp::vm::GlobalMetadata::BuildIl2CppAssembly(assembly, assemblyIndex, &assemblyImageIndex);

        assembly->image = il2cpp::vm::MetadataCache::GetImageFromIndex(assemblyImageIndex);

        Assembly::Register(assembly);
    }

    InitializeUnresolvedSignatureTable();

#if IL2CPP_ENABLE_NATIVE_STACKTRACES
    std::vector<MethodDefinitionKey> managedMethods;
    il2cpp::vm::GlobalMetadata::GetAllManagedMethods(managedMethods);
    il2cpp::utils::NativeSymbol::RegisterMethods(managedMethods);
#endif
    return true;
}

void il2cpp::vm::MetadataCache::ExecuteEagerStaticClassConstructors()
{
    for (int32_t i = 0; i < s_AssembliesCount; i++)
    {
        const Il2CppImage* image = s_AssembliesTable[i].image;
        if (image->codeGenModule->staticConstructorTypeIndices != NULL)
        {
            TypeDefinitionIndex* indexPointer = image->codeGenModule->staticConstructorTypeIndices;
            while (*indexPointer) // 0 terminated
            {
                Il2CppMetadataTypeHandle handle = GetTypeHandleFromIndex(image, *indexPointer);
                Il2CppClass* klass = GlobalMetadata::GetTypeInfoFromHandle(handle);
                Runtime::ClassInit(klass);
                indexPointer++;
            }
        }
    }
}

void il2cpp::vm::MetadataCache::ExecuteModuleInitializers()
{
    for (int32_t i = 0; i < s_AssembliesCount; i++)
    {
        const Il2CppImage* image = s_AssembliesTable[i].image;
        if (image->codeGenModule->moduleInitializer != NULL)
            image->codeGenModule->moduleInitializer();
    }
}

void ClearGenericMethodTable()
{
    s_MethodTableMap.clear();
}

void ClearWindowsRuntimeTypeNamesTables()
{
    s_ClassToWindowsRuntimeTypeNameMap.clear();
}

void il2cpp::vm::MetadataCache::InitializeGuidToClassTable()
{
    Il2CppInteropData* interopData = s_Il2CppCodeRegistration->interopData;
    uint32_t interopDataCount = s_Il2CppCodeRegistration->interopDataCount;
    std::vector<std::pair<const Il2CppGuid*, Il2CppClass*> > guidToNonImportClassMap;
    guidToNonImportClassMap.reserve(interopDataCount);

    for (uint32_t i = 0; i < interopDataCount; i++)
    {
        // It's important to check for non-import types because type projections will have identical GUIDs (e.g. IEnumerable<T> and IIterable<T>)
        if (interopData[i].guid != NULL)
        {
            Il2CppClass* klass = il2cpp::vm::Class::FromIl2CppType(interopData[i].type);
            if (!klass->is_import_or_windows_runtime)
                guidToNonImportClassMap.push_back(std::make_pair(interopData[i].guid, klass));
        }
    }

    s_GuidToNonImportClassMap.assign(guidToNonImportClassMap);
}

// this is called later in the intialization cycle with more systems setup like GC
void il2cpp::vm::MetadataCache::InitializeGCSafe()
{
    il2cpp::vm::GlobalMetadata::InitializeStringLiteralTable();
    il2cpp::vm::GlobalMetadata::InitializeGenericMethodTable(s_MethodTableMap);
    il2cpp::vm::GlobalMetadata::InitializeWindowsRuntimeTypeNamesTables(s_WindowsRuntimeTypeNameToClassMap, s_ClassToWindowsRuntimeTypeNameMap);
    InitializeGuidToClassTable();
}

void ClearImageNames()
{
    for (int32_t imageIndex = 0; imageIndex < s_ImagesCount; imageIndex++)
    {
        Il2CppImage* image = s_ImagesTable + imageIndex;
        IL2CPP_FREE((void*)image->nameNoExt);
    }
}

void il2cpp::vm::MetadataCache::Clear()
{
    ClearGenericMethodTable();
    ClearWindowsRuntimeTypeNamesTables();

    delete s_pUnresolvedSignatureMap;

    Assembly::ClearAllAssemblies();

    ClearImageNames();

    IL2CPP_FREE(s_ImagesTable);
    s_ImagesTable = NULL;
    s_ImagesCount = 0;

    IL2CPP_FREE(s_AssembliesTable);
    s_AssembliesTable = NULL;
    s_AssembliesCount = 0;

    s_GenericMethodSet.clear();

    metadata::ArrayMetadata::Clear();

    s_GenericInstSet.clear();

    s_Il2CppCodeRegistration = NULL;
    s_Il2CppCodeGenOptions = NULL;

    il2cpp::metadata::GenericMetadata::Clear();
    il2cpp::metadata::GenericMethod::ClearStatics();

    il2cpp::vm::GlobalMetadata::Clear();
}

void il2cpp::vm::MetadataCache::InitializeUnresolvedSignatureTable()
{
    s_pUnresolvedSignatureMap = new Il2CppUnresolvedSignatureMap();
    il2cpp::vm::GlobalMetadata::InitializeUnresolvedSignatureTable(*s_pUnresolvedSignatureMap);
}

Il2CppClass* il2cpp::vm::MetadataCache::GetGenericInstanceType(Il2CppClass* genericTypeDefinition, const il2cpp::metadata::Il2CppTypeVector& genericArgumentTypes)
{
    const Il2CppGenericInst* inst = il2cpp::vm::MetadataCache::GetGenericInst(genericArgumentTypes);
    Il2CppGenericClass* genericClass = il2cpp::metadata::GenericMetadata::GetGenericClass(genericTypeDefinition, inst);
    return il2cpp::vm::GenericClass::GetClass(genericClass);
}

const MethodInfo* il2cpp::vm::MetadataCache::GetGenericInstanceMethod(const MethodInfo* genericMethodDefinition, const il2cpp::metadata::Il2CppTypeVector& genericArgumentTypes)
{
    Il2CppGenericContext context = { NULL, GetGenericInst(genericArgumentTypes) };
    return il2cpp::vm::GlobalMetadata::GetGenericInstanceMethod(genericMethodDefinition, &context);
}

const Il2CppGenericContext* il2cpp::vm::MetadataCache::GetMethodGenericContext(const MethodInfo* method)
{
    if (!method->is_inflated)
    {
        IL2CPP_NOT_IMPLEMENTED(Image::GetMethodGenericContext);
        return NULL;
    }

    return &method->genericMethod->context;
}

const MethodInfo* il2cpp::vm::MetadataCache::GetGenericMethodDefinition(const MethodInfo* method)
{
    if (!method->is_inflated)
    {
        IL2CPP_NOT_IMPLEMENTED(Image::GetGenericMethodDefinition);
        return NULL;
    }

    return method->genericMethod->methodDefinition;
}

Il2CppClass* il2cpp::vm::MetadataCache::GetPointerType(Il2CppClass* type)
{
    il2cpp::os::FastAutoLock lock(&s_MetadataCache.m_CacheMutex);

    PointerTypeMap::const_iterator i = s_MetadataCache.m_PointerTypes.find(type);
    if (i == s_MetadataCache.m_PointerTypes.end())
        return NULL;

    return i->second;
}

Il2CppClass* il2cpp::vm::MetadataCache::GetWindowsRuntimeClass(const char* fullName)
{
    WindowsRuntimeTypeNameToClassMap::iterator it = s_WindowsRuntimeTypeNameToClassMap.find(fullName);
    if (it != s_WindowsRuntimeTypeNameToClassMap.end())
        return it->second;

    return NULL;
}

const char* il2cpp::vm::MetadataCache::GetWindowsRuntimeClassName(const Il2CppClass* klass)
{
    ClassToWindowsRuntimeTypeNameMap::iterator it = s_ClassToWindowsRuntimeTypeNameMap.find(klass);
    if (it != s_ClassToWindowsRuntimeTypeNameMap.end())
        return it->second;

    return NULL;
}

Il2CppMethodPointer il2cpp::vm::MetadataCache::GetWindowsRuntimeFactoryCreationFunction(const char* fullName)
{
    Il2CppClass* klass = GetWindowsRuntimeClass(fullName);
    if (klass == NULL)
        return NULL;

    WindowsRuntimeFactoryTable::iterator factoryEntry = s_WindowsRuntimeFactories.find_first(&klass->byval_arg);
    if (factoryEntry == s_WindowsRuntimeFactories.end())
        return NULL;

    return factoryEntry->createFactoryFunction;
}

Il2CppClass* il2cpp::vm::MetadataCache::GetClassForGuid(const Il2CppGuid* guid)
{
    IL2CPP_ASSERT(guid != NULL);

    GuidToClassMap::iterator it = s_GuidToNonImportClassMap.find_first(guid);
    if (it != s_GuidToNonImportClassMap.end())
        return it->second;

    return NULL;
}

void il2cpp::vm::MetadataCache::AddPointerType(Il2CppClass* type, Il2CppClass* pointerType)
{
    il2cpp::os::FastAutoLock lock(&s_MetadataCache.m_CacheMutex);
    s_MetadataCache.m_PointerTypes.insert(std::make_pair(type, pointerType));
}

const Il2CppGenericInst* il2cpp::vm::MetadataCache::GetGenericInst(const Il2CppType* const* types, uint32_t typeCount)
{
    // temporary inst to lookup a permanent one that may already exist
    Il2CppGenericInst inst;
    inst.type_argc = typeCount;
    inst.type_argv = (const Il2CppType**)alloca(inst.type_argc * sizeof(Il2CppType*));

    size_t index = 0;
    const Il2CppType* const* typesEnd = types + typeCount;
    for (const Il2CppType* const* iter = types; iter != typesEnd; ++iter, ++index)
        inst.type_argv[index] = *iter;

    {
        // Acquire lock to check if inst has already been cached.
        il2cpp::os::FastAutoLock lock(&s_MetadataCache.m_CacheMutex);
        Il2CppGenericInstSet::const_iterator iter = s_GenericInstSet.find(&inst);
        if (iter != s_GenericInstSet.end())
            return *iter;
    }

    Il2CppGenericInst* newInst = NULL;
    {
        il2cpp::os::FastAutoLock lock(&g_MetadataLock);
        newInst  = (Il2CppGenericInst*)MetadataMalloc(sizeof(Il2CppGenericInst));
        newInst->type_argc = typeCount;
        newInst->type_argv = (const Il2CppType**)MetadataMalloc(newInst->type_argc * sizeof(Il2CppType*));
    }

    index = 0;
    for (const Il2CppType* const* iter = types; iter != typesEnd; ++iter, ++index)
        newInst->type_argv[index] = *iter;

    {
        // Acquire lock agains to attempt to cache inst.
        il2cpp::os::FastAutoLock lock(&s_MetadataCache.m_CacheMutex);
        // Another thread may have already added this inst or we may be the first.
        // In either case, the iterator returned from 'insert' points to the item
        // cached within the set. We can always return this. In the case of another
        // thread beating us, the only downside is an extra allocation in the
        // metadata memory pool that lives for life of process anyway.
        auto result = s_GenericInstSet.insert(newInst);
        if (result.second)
            ++il2cpp_runtime_stats.generic_instance_count;

        return *(result.first);
    }
}

const Il2CppGenericInst* il2cpp::vm::MetadataCache::GetGenericInst(const il2cpp::metadata::Il2CppTypeVector& types)
{
    return GetGenericInst(&types[0], static_cast<uint32_t>(types.size()));
}

static baselib::ReentrantLock s_GenericMethodMutex;
const Il2CppGenericMethod* il2cpp::vm::MetadataCache::GetGenericMethod(const MethodInfo* methodDefinition, const Il2CppGenericInst* classInst, const Il2CppGenericInst* methodInst)
{
    Il2CppGenericMethod method = { 0 };
    method.methodDefinition = methodDefinition;
    method.context.class_inst = classInst;
    method.context.method_inst = methodInst;

    il2cpp::os::FastAutoLock lock(&s_GenericMethodMutex);
    Il2CppGenericMethodSet::const_iterator iter = s_GenericMethodSet.find(&method);
    if (iter != s_GenericMethodSet.end())
        return *iter;

    Il2CppGenericMethod* newMethod = MetadataAllocGenericMethod();
    newMethod->methodDefinition = methodDefinition;
    newMethod->context.class_inst = classInst;
    newMethod->context.method_inst = methodInst;

    s_GenericMethodSet.insert(newMethod);

    return newMethod;
}

static bool IsShareableEnum(const Il2CppType* type)
{
    // Base case for recursion - we've found an enum.
    if (il2cpp::vm::Type::IsEnum(type))
        return true;

    if (il2cpp::vm::Type::IsGenericInstance(type))
    {
        // Recursive case - look "inside" the generic instance type to see if this is a nested enum.
        Il2CppClass* definition = il2cpp::vm::GenericClass::GetTypeDefinition(type->data.generic_class);
        return IsShareableEnum(il2cpp::vm::Class::GetType(definition));
    }

    // Base case for recurion - this is not an enum or a generic instance type.
    return false;
}

// this logic must match the C# logic in GenericSharingAnalysis.GetSharedTypeForGenericParameter
static const Il2CppGenericInst* GetSharedInst(const Il2CppGenericInst* inst)
{
    if (inst == NULL)
        return NULL;

    il2cpp::metadata::Il2CppTypeVector types;
    for (uint32_t i = 0; i < inst->type_argc; ++i)
    {
        if (il2cpp::vm::Type::IsReference(inst->type_argv[i]))
            types.push_back(&il2cpp_defaults.object_class->byval_arg);
        else
        {
            const Il2CppType* type = inst->type_argv[i];
            if (s_Il2CppCodeGenOptions->enablePrimitiveValueTypeGenericSharing)
            {
                if (IsShareableEnum(type))
                {
                    const Il2CppType* underlyingType = il2cpp::vm::Type::GetUnderlyingType(type);
                    switch (underlyingType->type)
                    {
                        case IL2CPP_TYPE_I1:
                            type = &il2cpp_defaults.sbyte_shared_enum->byval_arg;
                            break;
                        case IL2CPP_TYPE_I2:
                            type = &il2cpp_defaults.int16_shared_enum->byval_arg;
                            break;
                        case IL2CPP_TYPE_I4:
                            type = &il2cpp_defaults.int32_shared_enum->byval_arg;
                            break;
                        case IL2CPP_TYPE_I8:
                            type = &il2cpp_defaults.int64_shared_enum->byval_arg;
                            break;
                        case IL2CPP_TYPE_U1:
                            type = &il2cpp_defaults.byte_shared_enum->byval_arg;
                            break;
                        case IL2CPP_TYPE_U2:
                            type = &il2cpp_defaults.uint16_shared_enum->byval_arg;
                            break;
                        case IL2CPP_TYPE_U4:
                            type = &il2cpp_defaults.uint32_shared_enum->byval_arg;
                            break;
                        case IL2CPP_TYPE_U8:
                            type = &il2cpp_defaults.uint64_shared_enum->byval_arg;
                            break;
                        default:
                            IL2CPP_ASSERT(0 && "Invalid enum underlying type");
                            break;
                    }
                }
            }

            if (il2cpp::vm::Type::IsGenericInstance(type))
            {
                const Il2CppGenericInst* sharedInst = GetSharedInst(type->data.generic_class->context.class_inst);
                Il2CppGenericClass* gklass = il2cpp::metadata::GenericMetadata::GetGenericClass(type->data.generic_class->type, sharedInst);
                Il2CppClass* klass = il2cpp::vm::GenericClass::GetClass(gklass);
                type = &klass->byval_arg;
            }
            types.push_back(type);
        }
    }

    const Il2CppGenericInst* sharedInst = il2cpp::vm::MetadataCache::GetGenericInst(types);

    return sharedInst;
}

InvokerMethod il2cpp::vm::MetadataCache::GetInvokerMethodPointer(const MethodInfo* methodDefinition, const Il2CppGenericContext* context)
{
    Il2CppGenericMethod method = { 0 };
    method.methodDefinition = const_cast<MethodInfo*>(methodDefinition);
    method.context.class_inst = context->class_inst;
    method.context.method_inst = context->method_inst;

    Il2CppMethodTableMapIter iter = s_MethodTableMap.find(&method);
    if (iter != s_MethodTableMap.end())
    {
        IL2CPP_ASSERT(iter->second->invokerIndex >= 0);
        if (static_cast<uint32_t>(iter->second->invokerIndex) < s_Il2CppCodeRegistration->invokerPointersCount)
            return s_Il2CppCodeRegistration->invokerPointers[iter->second->invokerIndex];
        return NULL;
    }
    // get the shared version if it exists
    method.context.class_inst = GetSharedInst(context->class_inst);
    method.context.method_inst = GetSharedInst(context->method_inst);

    iter = s_MethodTableMap.find(&method);
    if (iter != s_MethodTableMap.end())
    {
        IL2CPP_ASSERT(iter->second->invokerIndex >= 0);
        if (static_cast<uint32_t>(iter->second->invokerIndex) < s_Il2CppCodeRegistration->invokerPointersCount)
            return s_Il2CppCodeRegistration->invokerPointers[iter->second->invokerIndex];
        return NULL;
    }

    return NULL;
}

Il2CppMethodPointer il2cpp::vm::MetadataCache::GetMethodPointer(const MethodInfo* methodDefinition, const Il2CppGenericContext* context)
{
    Il2CppGenericMethod method = { 0 };
    method.methodDefinition = const_cast<MethodInfo*>(methodDefinition);
    method.context.class_inst = context->class_inst;
    method.context.method_inst = context->method_inst;

    Il2CppMethodTableMapIter iter = s_MethodTableMap.find(&method);
    if (iter != s_MethodTableMap.end())
    {
        IL2CPP_ASSERT(iter->second->invokerIndex >= 0);
        if (iter->second->adjustorThunkIndex != -1)
            return s_Il2CppCodeRegistration->genericAdjustorThunks[iter->second->adjustorThunkIndex];

        if (static_cast<uint32_t>(iter->second->methodIndex) < s_Il2CppCodeRegistration->genericMethodPointersCount)
            return s_Il2CppCodeRegistration->genericMethodPointers[iter->second->methodIndex];
        return NULL;
    }

    method.context.class_inst = GetSharedInst(context->class_inst);
    method.context.method_inst = GetSharedInst(context->method_inst);

    iter = s_MethodTableMap.find(&method);
    if (iter != s_MethodTableMap.end())
    {
        IL2CPP_ASSERT(iter->second->invokerIndex >= 0);
        if (iter->second->adjustorThunkIndex != -1)
            return s_Il2CppCodeRegistration->genericAdjustorThunks[iter->second->adjustorThunkIndex];

        if (static_cast<uint32_t>(iter->second->methodIndex) < s_Il2CppCodeRegistration->genericMethodPointersCount)
            return s_Il2CppCodeRegistration->genericMethodPointers[iter->second->methodIndex];
        return NULL;
    }

    return NULL;
}

const Il2CppType* il2cpp::vm::MetadataCache::GetIl2CppTypeFromIndex(const Il2CppImage* image, TypeIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetIl2CppTypeFromIndex(index);
}

const Il2CppType* il2cpp::vm::MetadataCache::GetTypeFromRgctxDefinition(const Il2CppRGCTXDefinition* rgctxDef)
{
    return il2cpp::vm::GlobalMetadata::GetTypeFromRgctxDefinition(rgctxDef);
}

const Il2CppGenericMethod* il2cpp::vm::MetadataCache::GetGenericMethodFromRgctxDefinition(const Il2CppRGCTXDefinition* rgctxDef)
{
    return il2cpp::vm::GlobalMetadata::GetGenericMethodFromRgctxDefinition(rgctxDef);
}

const MethodInfo* il2cpp::vm::MetadataCache::GetMethodInfoFromVTableSlot(const Il2CppClass* klass, int32_t vTableSlot)
{
    return il2cpp::vm::GlobalMetadata::GetMethodInfoFromVTableSlot(klass, vTableSlot);
}

static int CompareIl2CppTokenAdjustorThunkPair(const void* pkey, const void* pelem)
{
    return (int)(((Il2CppTokenAdjustorThunkPair*)pkey)->token - ((Il2CppTokenAdjustorThunkPair*)pelem)->token);
}

Il2CppMethodPointer il2cpp::vm::MetadataCache::GetAdjustorThunk(const Il2CppImage* image, uint32_t token)
{
    // ==={{ huatuo
    if (huatuo::metadata::IsInterpreterIndex(image->token))
    {
        return huatuo::metadata::MetadataModule::GetAdjustorThunk(image, token);
    }
    // ===}} huatuo
    if (image->codeGenModule->adjustorThunkCount == 0)
        return NULL;

    Il2CppTokenAdjustorThunkPair key;
    memset(&key, 0, sizeof(Il2CppTokenAdjustorThunkPair));
    key.token = token;

    const Il2CppTokenAdjustorThunkPair* result = (const Il2CppTokenAdjustorThunkPair*)bsearch(&key, image->codeGenModule->adjustorThunks,
        image->codeGenModule->adjustorThunkCount, sizeof(Il2CppTokenAdjustorThunkPair), CompareIl2CppTokenAdjustorThunkPair);

    if (result == NULL)
        return NULL;

    return result->adjustorThunk;
}

Il2CppMethodPointer il2cpp::vm::MetadataCache::GetMethodPointer(const Il2CppImage* image, uint32_t token)
{
    uint32_t rid = GetTokenRowId(token);
    uint32_t table =  GetTokenType(token);
    if (rid == 0)
        return NULL;

    // ==={{ huatuo
    if (huatuo::metadata::IsInterpreterImage(image))
    {
        return huatuo::metadata::MetadataModule::GetMethodPointer(image, token);
    }
    // ===}} huatuo

    IL2CPP_ASSERT(rid <= image->codeGenModule->methodPointerCount);

    return image->codeGenModule->methodPointers[rid - 1];
}

InvokerMethod il2cpp::vm::MetadataCache::GetMethodInvoker(const Il2CppImage* image, uint32_t token)
{
    uint32_t rid = GetTokenRowId(token);
    uint32_t table = GetTokenType(token);
    if (rid == 0)
        return NULL;
    // ==={{ huatuo
    if (huatuo::metadata::IsInterpreterImage(image))
    {
        return huatuo::metadata::MetadataModule::GetMethodInvoker(image, token);
    }
    // ===}} huatuo
    int32_t index = image->codeGenModule->invokerIndices[rid - 1];

    if (index == kMethodIndexInvalid)
        return NULL;

    IL2CPP_ASSERT(index >= 0 && static_cast<uint32_t>(index) < s_Il2CppCodeRegistration->invokerPointersCount);
    return s_Il2CppCodeRegistration->invokerPointers[index];
}

const Il2CppInteropData* il2cpp::vm::MetadataCache::GetInteropDataForType(const Il2CppType* type)
{
    IL2CPP_ASSERT(type != NULL);
    InteropDataMap::iterator interopData = s_InteropData.find_first(type);
    if (interopData == s_InteropData.end())
        return NULL;

    return interopData;
}

static bool MatchTokens(Il2CppTokenIndexMethodTuple key, Il2CppTokenIndexMethodTuple element)
{
    return key.token < element.token;
}

Il2CppMethodPointer il2cpp::vm::MetadataCache::GetReversePInvokeWrapper(const Il2CppImage* image, const MethodInfo* method)
{
    if (image->codeGenModule->reversePInvokeWrapperCount == 0)
        return NULL;

    // For each image (i.e. assembly), the reverse pinvoke wrapper indices are in an array sorted by
    // metadata token. Each entry also might have the method metadata pointer, which is used to further
    // find methods that have a matching metadata token.

    Il2CppTokenIndexMethodTuple key;
    memset(&key, 0, sizeof(Il2CppTokenIndexMethodTuple));
    key.token = method->token;

    // Binary search for a range which matches the metadata token.
    auto begin = image->codeGenModule->reversePInvokeWrapperIndices;
    auto end = image->codeGenModule->reversePInvokeWrapperIndices + image->codeGenModule->reversePInvokeWrapperCount;
    auto matchingRange = std::equal_range(begin, end, key, &MatchTokens);

    int32_t index = -1;
    auto numberOfMatches = std::distance(matchingRange.first, matchingRange.second);
    if (numberOfMatches == 1)
    {
        // Normal case - we found one non-generic method.
        index = matchingRange.first->index;
    }
    else if (numberOfMatches > 1)
    {
        // Multiple generic instance methods share the same token, since it is from the generic method definition.
        // To find the proper method, look for the one with a matching method metadata pointer.
        const Il2CppTokenIndexMethodTuple* currentMatch = matchingRange.first;
        const Il2CppTokenIndexMethodTuple* lastMatch = matchingRange.second;
        while (currentMatch != lastMatch)
        {
            // First, check the method metadata, and use it if it has been initialized.
            // If not, let's fall back to the generic method.
            const MethodInfo* possibleMatch = (const MethodInfo*)*currentMatch->method;
            if (!il2cpp::vm::GlobalMetadata::IsRuntimeMetadataInitialized(possibleMatch))
                possibleMatch = il2cpp::metadata::GenericMethod::GetMethod(il2cpp::vm::GlobalMetadata::GetGenericMethodFromTokenMethodTuple(currentMatch));
            if (possibleMatch == method)
            {
                index = currentMatch->index;
                break;
            }
            currentMatch++;
        }
    }

    if (index == -1)
        return NULL;

    IL2CPP_ASSERT(index >= 0 && static_cast<uint32_t>(index) < s_Il2CppCodeRegistration->reversePInvokeWrapperCount);
    return s_Il2CppCodeRegistration->reversePInvokeWrappers[index];
}

static const Il2CppType* GetReducedType(const Il2CppType* type)
{
    if (type->byref)
        return &il2cpp_defaults.object_class->byval_arg;

    if (il2cpp::vm::Type::IsEnum(type))
        type = il2cpp::vm::Type::GetUnderlyingType(type);

    switch (type->type)
    {
        case IL2CPP_TYPE_BOOLEAN:
            return &il2cpp_defaults.sbyte_class->byval_arg;
        case IL2CPP_TYPE_CHAR:
            return &il2cpp_defaults.int16_class->byval_arg;
        case IL2CPP_TYPE_BYREF:
        case IL2CPP_TYPE_CLASS:
        case IL2CPP_TYPE_OBJECT:
        case IL2CPP_TYPE_STRING:
        case IL2CPP_TYPE_ARRAY:
        case IL2CPP_TYPE_SZARRAY:
            return &il2cpp_defaults.object_class->byval_arg;
        case IL2CPP_TYPE_GENERICINST:
            if (il2cpp::vm::Type::GenericInstIsValuetype(type))
                return type;
            else
                return &il2cpp_defaults.object_class->byval_arg;
        default:
            return type;
    }
}

Il2CppMethodPointer il2cpp::vm::MetadataCache::GetUnresolvedVirtualCallStub(const MethodInfo* method)
{
    il2cpp::metadata::Il2CppSignature signature;
    signature.Count = method->parameters_count + 1;
    signature.Types = (const Il2CppType**)alloca(signature.Count * sizeof(Il2CppType*));

    signature.Types[0] = GetReducedType(method->return_type);
    for (int i = 0; i < method->parameters_count; ++i)
        signature.Types[i + 1] = GetReducedType(method->parameters[i].parameter_type);

    Il2CppUnresolvedSignatureMapIter it = s_pUnresolvedSignatureMap->find(signature);
    if (it != s_pUnresolvedSignatureMap->end())
        return it->second;

    return NULL;
}

const Il2CppAssembly* il2cpp::vm::MetadataCache::GetAssemblyFromIndex(AssemblyIndex index)
{
    if (index == kGenericContainerIndexInvalid)
        return NULL;

    IL2CPP_ASSERT(index <= s_AssembliesCount);
    return s_AssembliesTable + index;
}

// ==={{ huatuo
const Il2CppAssembly* il2cpp::vm::MetadataCache::GetAssemblyByName(const char* nameToFind)
{
    return GetOrLoadAssemblyByName(nameToFind, false);
}

const Il2CppAssembly* il2cpp::vm::MetadataCache::GetOrLoadAssemblyByName(const char* assemblyNameOrPath, bool tryLoad)
{
    const char* assemblyName = huatuo::GetAssemblyNameFromPath(assemblyNameOrPath);

    il2cpp::utils::VmStringUtils::CaseInsensitiveComparer comparer;

    for (int i = 0; i < s_AssembliesCount; i++)
    {
        const Il2CppAssembly* assembly = s_AssembliesTable + i;

        if (comparer(assembly->aname.name, assemblyName) || comparer(assembly->image->name, assemblyName))
            return assembly;
    }

    il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);

    for (auto assembly : s_cliAssemblies)
    {
        if (comparer(assembly->aname.name, assemblyName) || comparer(assembly->image->name, assemblyName))
            return assembly;
    }

    if (tryLoad)
    {
        Il2CppAssembly* newAssembly = huatuo::metadata::Assembly::LoadFromFile(assemblyNameOrPath);
        if (newAssembly)
        {
            il2cpp::vm::Assembly::Register(newAssembly);
            s_cliAssemblies.push_back(newAssembly);
            return newAssembly;
        }
    }

    return nullptr;
}

const Il2CppAssembly* il2cpp::vm::MetadataCache::LoadAssemblyFromBytes(const char* assemblyBytes, size_t length)
{
    il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);

    Il2CppAssembly* newAssembly = huatuo::metadata::Assembly::LoadFromBytes(assemblyBytes, length, true);
    if (newAssembly)
    {
        // avoid register placeholder assembly twicely.
        for (Il2CppAssembly* ass : s_cliAssemblies)
        {
            if (ass == newAssembly)
            {
                return ass;
            }
        }
        il2cpp::vm::Assembly::Register(newAssembly);
        s_cliAssemblies.push_back(newAssembly);
        return newAssembly;
    }

    return nullptr;
}

const Il2CppAssembly* il2cpp::vm::MetadataCache::LoadAssemblyByName(const char* nameToFind)
{
    return GetOrLoadAssemblyByName(nameToFind, true);
}

const Il2CppGenericMethod* il2cpp::vm::MetadataCache::FindGenericMethod(std::function<bool(const Il2CppGenericMethod*)> predic)
{
    for (auto e : s_MethodTableMap)
    {
        if (predic(e.first))
        {
            return e.first;
        }
    }
    return nullptr;
}

void il2cpp::vm::MetadataCache::FixThreadLocalStaticOffsetForFieldLocked(FieldInfo* field, int32_t offset, const il2cpp::os::FastAutoLock& lock)
{
    s_ThreadLocalStaticOffsetMap[field] = offset;
}

// ===}} huatuo

Il2CppImage* il2cpp::vm::MetadataCache::GetImageFromIndex(ImageIndex index)
{
    if (index == kGenericContainerIndexInvalid)
        return NULL;

    IL2CPP_ASSERT(index <= s_ImagesCount);
    return s_ImagesTable + index;
}

Il2CppClass* il2cpp::vm::MetadataCache::GetTypeInfoFromType(const Il2CppType* type)
{
    if (type == NULL)
        return NULL;

    return il2cpp::vm::GlobalMetadata::GetTypeInfoFromType(type);
}

Il2CppClass* il2cpp::vm::MetadataCache::GetTypeInfoFromHandle(Il2CppMetadataTypeHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetTypeInfoFromHandle(handle);
}

Il2CppMetadataGenericContainerHandle il2cpp::vm::MetadataCache::GetGenericContainerFromGenericClass(const Il2CppImage* image, const Il2CppGenericClass* genericClass)
{
    return il2cpp::vm::GlobalMetadata::GetGenericContainerFromGenericClass(genericClass);
}

Il2CppMetadataGenericContainerHandle il2cpp::vm::MetadataCache::GetGenericContainerFromMethod(Il2CppMetadataMethodDefinitionHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetGenericContainerFromMethod(handle);
}

Il2CppMetadataGenericParameterHandle il2cpp::vm::MetadataCache::GetGenericParameterFromType(const Il2CppType* type)
{
    return il2cpp::vm::GlobalMetadata::GetGenericParameterFromType(type);
}

Il2CppClass* il2cpp::vm::MetadataCache::GetContainerDeclaringType(Il2CppMetadataGenericContainerHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetContainerDeclaringType(handle);
}

Il2CppClass* il2cpp::vm::MetadataCache::GetParameterDeclaringType(Il2CppMetadataGenericParameterHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetParameterDeclaringType(handle);
}

Il2CppMetadataGenericParameterHandle il2cpp::vm::MetadataCache::GetGenericParameterFromIndex(Il2CppMetadataGenericContainerHandle handle, GenericContainerParameterIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetGenericParameterFromIndex(handle, index);
}

const Il2CppType* il2cpp::vm::MetadataCache::GetGenericParameterConstraintFromIndex(Il2CppMetadataGenericParameterHandle handle, GenericParameterConstraintIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetGenericParameterConstraintFromIndex(handle, index);
}

Il2CppClass* il2cpp::vm::MetadataCache::GetNestedTypeFromOffset(const Il2CppClass* klass, TypeNestedTypeIndex offset)
{
    return il2cpp::vm::GlobalMetadata::GetNestedTypeFromOffset(klass, offset);
}

const Il2CppType* il2cpp::vm::MetadataCache::GetInterfaceFromOffset(const Il2CppClass* klass, TypeInterfaceIndex offset)
{
    return il2cpp::vm::GlobalMetadata::GetInterfaceFromOffset(klass, offset);
}

Il2CppInterfaceOffsetInfo il2cpp::vm::MetadataCache::GetInterfaceOffsetInfo(const Il2CppClass* klass, TypeInterfaceOffsetIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetInterfaceOffsetInfo(klass, index);
}

static int CompareIl2CppTokenRangePair(const void* pkey, const void* pelem)
{
    return (int)(((Il2CppTokenRangePair*)pkey)->token - ((Il2CppTokenRangePair*)pelem)->token);
}

il2cpp::vm::RGCTXCollection il2cpp::vm::MetadataCache::GetRGCTXs(const Il2CppImage* image, uint32_t token)
{
    RGCTXCollection collection = { 0, NULL };
    if (image->codeGenModule->rgctxRangesCount == 0)
        return collection;

    Il2CppTokenRangePair key;
    memset(&key, 0, sizeof(Il2CppTokenRangePair));
    key.token = token;

    const Il2CppTokenRangePair* res = (const Il2CppTokenRangePair*)bsearch(&key, image->codeGenModule->rgctxRanges, image->codeGenModule->rgctxRangesCount, sizeof(Il2CppTokenRangePair), CompareIl2CppTokenRangePair);

    if (res == NULL)
        return collection;

    collection.count = res->range.length;
    collection.items = image->codeGenModule->rgctxs + res->range.start;

    return collection;
}

const uint8_t* il2cpp::vm::MetadataCache::GetFieldDefaultValue(const FieldInfo* field, const Il2CppType** type)
{
    return il2cpp::vm::GlobalMetadata::GetFieldDefaultValue(field, type);
}

const uint8_t* il2cpp::vm::MetadataCache::GetParameterDefaultValue(const MethodInfo* method, const ParameterInfo* parameter, const Il2CppType** type, bool* isExplicitySetNullDefaultValue)
{
    return il2cpp::vm::GlobalMetadata::GetParameterDefaultValue(method, parameter, type, isExplicitySetNullDefaultValue);
}

int il2cpp::vm::MetadataCache::GetFieldMarshaledSizeForField(const FieldInfo* field)
{
    return il2cpp::vm::GlobalMetadata::GetFieldMarshaledSizeForField(field);
}

int32_t il2cpp::vm::MetadataCache::GetFieldOffsetFromIndexLocked(const Il2CppClass* klass, int32_t fieldIndexInType, FieldInfo* field, const il2cpp::os::FastAutoLock& lock)
{
    int32_t offset = il2cpp::vm::GlobalMetadata::GetFieldOffset(klass, fieldIndexInType, field);
    if (offset < 0)
    {
        AddThreadLocalStaticOffsetForFieldLocked(field, offset & ~THREAD_LOCAL_STATIC_MASK, lock);
        return THREAD_STATIC_FIELD_OFFSET;
    }
    return offset;
}

void il2cpp::vm::MetadataCache::AddThreadLocalStaticOffsetForFieldLocked(FieldInfo* field, int32_t offset, const il2cpp::os::FastAutoLock& lock)
{
    s_ThreadLocalStaticOffsetMap.add(field, offset);
}

int32_t il2cpp::vm::MetadataCache::GetThreadLocalStaticOffsetForField(FieldInfo* field)
{
    IL2CPP_ASSERT(field->offset == THREAD_STATIC_FIELD_OFFSET);

    il2cpp::os::FastAutoLock lock(&g_MetadataLock);
    Il2CppThreadLocalStaticOffsetHashMapIter iter = s_ThreadLocalStaticOffsetMap.find(field);
    IL2CPP_ASSERT(iter != s_ThreadLocalStaticOffsetMap.end());
    return iter->second;
}

Il2CppMetadataCustomAttributeHandle il2cpp::vm::MetadataCache::GetCustomAttributeTypeToken(const Il2CppImage* image, uint32_t token)
{
    return il2cpp::vm::GlobalMetadata::GetCustomAttributeTypeToken(image, token);
}

const Il2CppAssembly* il2cpp::vm::MetadataCache::GetReferencedAssembly(const Il2CppAssembly* assembly, int32_t referencedAssemblyTableIndex)
{
    return il2cpp::vm::GlobalMetadata::GetReferencedAssembly(assembly, referencedAssemblyTableIndex, s_AssembliesTable, s_AssembliesCount);
}

CustomAttributesCache* il2cpp::vm::MetadataCache::GenerateCustomAttributesCache(Il2CppMetadataCustomAttributeHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GenerateCustomAttributesCache(handle);
}

CustomAttributesCache* il2cpp::vm::MetadataCache::GenerateCustomAttributesCache(const Il2CppImage* image, uint32_t token)
{
    return il2cpp::vm::GlobalMetadata::GenerateCustomAttributesCache(image, token);
}

bool il2cpp::vm::MetadataCache::HasAttribute(Il2CppMetadataCustomAttributeHandle token, Il2CppClass* attribute)
{
    return il2cpp::vm::GlobalMetadata::HasAttribute(token, attribute);
}

bool il2cpp::vm::MetadataCache::HasAttribute(const Il2CppImage* image, uint32_t token, Il2CppClass* attribute)
{
    return il2cpp::vm::GlobalMetadata::HasAttribute(image, token, attribute);
}

void il2cpp::vm::MetadataCache::InitializeAllMethodMetadata()
{
    il2cpp::vm::GlobalMetadata::InitializeAllMethodMetadata();
}

void* il2cpp::vm::MetadataCache::InitializeRuntimeMetadata(uintptr_t* metadataPointer)
{
    return il2cpp::vm::GlobalMetadata::InitializeRuntimeMetadata(metadataPointer, true);
}

void il2cpp::vm::MetadataCache::WalkPointerTypes(WalkTypesCallback callback, void* context)
{
    il2cpp::os::FastAutoLock lock(&s_MetadataCache.m_CacheMutex);
    for (PointerTypeMap::iterator it = s_MetadataCache.m_PointerTypes.begin(); it != s_MetadataCache.m_PointerTypes.end(); it++)
    {
        callback(it->second, context);
    }
}

Il2CppMetadataTypeHandle il2cpp::vm::MetadataCache::GetTypeHandleFromIndex(const Il2CppImage* image, TypeDefinitionIndex typeIndex)
{
    return il2cpp::vm::GlobalMetadata::GetTypeHandleFromIndex(typeIndex);
}

Il2CppMetadataTypeHandle il2cpp::vm::MetadataCache::GetTypeHandleFromType(const Il2CppType* type)
{
    return il2cpp::vm::GlobalMetadata::GetTypeHandleFromType(type);
}

bool il2cpp::vm::MetadataCache::TypeIsNested(Il2CppMetadataTypeHandle handle)
{
    return il2cpp::vm::GlobalMetadata::TypeIsNested(handle);
}

bool il2cpp::vm::MetadataCache::TypeIsValueType(Il2CppMetadataTypeHandle handle)
{
    return il2cpp::vm::GlobalMetadata::TypeIsValueType(handle);
}

bool il2cpp::vm::MetadataCache::StructLayoutPackIsDefault(Il2CppMetadataTypeHandle handle)
{
    return il2cpp::vm::GlobalMetadata::StructLayoutPackIsDefault(handle);
}

int32_t il2cpp::vm::MetadataCache::StructLayoutPack(Il2CppMetadataTypeHandle handle)
{
    return il2cpp::vm::GlobalMetadata::StructLayoutPack(handle);
}

bool il2cpp::vm::MetadataCache::StructLayoutSizeIsDefault(Il2CppMetadataTypeHandle handle)
{
    return il2cpp::vm::GlobalMetadata::StructLayoutSizeIsDefault(handle);
}

std::pair<const char*, const char*> il2cpp::vm::MetadataCache::GetTypeNamespaceAndName(Il2CppMetadataTypeHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetTypeNamespaceAndName(handle);
}

Il2CppMetadataTypeHandle il2cpp::vm::MetadataCache::GetNestedTypes(Il2CppClass *klass, void* *iter)
{
    return GetNestedTypes(
        klass->typeMetadataHandle,
        iter
    );
}

Il2CppMetadataTypeHandle il2cpp::vm::MetadataCache::GetNestedTypes(Il2CppMetadataTypeHandle handle, void** iter)
{
    return il2cpp::vm::GlobalMetadata::GetNestedTypes(handle, iter);
}

Il2CppMetadataFieldInfo il2cpp::vm::MetadataCache::GetFieldInfo(const Il2CppClass* klass, TypeFieldIndex fieldIndex)
{
    return il2cpp::vm::GlobalMetadata::GetFieldInfo(klass, fieldIndex);
}

Il2CppMetadataMethodInfo il2cpp::vm::MetadataCache::GetMethodInfo(const Il2CppClass* klass, TypeMethodIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetMethodInfo(klass, index);
}

Il2CppMetadataParameterInfo il2cpp::vm::MetadataCache::GetParameterInfo(const Il2CppClass* klass, Il2CppMetadataMethodDefinitionHandle handle, MethodParameterIndex paramIndex)
{
    return il2cpp::vm::GlobalMetadata::GetParameterInfo(klass, handle, paramIndex);
}

Il2CppMetadataPropertyInfo il2cpp::vm::MetadataCache::GetPropertyInfo(const Il2CppClass* klass, TypePropertyIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetPropertyInfo(klass, index);
}

Il2CppMetadataEventInfo il2cpp::vm::MetadataCache::GetEventInfo(const Il2CppClass* klass, TypeEventIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetEventInfo(klass, index);
}

uint32_t il2cpp::vm::MetadataCache::GetGenericContainerCount(Il2CppMetadataGenericContainerHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetGenericContainerCount(handle);
}

void il2cpp::vm::MetadataCache::MakeGenericArgType(Il2CppMetadataGenericContainerHandle containerHandle, Il2CppMetadataGenericParameterHandle paramHandle, Il2CppType* arg)
{
    return il2cpp::vm::GlobalMetadata::MakeGenericArgType(containerHandle, paramHandle, arg);
}

bool il2cpp::vm::MetadataCache::GetGenericContainerIsMethod(Il2CppMetadataGenericContainerHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetGenericContainerIsMethod(handle);
}

int16_t il2cpp::vm::MetadataCache::GetGenericConstraintCount(Il2CppMetadataGenericParameterHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetGenericConstraintCount(handle);
}

const char* il2cpp::vm::MetadataCache::GetGenericParameterName(Il2CppMetadataGenericParameterHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetGenericParameterName(handle);
}

Il2CppGenericParameterInfo il2cpp::vm::MetadataCache::GetGenericParameterInfo(Il2CppMetadataGenericParameterHandle handle)
{
    return il2cpp::vm::GlobalMetadata::GetGenericParameterInfo(handle);
}

uint16_t il2cpp::vm::MetadataCache::GetGenericParameterFlags(Il2CppMetadataGenericContainerHandle handle, GenericContainerParameterIndex index)
{
    return il2cpp::vm::GlobalMetadata::GetGenericParameterFlags(handle, index);
}

const MethodInfo* il2cpp::vm::MetadataCache::GetMethodInfoFromCatchPoint(const Il2CppImage* image, const Il2CppCatchPoint* cp)
{
    return il2cpp::vm::GlobalMetadata::GetMethodInfoFromCatchPoint(cp);
}

const MethodInfo* il2cpp::vm::MetadataCache::GetMethodInfoFromSequencePoint(const Il2CppImage* image, const Il2CppSequencePoint* seqPoint)
{
    return il2cpp::vm::GlobalMetadata::GetMethodInfoFromSequencePoint(seqPoint);
}

Il2CppClass* il2cpp::vm::MetadataCache::GetTypeInfoFromTypeSourcePair(const Il2CppImage* image, const Il2CppTypeSourceFilePair* pair)
{
    return il2cpp::vm::GlobalMetadata::GetTypeInfoFromTypeSourcePair(pair);
}
