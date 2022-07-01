#pragma once

#include "il2cpp-config.h"

#include <stdint.h>
#include "il2cpp-runtime-metadata.h"
#include "il2cpp-metadata.h"
#include "il2cpp-pinvoke-support.h"

#define THREAD_LOCAL_STATIC_MASK (int32_t)0x80000000

#define IL2CPP_CLASS_IS_ARRAY(c) ((c)->rank)

struct Il2CppCodeGenModule;
struct Il2CppMetadataRegistration;
struct Il2CppCodeRegistration;

typedef struct Il2CppClass Il2CppClass;
typedef struct Il2CppGuid Il2CppGuid;
typedef struct Il2CppImage Il2CppImage;
typedef struct Il2CppAppDomain Il2CppAppDomain;
typedef struct Il2CppAppDomainSetup Il2CppAppDomainSetup;
typedef struct Il2CppDelegate Il2CppDelegate;
typedef struct Il2CppAppContext Il2CppAppContext;
typedef struct Il2CppNameToTypeHandleHashTable Il2CppNameToTypeHandleHashTable;
typedef struct Il2CppCodeGenModule Il2CppCodeGenModule;
typedef struct Il2CppMetadataRegistration Il2CppMetadataRegistration;
typedef struct Il2CppCodeRegistration Il2CppCodeRegistration;

#if RUNTIME_TINY
typedef Il2CppMethodPointer VirtualInvokeData;
#else
typedef struct VirtualInvokeData
{
    Il2CppMethodPointer methodPtr;
    const MethodInfo* method;
} VirtualInvokeData;
#endif

typedef enum Il2CppTypeNameFormat
{
    IL2CPP_TYPE_NAME_FORMAT_IL,
    IL2CPP_TYPE_NAME_FORMAT_REFLECTION,
    IL2CPP_TYPE_NAME_FORMAT_FULL_NAME,
    IL2CPP_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED
} Il2CppTypeNameFormat;


typedef struct Il2CppDefaults
{
    Il2CppImage *corlib;
    Il2CppClass *object_class;
    Il2CppClass *byte_class;
    Il2CppClass *void_class;
    Il2CppClass *boolean_class;
    Il2CppClass *sbyte_class;
    Il2CppClass *int16_class;
    Il2CppClass *uint16_class;
    Il2CppClass *int32_class;
    Il2CppClass *uint32_class;
    Il2CppClass *int_class;
    Il2CppClass *uint_class;
    Il2CppClass *int64_class;
    Il2CppClass *uint64_class;
    Il2CppClass *single_class;
    Il2CppClass *double_class;
    Il2CppClass *char_class;
    Il2CppClass *string_class;
    Il2CppClass *enum_class;
    Il2CppClass *array_class;
    Il2CppClass *delegate_class;
    Il2CppClass *multicastdelegate_class;
    Il2CppClass *asyncresult_class;
    Il2CppClass *manualresetevent_class;
    Il2CppClass *typehandle_class;
    Il2CppClass *fieldhandle_class;
    Il2CppClass *methodhandle_class;
    Il2CppClass *systemtype_class;
    Il2CppClass *monotype_class;
    Il2CppClass *exception_class;
    Il2CppClass *threadabortexception_class;
    Il2CppClass *thread_class;
    Il2CppClass *internal_thread_class;
    /*Il2CppClass *transparent_proxy_class;
    Il2CppClass *real_proxy_class;
    Il2CppClass *mono_method_message_class;*/
    Il2CppClass *appdomain_class;
    Il2CppClass *appdomain_setup_class;
    Il2CppClass *field_info_class;
    Il2CppClass *method_info_class;
    Il2CppClass *property_info_class;
    Il2CppClass *event_info_class;
    Il2CppClass *mono_event_info_class;
    Il2CppClass *stringbuilder_class;
    /*Il2CppClass *math_class;*/
    Il2CppClass *stack_frame_class;
    Il2CppClass *stack_trace_class;
    Il2CppClass *marshal_class;
    /*Il2CppClass *iserializeable_class;
    Il2CppClass *serializationinfo_class;
    Il2CppClass *streamingcontext_class;*/
    Il2CppClass *typed_reference_class;
    /*Il2CppClass *argumenthandle_class;*/
    Il2CppClass *marshalbyrefobject_class;
    /*Il2CppClass *monitor_class;
    Il2CppClass *iremotingtypeinfo_class;
    Il2CppClass *runtimesecurityframe_class;
    Il2CppClass *executioncontext_class;
    Il2CppClass *internals_visible_class;*/
    Il2CppClass *generic_ilist_class;
    Il2CppClass *generic_icollection_class;
    Il2CppClass *generic_ienumerable_class;
    Il2CppClass *generic_ireadonlylist_class;
    Il2CppClass *generic_ireadonlycollection_class;
    Il2CppClass *runtimetype_class;
    Il2CppClass *generic_nullable_class;
    /*Il2CppClass *variant_class;
    Il2CppClass *com_object_class;*/
    Il2CppClass *il2cpp_com_object_class;
    /*Il2CppClass *com_interop_proxy_class;
    Il2CppClass *iunknown_class;
    Il2CppClass *idispatch_class;
    Il2CppClass *safehandle_class;
    Il2CppClass *handleref_class;*/
    Il2CppClass *attribute_class;
    Il2CppClass *customattribute_data_class;
    //Il2CppClass *critical_finalizer_object;
    Il2CppClass *version;
    Il2CppClass *culture_info;
    Il2CppClass *async_call_class;
    Il2CppClass *assembly_class;
    Il2CppClass *mono_assembly_class;
    Il2CppClass *assembly_name_class;
    Il2CppClass *mono_field_class;
    Il2CppClass *mono_method_class;
    Il2CppClass *mono_method_info_class;
    Il2CppClass *mono_property_info_class;
    Il2CppClass *parameter_info_class;
    Il2CppClass *mono_parameter_info_class;
    Il2CppClass *module_class;
    Il2CppClass *pointer_class;
    Il2CppClass *system_exception_class;
    Il2CppClass *argument_exception_class;
    Il2CppClass *wait_handle_class;
    Il2CppClass *safe_handle_class;
    Il2CppClass *sort_key_class;
    Il2CppClass *dbnull_class;
    Il2CppClass *error_wrapper_class;
    Il2CppClass *missing_class;
    Il2CppClass *value_type_class;

    // Stuff used by the mono code
    Il2CppClass *threadpool_wait_callback_class;
    MethodInfo *threadpool_perform_wait_callback_method;
    Il2CppClass *mono_method_message_class;

    // Windows.Foundation.IReference`1<T>
    Il2CppClass* ireference_class;
    // Windows.Foundation.IReferenceArray`1<T>
    Il2CppClass* ireferencearray_class;
    // Windows.Foundation.Collections.IKeyValuePair`2<K, V>
    Il2CppClass* ikey_value_pair_class;
    // System.Collections.Generic.KeyValuePair`2<K, V>
    Il2CppClass* key_value_pair_class;
    // Windows.Foundation.Uri
    Il2CppClass* windows_foundation_uri_class;
    // Windows.Foundation.IUriRuntimeClass
    Il2CppClass* windows_foundation_iuri_runtime_class_class;
    // System.Uri
    Il2CppClass* system_uri_class;
    // System.Guid
    Il2CppClass* system_guid_class;

    Il2CppClass* sbyte_shared_enum;
    Il2CppClass* int16_shared_enum;
    Il2CppClass* int32_shared_enum;
    Il2CppClass* int64_shared_enum;

    Il2CppClass* byte_shared_enum;
    Il2CppClass* uint16_shared_enum;
    Il2CppClass* uint32_shared_enum;
    Il2CppClass* uint64_shared_enum;
} Il2CppDefaults;

extern LIBIL2CPP_CODEGEN_API Il2CppDefaults il2cpp_defaults;

struct Il2CppClass;
struct MethodInfo;
struct FieldInfo;
struct Il2CppObject;
struct MemberInfo;

typedef struct CustomAttributesCache
{
    int count;
    Il2CppObject** attributes;
} CustomAttributesCache;

typedef void (*CustomAttributesCacheGenerator)(CustomAttributesCache*);

#ifndef THREAD_STATIC_FIELD_OFFSET
#define THREAD_STATIC_FIELD_OFFSET -1
#endif

typedef struct FieldInfo
{
    const char* name;
    const Il2CppType* type;
    Il2CppClass *parent;
    int32_t offset; // If offset is -1, then it's thread static
    uint32_t token;
} FieldInfo;

typedef struct PropertyInfo
{
    Il2CppClass *parent;
    const char *name;
    const MethodInfo *get;
    const MethodInfo *set;
    uint32_t attrs;
    uint32_t token;
} PropertyInfo;

typedef struct EventInfo
{
    const char* name;
    const Il2CppType* eventType;
    Il2CppClass* parent;
    const MethodInfo* add;
    const MethodInfo* remove;
    const MethodInfo* raise;
    uint32_t token;
} EventInfo;

typedef struct ParameterInfo
{
    const char* name;
    int32_t position;
    uint32_t token;
    const Il2CppType* parameter_type;
} ParameterInfo;

typedef void* (*InvokerMethod)(Il2CppMethodPointer, const MethodInfo*, void*, void**);

typedef enum MethodVariableKind
{
    kMethodVariableKind_This,
    kMethodVariableKind_Parameter,
    kMethodVariableKind_LocalVariable
} MethodVariableKind;

typedef enum SequencePointKind
{
    kSequencePointKind_Normal,
    kSequencePointKind_StepOut
} SequencePointKind;

typedef struct Il2CppMethodExecutionContextInfo
{
    TypeIndex typeIndex;
    int32_t nameIndex;
    int32_t scopeIndex;
} Il2CppMethodExecutionContextInfo;

typedef struct Il2CppMethodExecutionContextInfoIndex
{
    int32_t startIndex;
    int32_t count;
} Il2CppMethodExecutionContextInfoIndex;

typedef struct Il2CppMethodScope
{
    int32_t startOffset;
    int32_t endOffset;
} Il2CppMethodScope;

typedef struct Il2CppMethodHeaderInfo
{
    int32_t code_size;
    int32_t startScope;
    int32_t numScopes;
} Il2CppMethodHeaderInfo;

typedef struct Il2CppSequencePointSourceFile
{
    const char *file;
    uint8_t hash[16];
} Il2CppSequencePointSourceFile;

typedef struct Il2CppTypeSourceFilePair
{
    TypeDefinitionIndex __klassIndex;
    int32_t sourceFileIndex;
} Il2CppTypeSourceFilePair;

typedef struct Il2CppSequencePoint
{
    MethodIndex __methodDefinitionIndex;
    int32_t sourceFileIndex;
    int32_t lineStart, lineEnd;
    int32_t columnStart, columnEnd;
    int32_t ilOffset;
    SequencePointKind kind;
    int32_t isActive;
    int32_t id;
} Il2CppSequencePoint;

typedef struct Il2CppCatchPoint
{
    MethodIndex __methodDefinitionIndex;
    TypeIndex catchTypeIndex;
    int32_t ilOffset;
    int32_t tryId;
    int32_t parentTryId;
} Il2CppCatchPoint;

typedef struct Il2CppDebuggerMetadataRegistration
{
    Il2CppMethodExecutionContextInfo* methodExecutionContextInfos;
    Il2CppMethodExecutionContextInfoIndex* methodExecutionContextInfoIndexes;
    Il2CppMethodScope* methodScopes;
    Il2CppMethodHeaderInfo* methodHeaderInfos;
    Il2CppSequencePointSourceFile* sequencePointSourceFiles;
    int32_t numSequencePoints;
    Il2CppSequencePoint* sequencePoints;
    int32_t numCatchPoints;
    Il2CppCatchPoint* catchPoints;
    int32_t numTypeSourceFileEntries;
    Il2CppTypeSourceFilePair* typeSourceFiles;
    const char** methodExecutionContextInfoStrings;
} Il2CppDebuggerMetadataRegistration;

typedef union Il2CppRGCTXData
{
    void* rgctxDataDummy;
    const MethodInfo* method;
    const Il2CppType* type;
    Il2CppClass* klass;
} Il2CppRGCTXData;

typedef struct MethodInfo
{
    Il2CppMethodPointer methodPointer;
    InvokerMethod invoker_method;
    const char* name;
    Il2CppClass *klass;
    const Il2CppType *return_type;
    const ParameterInfo* parameters;

    union
    {
        const Il2CppRGCTXData* rgctx_data; /* is_inflated is true and is_generic is false, i.e. a generic instance method */
        Il2CppMetadataMethodDefinitionHandle methodMetadataHandle;
    };

    /* note, when is_generic == true and is_inflated == true the method represents an uninflated generic method on an inflated type. */
    union
    {
        const Il2CppGenericMethod* genericMethod; /* is_inflated is true */
        Il2CppMetadataGenericContainerHandle genericContainerHandle; /* is_inflated is false and is_generic is true */
        Il2CppMethodPointer nativeFunction; /* if is_marshaled_from_native is true */
    };

    uint32_t token;
    uint16_t flags;
    uint16_t iflags;
    uint16_t slot;
    uint8_t parameters_count;
    uint8_t is_generic : 1; /* true if method is a generic method definition */
    uint8_t is_inflated : 1; /* true if declaring_type is a generic instance or if method is a generic instance*/
    uint8_t wrapper_type : 1; /* always zero (MONO_WRAPPER_NONE) needed for the debugger */
    uint8_t is_marshaled_from_native : 1; /* a fake MethodInfo wrapping a native function pointer */

    // ==={{ huatuo
    void* huatuoData;
    // ===}} huatuo
} MethodInfo;

typedef struct Il2CppRuntimeInterfaceOffsetPair
{
    Il2CppClass* interfaceType;
    int32_t offset;
} Il2CppRuntimeInterfaceOffsetPair;

#if IL2CPP_COMPILER_MSVC
#pragma warning( push )
#pragma warning( disable : 4200 )
#elif defined(__clang__)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#endif

typedef struct Il2CppClass
{
    // The following fields are always valid for a Il2CppClass structure
    const Il2CppImage* image;
    void* gc_desc;
    const char* name;
    const char* namespaze;
    Il2CppType byval_arg;
    Il2CppType this_arg;
    Il2CppClass* element_class;
    Il2CppClass* castClass;
    Il2CppClass* declaringType;
    Il2CppClass* parent;
    Il2CppGenericClass *generic_class;
    Il2CppMetadataTypeHandle typeMetadataHandle; // non-NULL for Il2CppClass's constructed from type defintions
    const Il2CppInteropData* interopData;
    Il2CppClass* klass; // hack to pretend we are a MonoVTable. Points to ourself
    // End always valid fields

    // The following fields need initialized before access. This can be done per field or as an aggregate via a call to Class::Init
    FieldInfo* fields; // Initialized in SetupFields
    const EventInfo* events; // Initialized in SetupEvents
    const PropertyInfo* properties; // Initialized in SetupProperties
    const MethodInfo** methods; // Initialized in SetupMethods
    Il2CppClass** nestedTypes; // Initialized in SetupNestedTypes
    Il2CppClass** implementedInterfaces; // Initialized in SetupInterfaces
    Il2CppRuntimeInterfaceOffsetPair* interfaceOffsets; // Initialized in Init
    void* static_fields; // Initialized in Init
    const Il2CppRGCTXData* rgctx_data; // Initialized in Init
    // used for fast parent checks
    Il2CppClass** typeHierarchy; // Initialized in SetupTypeHierachy
    // End initialization required fields

    void *unity_user_data;

    uint32_t initializationExceptionGCHandle;

    uint32_t cctor_started;
    uint32_t cctor_finished;
    ALIGN_TYPE(8) size_t cctor_thread;

    // Remaining fields are always valid except where noted
    Il2CppMetadataGenericContainerHandle genericContainerHandle;
    uint32_t instance_size; // valid when size_inited is true
    uint32_t actualSize;
    uint32_t element_size;
    int32_t native_size;
    uint32_t static_fields_size;
    uint32_t thread_static_fields_size;
    int32_t thread_static_fields_offset;
    uint32_t flags;
    uint32_t token;

    uint16_t method_count; // lazily calculated for arrays, i.e. when rank > 0
    uint16_t property_count;
    uint16_t field_count;
    uint16_t event_count;
    uint16_t nested_type_count;
    uint16_t vtable_count; // lazily calculated for arrays, i.e. when rank > 0
    uint16_t interfaces_count;
    uint16_t interface_offsets_count; // lazily calculated for arrays, i.e. when rank > 0

    uint8_t typeHierarchyDepth; // Initialized in SetupTypeHierachy
    uint8_t genericRecursionDepth;
    uint8_t rank;
    uint8_t minimumAlignment; // Alignment of this type
    uint8_t naturalAligment; // Alignment of this type without accounting for packing
    uint8_t packingSize;

    // this is critical for performance of Class::InitFromCodegen. Equals to initialized && !has_initialization_error at all times.
    // Use Class::UpdateInitializedAndNoError to update
    uint8_t initialized_and_no_error : 1;

    uint8_t valuetype : 1;
    uint8_t initialized : 1;
    uint8_t enumtype : 1;
    uint8_t is_generic : 1;
    uint8_t has_references : 1; // valid when size_inited is true
    uint8_t init_pending : 1;
    uint8_t size_init_pending : 1;
    uint8_t size_inited : 1;
    uint8_t has_finalize : 1;
    uint8_t has_cctor : 1;
    uint8_t is_blittable : 1;
    uint8_t is_import_or_windows_runtime : 1;
    uint8_t is_vtable_initialized : 1;
    uint8_t has_initialization_error : 1;
    VirtualInvokeData vtable[IL2CPP_ZERO_LEN_ARRAY];
} Il2CppClass;

#if IL2CPP_COMPILER_MSVC
#pragma warning( pop )
#elif defined(__clang__)
#pragma clang diagnostic pop
#endif

// compiler calcualted values
typedef struct Il2CppTypeDefinitionSizes
{
    uint32_t instance_size;
    int32_t native_size;
    uint32_t static_fields_size;
    uint32_t thread_static_fields_size;
} Il2CppTypeDefinitionSizes;

typedef struct Il2CppDomain
{
    Il2CppAppDomain* domain;
    Il2CppAppDomainSetup* setup;
    Il2CppAppContext* default_context;
    const char* friendly_name;
    uint32_t domain_id;

    volatile int threadpool_jobs;
    void* agent_info;
} Il2CppDomain;

typedef struct Il2CppAssemblyName
{
    const char* name;
    const char* culture;
    const uint8_t* public_key;
    uint32_t hash_alg;
    int32_t hash_len;
    uint32_t flags;
    int32_t major;
    int32_t minor;
    int32_t build;
    int32_t revision;
    uint8_t public_key_token[PUBLIC_KEY_BYTE_LENGTH];
} Il2CppAssemblyName;

typedef struct Il2CppImage
{
    const char* name;
    const char *nameNoExt;
    Il2CppAssembly* assembly;

    uint32_t typeCount;
    uint32_t exportedTypeCount;
    uint32_t customAttributeCount;

    Il2CppMetadataImageHandle metadataHandle;

#ifdef __cplusplus
    mutable
#endif
    Il2CppNameToTypeHandleHashTable * nameToClassHashTable;

    const Il2CppCodeGenModule* codeGenModule;

    uint32_t token;
    uint8_t dynamic;
} Il2CppImage;

typedef struct Il2CppAssembly
{
    Il2CppImage* image;
    uint32_t token;
    int32_t referencedAssemblyStart;
    int32_t referencedAssemblyCount;
    Il2CppAssemblyName aname;
} Il2CppAssembly;

typedef struct Il2CppCodeGenOptions
{
    bool enablePrimitiveValueTypeGenericSharing;
    int maximumRuntimeGenericDepth;
} Il2CppCodeGenOptions;

typedef struct Il2CppRange
{
    int32_t start;
    int32_t length;
} Il2CppRange;

typedef struct Il2CppTokenRangePair
{
    uint32_t token;
    Il2CppRange range;
} Il2CppTokenRangePair;

typedef struct Il2CppTokenIndexMethodTuple
{
    uint32_t token;
    int32_t index;
    void** method;
    uint32_t __genericMethodIndex;
} Il2CppTokenIndexMethodTuple;

typedef struct Il2CppTokenAdjustorThunkPair
{
    uint32_t token;
    Il2CppMethodPointer adjustorThunk;
} Il2CppTokenAdjustorThunkPair;

typedef struct Il2CppWindowsRuntimeFactoryTableEntry
{
    const Il2CppType* type;
    Il2CppMethodPointer createFactoryFunction;
} Il2CppWindowsRuntimeFactoryTableEntry;

typedef struct Il2CppCodeGenModule
{
    const char* moduleName;
    const uint32_t methodPointerCount;
    const Il2CppMethodPointer* methodPointers;
    const uint32_t adjustorThunkCount;
    const Il2CppTokenAdjustorThunkPair* adjustorThunks;
    const int32_t* invokerIndices;
    const uint32_t reversePInvokeWrapperCount;
    const Il2CppTokenIndexMethodTuple* reversePInvokeWrapperIndices;
    const uint32_t rgctxRangesCount;
    const Il2CppTokenRangePair* rgctxRanges;
    const uint32_t rgctxsCount;
    const Il2CppRGCTXDefinition* rgctxs;
    const Il2CppDebuggerMetadataRegistration *debuggerMetadata;
    const CustomAttributesCacheGenerator* customAttributeCacheGenerator;
    const Il2CppMethodPointer moduleInitializer;
    TypeDefinitionIndex* staticConstructorTypeIndices;
    const Il2CppMetadataRegistration* metadataRegistration; // Per-assembly mode only
    const Il2CppCodeRegistration* codeRegistaration; // Per-assembly mode only
} Il2CppCodeGenModule;

typedef struct Il2CppCodeRegistration
{
    uint32_t reversePInvokeWrapperCount;
    const Il2CppMethodPointer* reversePInvokeWrappers;
    uint32_t genericMethodPointersCount;
    const Il2CppMethodPointer* genericMethodPointers;
    const Il2CppMethodPointer* genericAdjustorThunks;
    uint32_t invokerPointersCount;
    const InvokerMethod* invokerPointers;
    uint32_t unresolvedVirtualCallCount;
    const Il2CppMethodPointer* unresolvedVirtualCallPointers;
    uint32_t interopDataCount;
    Il2CppInteropData* interopData;
    uint32_t windowsRuntimeFactoryCount;
    Il2CppWindowsRuntimeFactoryTableEntry* windowsRuntimeFactoryTable;
    uint32_t codeGenModulesCount;
    const Il2CppCodeGenModule** codeGenModules;
} Il2CppCodeRegistration;

typedef struct Il2CppMetadataRegistration
{
    int32_t genericClassesCount;
    Il2CppGenericClass* const * genericClasses;
    int32_t genericInstsCount;
    const Il2CppGenericInst* const * genericInsts;
    int32_t genericMethodTableCount;
    const Il2CppGenericMethodFunctionsDefinitions* genericMethodTable;
    int32_t typesCount;
    const Il2CppType* const * types;
    int32_t methodSpecsCount;
    const Il2CppMethodSpec* methodSpecs;

    FieldIndex fieldOffsetsCount;
    const int32_t** fieldOffsets;

    TypeDefinitionIndex typeDefinitionsSizesCount;
    const Il2CppTypeDefinitionSizes** typeDefinitionsSizes;
    const size_t metadataUsagesCount;
    void** const* metadataUsages;
} Il2CppMetadataRegistration;

/*
* new structure to hold performance counters values that are exported
* to managed code.
* Note: never remove fields from this structure and only add them to the end.
* Size of fields and type should not be changed as well.
*/
typedef struct Il2CppPerfCounters
{
    /* JIT category */
    uint32_t jit_methods;
    uint32_t jit_bytes;
    uint32_t jit_time;
    uint32_t jit_failures;
    /* Exceptions category */
    uint32_t exceptions_thrown;
    uint32_t exceptions_filters;
    uint32_t exceptions_finallys;
    uint32_t exceptions_depth;
    uint32_t aspnet_requests_queued;
    uint32_t aspnet_requests;
    /* Memory category */
    uint32_t gc_collections0;
    uint32_t gc_collections1;
    uint32_t gc_collections2;
    uint32_t gc_promotions0;
    uint32_t gc_promotions1;
    uint32_t gc_promotion_finalizers;
    uint32_t gc_gen0size;
    uint32_t gc_gen1size;
    uint32_t gc_gen2size;
    uint32_t gc_lossize;
    uint32_t gc_fin_survivors;
    uint32_t gc_num_handles;
    uint32_t gc_allocated;
    uint32_t gc_induced;
    uint32_t gc_time;
    uint32_t gc_total_bytes;
    uint32_t gc_committed_bytes;
    uint32_t gc_reserved_bytes;
    uint32_t gc_num_pinned;
    uint32_t gc_sync_blocks;
    /* Remoting category */
    uint32_t remoting_calls;
    uint32_t remoting_channels;
    uint32_t remoting_proxies;
    uint32_t remoting_classes;
    uint32_t remoting_objects;
    uint32_t remoting_contexts;
    /* Loader category */
    uint32_t loader_classes;
    uint32_t loader_total_classes;
    uint32_t loader_appdomains;
    uint32_t loader_total_appdomains;
    uint32_t loader_assemblies;
    uint32_t loader_total_assemblies;
    uint32_t loader_failures;
    uint32_t loader_bytes;
    uint32_t loader_appdomains_uloaded;
    /* Threads and Locks category  */
    uint32_t thread_contentions;
    uint32_t thread_queue_len;
    uint32_t thread_queue_max;
    uint32_t thread_num_logical;
    uint32_t thread_num_physical;
    uint32_t thread_cur_recognized;
    uint32_t thread_num_recognized;
    /* Interop category */
    uint32_t interop_num_ccw;
    uint32_t interop_num_stubs;
    uint32_t interop_num_marshals;
    /* Security category */
    uint32_t security_num_checks;
    uint32_t security_num_link_checks;
    uint32_t security_time;
    uint32_t security_depth;
    uint32_t unused;
    /* Threadpool */
    uint64_t threadpool_workitems;
    uint64_t threadpool_ioworkitems;
    unsigned int threadpool_threads;
    unsigned int threadpool_iothreads;
} Il2CppPerfCounters;
