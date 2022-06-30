#pragma once

#include "il2cpp-config.h"

#if !RUNTIME_TINY

#include <stdint.h>
#include <stddef.h>
#include "il2cpp-class-internals.h"
#include "il2cpp-windowsruntime-types.h"

typedef struct Il2CppClass Il2CppClass;
typedef struct MethodInfo MethodInfo;
typedef struct PropertyInfo PropertyInfo;
typedef struct FieldInfo FieldInfo;
typedef struct EventInfo EventInfo;
typedef struct Il2CppType Il2CppType;
typedef struct Il2CppAssembly Il2CppAssembly;
typedef struct Il2CppException Il2CppException;
typedef struct Il2CppImage Il2CppImage;
typedef struct Il2CppDomain Il2CppDomain;
typedef struct Il2CppString Il2CppString;
typedef struct Il2CppReflectionMethod Il2CppReflectionMethod;
typedef struct Il2CppAsyncCall Il2CppAsyncCall;
typedef struct Il2CppIUnknown Il2CppIUnknown;
typedef struct Il2CppWaitHandle Il2CppWaitHandle;
typedef struct MonitorData MonitorData;

#ifdef __cplusplus
namespace il2cpp
{
namespace os
{
    class Thread;
}
}

namespace baselib
{
#if !IL2CPP_TINY || IL2CPP_TINY_FROM_IL2CPP_BUILDER
    inline namespace il2cpp_baselib
{
#endif
    class ReentrantLock;
#if !IL2CPP_TINY || IL2CPP_TINY_FROM_IL2CPP_BUILDER
}
#endif
}
#endif //__cplusplus

typedef struct Il2CppReflectionAssembly Il2CppReflectionAssembly;

typedef Il2CppClass Il2CppVTable;
typedef struct Il2CppObject
{
    union
    {
        Il2CppClass *klass;
        Il2CppVTable *vtable;
    };
    MonitorData *monitor;
} Il2CppObject;

typedef int32_t il2cpp_array_lower_bound_t;
#define IL2CPP_ARRAY_MAX_INDEX ((int32_t) 0x7fffffff)
#define IL2CPP_ARRAY_MAX_SIZE  ((uint32_t) 0xffffffff)

typedef struct Il2CppArrayBounds
{
    il2cpp_array_size_t length;
    il2cpp_array_lower_bound_t lower_bound;
} Il2CppArrayBounds;

#if IL2CPP_COMPILER_MSVC
#pragma warning( push )
#pragma warning( disable : 4200 )
#elif defined(__clang__)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#endif

//Warning: Updates to this struct must also be made to IL2CPPArraySize C code
#ifdef __cplusplus
typedef struct Il2CppArray : public Il2CppObject
{
#else
typedef struct Il2CppArray
{
    Il2CppObject obj;
#endif //__cplusplus
    /* bounds is NULL for szarrays */
    Il2CppArrayBounds *bounds;
    /* total number of elements of the array */
    il2cpp_array_size_t max_length;
} Il2CppArray;

#ifdef __cplusplus
typedef struct Il2CppArraySize : public Il2CppArray
{
#else
//mono code has no inheritance, so its members must be available from this type
typedef struct Il2CppArraySize
{
    Il2CppObject obj;
    Il2CppArrayBounds *bounds;
    il2cpp_array_size_t max_length;
#endif //__cplusplus
    ALIGN_TYPE(8) void* vector[IL2CPP_ZERO_LEN_ARRAY];
} Il2CppArraySize;

static const size_t kIl2CppSizeOfArray = (offsetof(Il2CppArraySize, vector));
static const size_t kIl2CppOffsetOfArrayBounds = (offsetof(Il2CppArray, bounds));
static const size_t kIl2CppOffsetOfArrayLength = (offsetof(Il2CppArray, max_length));

#define il2cpp_array_addr_with_size(arr, idx, size) ((((uint8_t*)(arr)) + kIl2CppSizeOfArray) + ((size_t)(size) * (idx)))


// System.String
typedef struct Il2CppString
{
    Il2CppObject object;
    int32_t length;                             ///< Length of string *excluding* the trailing null (which is included in 'chars').
    Il2CppChar chars[IL2CPP_ZERO_LEN_ARRAY];
} Il2CppString;

#if IL2CPP_COMPILER_MSVC
#pragma warning( pop )
#elif defined(__clang__)
#pragma clang diagnostic pop
#endif

typedef struct Il2CppReflectionType
{
    Il2CppObject object;
    const Il2CppType *type;
} Il2CppReflectionType;

// IMPORTANT: All managed types corresponding to the objects below must be blacklisted in mscorlib.xml

// System.RuntimeType
typedef struct Il2CppReflectionRuntimeType
{
    Il2CppReflectionType type;
    Il2CppObject* type_info;
    Il2CppObject* genericCache;
    Il2CppObject* serializationCtor;
} Il2CppReflectionRuntimeType;

// System.MonoType
typedef struct Il2CppReflectionMonoType
{
    Il2CppReflectionRuntimeType type;

#ifdef __cplusplus
    const Il2CppType* GetIl2CppType() const
    {
        return type.type.type;
    }

#endif //__cplusplus
} Il2CppReflectionMonoType;

// System.Reflection.EventInfo
typedef struct Il2CppReflectionEvent
{
    Il2CppObject object;
    Il2CppObject *cached_add_event;
} Il2CppReflectionEvent;

// System.Reflection.MonoEvent
typedef struct Il2CppReflectionMonoEvent
{
    Il2CppReflectionEvent event;
    Il2CppReflectionType* reflectedType;
    const EventInfo* eventInfo;
} Il2CppReflectionMonoEvent;

// System.Reflection.MonoEventInfo
typedef struct Il2CppReflectionMonoEventInfo
{
    Il2CppReflectionType* declaringType;
    Il2CppReflectionType* reflectedType;
    Il2CppString* name;
    Il2CppReflectionMethod* addMethod;
    Il2CppReflectionMethod* removeMethod;
    Il2CppReflectionMethod* raiseMethod;
    uint32_t eventAttributes;
    Il2CppArray* otherMethods;
} Il2CppReflectionMonoEventInfo;

// System.Reflection.MonoField
typedef struct Il2CppReflectionField
{
    Il2CppObject object;
    Il2CppClass *klass;
    FieldInfo *field;
    Il2CppString *name;
    Il2CppReflectionType *type;
    uint32_t attrs;
} Il2CppReflectionField;

// System.Reflection.MonoProperty
typedef struct Il2CppReflectionProperty
{
    Il2CppObject object;
    Il2CppClass *klass;
    const PropertyInfo *property;
} Il2CppReflectionProperty;

// System.Reflection.MonoMethod
typedef struct Il2CppReflectionMethod
{
    Il2CppObject object;
    const MethodInfo *method;
    Il2CppString *name;
    Il2CppReflectionType *reftype;
} Il2CppReflectionMethod;

// System.Reflection.MonoGenericMethod
typedef struct Il2CppReflectionGenericMethod
{
    Il2CppReflectionMethod base;
} Il2CppReflectionGenericMethod;

// System.Reflection.MonoMethodInfo
typedef struct Il2CppMethodInfo
{
    Il2CppReflectionType *parent;
    Il2CppReflectionType *ret;
    uint32_t attrs;
    uint32_t implattrs;
    uint32_t callconv;
} Il2CppMethodInfo;

// System.Reflection.MonoPropertyInfo
typedef struct Il2CppPropertyInfo
{
    Il2CppReflectionType* parent;
    Il2CppReflectionType* declaringType;
    Il2CppString *name;
    Il2CppReflectionMethod *get;
    Il2CppReflectionMethod *set;
    uint32_t attrs;
} Il2CppPropertyInfo;

// System.Reflection.ParameterInfo
typedef struct Il2CppReflectionParameter
{
    Il2CppObject object;
    uint32_t AttrsImpl;
    Il2CppReflectionType *ClassImpl;
    Il2CppObject *DefaultValueImpl;
    Il2CppObject *MemberImpl;
    Il2CppString *NameImpl;
    int32_t PositionImpl;
    Il2CppObject* MarshalAs;
} Il2CppReflectionParameter;

// System.Reflection.RuntimeModule
typedef struct Il2CppReflectionModule
{
    Il2CppObject obj;
    const Il2CppImage* image;
    Il2CppReflectionAssembly* assembly;
    Il2CppString* fqname;
    Il2CppString* name;
    Il2CppString* scopename;
    bool is_resource;
    uint32_t token;
} Il2CppReflectionModule;

// System.Reflection.AssemblyName
typedef struct Il2CppReflectionAssemblyName
{
    Il2CppObject  obj;
    Il2CppString *name;
    Il2CppString *codebase;
    int32_t major, minor, build, revision;
    Il2CppObject  *cultureInfo;
    uint32_t     flags;
    uint32_t     hashalg;
    Il2CppObject  *keypair;
    Il2CppArray   *publicKey;
    Il2CppArray   *keyToken;
    uint32_t     versioncompat;
    Il2CppObject *version;
    uint32_t     processor_architecture;
    uint32_t contentType;
} Il2CppReflectionAssemblyName;

// System.RuntimeAssembly
typedef struct Il2CppReflectionAssembly
{
    Il2CppObject object;
    const Il2CppAssembly *assembly;
    /* CAS related */
    Il2CppObject *evidence; /* Evidence */
    Il2CppObject *resolve_event_holder;
    Il2CppObject *minimum;  /* PermissionSet - for SecurityAction.RequestMinimum */
    Il2CppObject *optional; /* PermissionSet - for SecurityAction.RequestOptional */
    Il2CppObject *refuse;   /* PermissionSet - for SecurityAction.RequestRefuse */
    Il2CppObject *granted;  /* PermissionSet - for the resolved assembly granted permissions */
    Il2CppObject *denied;   /* PermissionSet - for the resolved assembly denied permissions */
    /* */
    bool from_byte_array;
    Il2CppString *name;
} Il2CppReflectionAssembly;

// System.Reflection.Emit.UnmanagedMarshal
typedef struct Il2CppReflectionMarshal
{
    Il2CppObject object;
    int32_t count;
    int32_t type;
    int32_t eltype;
    Il2CppString* guid;
    Il2CppString* mcookie;
    Il2CppString* marshaltype;
    Il2CppObject* marshaltyperef;
    int32_t param_num;
    bool has_size;
} Il2CppReflectionMarshal;

// System.Reflection.Pointer
typedef struct Il2CppReflectionPointer
{
    Il2CppObject object;
    void* data;
    Il2CppReflectionType* type;
} Il2CppReflectionPointer;

typedef struct Il2CppThreadName
{
    Il2CppChar* chars;
    int32_t unused;
    int32_t length;
} Il2CppThreadName;

typedef struct
{
    uint32_t ref;
    void (*destructor)(void* data);
} Il2CppRefCount;

/* Data owned by a MonoInternalThread that must live until both the finalizer
 * for MonoInternalThread has run, and the underlying machine thread has
 * detached.
 *
 * Normally a thread is first detached and then the InternalThread object is
 * finalized and collected.  However during shutdown, when the root domain is
 * finalized, all the InternalThread objects are finalized first and the
 * machine threads are detached later.
 */
typedef struct
{
    Il2CppRefCount ref;
#ifdef __cplusplus
    baselib::ReentrantLock* synch_cs;
#else
    void* synch_cs;
#endif
} Il2CppLongLivedThreadData;

// System.Threading.InternalThread
typedef struct Il2CppInternalThread
{
    Il2CppObject obj;
    int lock_thread_id;
#ifdef __cplusplus
    il2cpp::os::Thread* handle;
#else
    void* handle;
#endif //__cplusplus
    void* native_handle;
    Il2CppThreadName name;
    uint32_t state;
    Il2CppObject* abort_exc;
    int abort_state_handle;
    uint64_t tid;
    intptr_t debugger_thread;
    void** static_data;
    void* runtime_thread_info;
    Il2CppObject* current_appcontext;
    Il2CppObject* root_domain_thread;
    Il2CppArray* _serialized_principal;
    int _serialized_principal_version;
    void* appdomain_refs;
    int32_t interruption_requested;
#ifdef __cplusplus
    Il2CppLongLivedThreadData *longlived;
#else
    void* longlived;
#endif //__cplusplus
    bool threadpool_thread;
    bool thread_interrupt_requested;
    int stack_size;
    uint8_t apartment_state;
    int critical_region_level;
    int managed_id;
    uint32_t small_id;
    void* manage_callback;

    intptr_t flags;
    void* thread_pinning_ref;
    void* abort_protected_block_count;
    int32_t priority;
    void* owned_mutexes;
    void * suspended;
    int32_t self_suspended;
    size_t thread_state;
    void* unused[3];  // same size as netcore
    void* last;
} Il2CppInternalThread;

/* Keep in sync with System.IOSelectorJob in mcs/class/System/System/IOSelectorJob.cs */
typedef struct Il2CppIOSelectorJob
{
    Il2CppObject object;
    int32_t operation;
    Il2CppObject *callback;
    Il2CppObject *state;
} Il2CppIOSelectorJob;

/* This is a copy of System.Runtime.Remoting.Messaging.CallType */
typedef enum
{
    Il2Cpp_CallType_Sync = 0,
    Il2Cpp_CallType_BeginInvoke = 1,
    Il2Cpp_CallType_EndInvoke = 2,
    Il2Cpp_CallType_OneWay = 3
} Il2CppCallType;

typedef struct Il2CppMethodMessage
{
    Il2CppObject obj;
    Il2CppReflectionMethod *method;
    Il2CppArray  *args;
    Il2CppArray  *names;
    Il2CppArray  *arg_types;
    Il2CppObject *ctx;
    Il2CppObject *rval;
    Il2CppObject *exc;
    Il2CppAsyncResult *async_result;
    uint32_t        call_type;
} Il2CppMethodMessage;

/* This is a copy of System.AppDomainSetup */
typedef struct Il2CppAppDomainSetup
{
    Il2CppObject object;
    Il2CppString* application_base;
    Il2CppString* application_name;
    Il2CppString* cache_path;
    Il2CppString* configuration_file;
    Il2CppString* dynamic_base;
    Il2CppString* license_file;
    Il2CppString* private_bin_path;
    Il2CppString* private_bin_path_probe;
    Il2CppString* shadow_copy_directories;
    Il2CppString* shadow_copy_files;
    uint8_t publisher_policy;
    uint8_t path_changed;
    int loader_optimization;
    uint8_t disallow_binding_redirects;
    uint8_t disallow_code_downloads;
    Il2CppObject* activation_arguments; /* it is System.Object in 1.x, ActivationArguments in 2.0 */
    Il2CppObject* domain_initializer;
    Il2CppObject* application_trust; /* it is System.Object in 1.x, ApplicationTrust in 2.0 */
    Il2CppArray* domain_initializer_args;
    uint8_t disallow_appbase_probe;
    Il2CppArray* configuration_bytes;
    Il2CppArray* serialized_non_primitives;
} Il2CppAppDomainSetup;


// System.Threading.Thread
typedef struct Il2CppThread
{
    Il2CppObject  obj;
    Il2CppInternalThread* internal_thread;
    Il2CppObject* start_obj;
    Il2CppException* pending_exception;
    Il2CppObject* principal;
    int32_t principal_version;
    Il2CppDelegate* delegate;
    Il2CppObject* executionContext;
    bool executionContextBelongsToOuterScope;

#ifdef __cplusplus
    Il2CppInternalThread* GetInternalThread() const
    {
        return internal_thread;
    }

#endif //__cplusplus
} Il2CppThread;

#ifdef __cplusplus
// System.Exception
typedef struct Il2CppException : public Il2CppObject
{
#else
typedef struct Il2CppException
{
    Il2CppObject object;
#endif //__cplusplus
#if !IL2CPP_TINY
    Il2CppString* className;
    Il2CppString* message;
    Il2CppObject* _data;
    Il2CppException* inner_ex;
    Il2CppString* _helpURL;
    Il2CppArray* trace_ips;
    Il2CppString* stack_trace;
    Il2CppString* remote_stack_trace;
    int remote_stack_index;
    Il2CppObject* _dynamicMethods;
    il2cpp_hresult_t hresult;
    Il2CppString* source;
    Il2CppObject* safeSerializationManager;
    Il2CppArray* captured_traces;
    Il2CppArray* native_trace_ips;
    int32_t caught_in_unmanaged;
#else
    Il2CppString* message;
    union
    {
        // Stack trace is the field at this position,
        // but we'll define inner_ex and hresult to reduce the number of defines we need in vm::Exception.cpp
        Il2CppString* stack_trace;
        Il2CppException* inner_ex;
        il2cpp_hresult_t hresult;
    };
#endif
} Il2CppException;

// System.SystemException
typedef struct Il2CppSystemException
{
    Il2CppException base;
} Il2CppSystemException;

// System.ArgumentException
typedef struct Il2CppArgumentException
{
    Il2CppException base;
    Il2CppString *argName;
} Il2CppArgumentException;

// System.TypedReference
typedef struct Il2CppTypedRef
{
    const Il2CppType *type;
    void*  value;
    Il2CppClass *klass;
} Il2CppTypedRef;

// System.Delegate
typedef struct Il2CppDelegate
{
    Il2CppObject object;
#if !IL2CPP_TINY
    /* The compiled code of the target method */
    Il2CppMethodPointer method_ptr;
    /* The invoke code */
    Il2CppMethodPointer invoke_impl;
    Il2CppObject *target;
    const MethodInfo *method;

    // This is used in PlatformInvoke.cpp to store the native function pointer
    // IMPORTANT: It is assumed to NULL otherwise!  See PlatformInvoke::IsFakeDelegateMethodMarshaledFromNativeCode
    void* delegate_trampoline;

    // Used to store the mulicast_invoke_impl
    intptr_t extraArg;

    /* MONO:
     * If non-NULL, this points to a memory location which stores the address of
     * the compiled code of the method, or NULL if it is not yet compiled.
     * uint8_t **method_code;
     */
    // IL2CPP: Points to the "this" method pointer we use when calling invoke_impl
    // For closed delegates invoke_impl_this points to target and invoke_impl is method pointer so we just do a single indirect call
    // For all other delegates invoke_impl_this is points to it's owning delegate an invoke_impl is a delegate invoke stub
    // NOTE: This field is NOT VISIBLE to the GC because its not a managed field in the classlibs
    //       Our usages are safe becuase we either pointer to ourself or whats stored in the target field
    Il2CppObject* invoke_impl_this;

    void* interp_method;
    /* Interp method that is executed when invoking the delegate */
    void* interp_invoke_impl;
    Il2CppReflectionMethod *method_info;
    Il2CppReflectionMethod *original_method_info;
    Il2CppObject *data;

    bool method_is_virtual;
#else
    void* method_ptr;
    Il2CppObject* m_target;
    void* invoke_impl;
    void* multicast_invoke_impl;
    void* m_ReversePInvokeWrapperPtr;
    bool m_IsDelegateOpen;
#endif // !IL2CPP_TINY
} Il2CppDelegate;

typedef struct Il2CppMulticastDelegate
{
    Il2CppDelegate delegate;
    Il2CppArray *delegates;
#if IL2CPP_TINY
    int delegateCount;
#endif
} Il2CppMulticastDelegate;

// System.MarshalByRefObject
typedef struct Il2CppMarshalByRefObject
{
    Il2CppObject obj;
    Il2CppObject *identity;
} Il2CppMarshalByRefObject;

#ifdef __cplusplus
struct QICache
{
    const Il2CppGuid* iid;
    Il2CppIUnknown* qiResult;
};

// System.__Il2CppComObject (dummy type that replaces System.__ComObject)
struct Il2CppComObject : Il2CppObject
{
    Il2CppIUnknown* identity;

    QICache qiShortCache[8];
    QICache* qiLongCache;
    int32_t qiShortCacheSize;
    int32_t qiLongCacheSize;
    int32_t qiLongCacheCapacity;

    // Same native object can be marshaled to managed code several times. If that happens,
    // we have to marshal it to the same RCW (same Il2CppComObject). We use a map of
    // IUnknown pointer -> weak GC handles to achieve it, and that works. When managed code
    // stops referencing the RCW, GC just garbage collects it and the finalizer will clean it
    // from our map. So far so good, eh?
    //
    // Enter Marshal.ReleaseComObject. This beast is designed to release the underlying COM object,
    // but ONLY after we used N amount of times (where N is the amount of times we marshaled
    // IUnknown into Il2CppComObject). In order to make it work, we need to implement ref counting.
    // This ref count gets incremented each time we marshal IUnknown to Il2CppComObject,
    // and gets decremented when Marshal.ReleaseComObject gets called. Fortunately, since we
    // live in a world of fairies and garbage collectors, we don't actually have to release it
    // manually in order for it to get cleaned up automatically in the future.
    int32_t refCount;
};
#endif //__cplusplus

// Fully Shared GenericTypes
// Il2CppFullySharedGenericAny comes from a generic paramter - it can by any type
// Il2CppFullySharedGenericStruct comes from a generic struct - e.g. struct MyStruct<T> {}.  We don't know it's size - it's a void*
// Fully shared classes will inherit from System.Object
typedef void* Il2CppFullySharedGenericAny;
typedef void* Il2CppFullySharedGenericStruct;

// System.AppDomain
typedef struct Il2CppAppDomain
{
    Il2CppMarshalByRefObject mbr;
    Il2CppDomain *data;
} Il2CppAppDomain;

// System.Diagnostics.StackFrame
typedef struct Il2CppStackFrame
{
    Il2CppObject obj;
    int32_t il_offset;
    int32_t native_offset;
    uint64_t methodAddress;
    uint32_t methodIndex;
    Il2CppReflectionMethod *method;
    Il2CppString *filename;
    int32_t line;
    int32_t column;
    Il2CppString *internal_method_name;
} Il2CppStackFrame;

// System.Globalization.DateTimeFormatInfo
typedef struct Il2CppDateTimeFormatInfo
{
    Il2CppObject obj;
    Il2CppObject* CultureData;
    Il2CppString* Name;
    Il2CppString* LangName;
    Il2CppObject* CompareInfo;
    Il2CppObject* CultureInfo;
    Il2CppString* AMDesignator;
    Il2CppString* PMDesignator;
    Il2CppString* DateSeparator;
    Il2CppString* GeneralShortTimePattern;
    Il2CppString* GeneralLongTimePattern;
    Il2CppString* TimeSeparator;
    Il2CppString* MonthDayPattern;
    Il2CppString* DateTimeOffsetPattern;
    Il2CppObject* Calendar;
    uint32_t FirstDayOfWeek;
    uint32_t CalendarWeekRule;
    Il2CppString* FullDateTimePattern;
    Il2CppArray* AbbreviatedDayNames;
    Il2CppArray* ShortDayNames;
    Il2CppArray* DayNames;
    Il2CppArray* AbbreviatedMonthNames;
    Il2CppArray* MonthNames;
    Il2CppArray* GenitiveMonthNames;
    Il2CppArray* GenitiveAbbreviatedMonthNames;
    Il2CppArray* LeapYearMonthNames;
    Il2CppString* LongDatePattern;
    Il2CppString* ShortDatePattern;
    Il2CppString* YearMonthPattern;
    Il2CppString* LongTimePattern;
    Il2CppString* ShortTimePattern;
    Il2CppArray* YearMonthPatterns;
    Il2CppArray* ShortDatePatterns;
    Il2CppArray* LongDatePatterns;
    Il2CppArray* ShortTimePatterns;
    Il2CppArray* LongTimePatterns;
    Il2CppArray* EraNames;
    Il2CppArray* AbbrevEraNames;
    Il2CppArray* AbbrevEnglishEraNames;
    Il2CppArray* OptionalCalendars;
    bool readOnly;
    int32_t FormatFlags;
    int32_t CultureID;
    bool UseUserOverride;
    bool UseCalendarInfo;
    int32_t DataItem;
    bool IsDefaultCalendar;
    Il2CppArray* DateWords;
    Il2CppString* FullTimeSpanPositivePattern;
    Il2CppString* FullTimeSpanNegativePattern;
    Il2CppArray* dtfiTokenHash;
} Il2CppDateTimeFormatInfo;

// System.Globalization.NumberFormatInfo
typedef struct Il2CppNumberFormatInfo
{
    Il2CppObject obj;
    Il2CppArray* numberGroupSizes;
    Il2CppArray* currencyGroupSizes;
    Il2CppArray* percentGroupSizes;
    Il2CppString* positiveSign;
    Il2CppString* negativeSign;
    Il2CppString* numberDecimalSeparator;
    Il2CppString* numberGroupSeparator;
    Il2CppString* currencyGroupSeparator;
    Il2CppString* currencyDecimalSeparator;
    Il2CppString* currencySymbol;
    Il2CppString* ansiCurrencySymbol;
    Il2CppString* naNSymbol;
    Il2CppString* positiveInfinitySymbol;
    Il2CppString* negativeInfinitySymbol;
    Il2CppString* percentDecimalSeparator;
    Il2CppString* percentGroupSeparator;
    Il2CppString* percentSymbol;
    Il2CppString* perMilleSymbol;
    Il2CppArray* nativeDigits;
    int dataItem;
    int numberDecimalDigits;
    int currencyDecimalDigits;
    int currencyPositivePattern;
    int currencyNegativePattern;
    int numberNegativePattern;
    int percentPositivePattern;
    int percentNegativePattern;
    int percentDecimalDigits;
    int digitSubstitution;
    bool readOnly;
    bool useUserOverride;
    bool isInvariant;
    bool validForParseAsNumber;
    bool validForParseAsCurrency;
} Il2CppNumberFormatInfo;

typedef struct NumberFormatEntryManaged
{
    int32_t currency_decimal_digits;
    int32_t currency_decimal_separator;
    int32_t currency_group_separator;
    int32_t currency_group_sizes0;
    int32_t currency_group_sizes1;
    int32_t currency_negative_pattern;
    int32_t currency_positive_pattern;
    int32_t currency_symbol;
    int32_t nan_symbol;
    int32_t negative_infinity_symbol;
    int32_t negative_sign;
    int32_t number_decimal_digits;
    int32_t number_decimal_separator;
    int32_t number_group_separator;
    int32_t number_group_sizes0;
    int32_t number_group_sizes1;
    int32_t number_negative_pattern;
    int32_t per_mille_symbol;
    int32_t percent_negative_pattern;
    int32_t percent_positive_pattern;
    int32_t percent_symbol;
    int32_t positive_infinity_symbol;
    int32_t positive_sign;
} NumberFormatEntryManaged;

typedef struct Il2CppCultureData
{
    Il2CppObject obj;
    Il2CppString *AMDesignator;
    Il2CppString *PMDesignator;
    Il2CppString *TimeSeparator;
    Il2CppArray *LongTimePatterns;
    Il2CppArray *ShortTimePatterns;
    uint32_t FirstDayOfWeek;
    uint32_t CalendarWeekRule;
} Il2CppCultureData;

typedef struct Il2CppCalendarData
{
    Il2CppObject obj;
    Il2CppString *NativeName;
    Il2CppArray *ShortDatePatterns;
    Il2CppArray *YearMonthPatterns;
    Il2CppArray *LongDatePatterns;
    Il2CppString *MonthDayPattern;

    Il2CppArray *EraNames;
    Il2CppArray *AbbreviatedEraNames;
    Il2CppArray *AbbreviatedEnglishEraNames;
    Il2CppArray *DayNames;
    Il2CppArray *AbbreviatedDayNames;
    Il2CppArray *SuperShortDayNames;
    Il2CppArray *MonthNames;
    Il2CppArray *AbbreviatedMonthNames;
    Il2CppArray *GenitiveMonthNames;
    Il2CppArray *GenitiveAbbreviatedMonthNames;
} Il2CppCalendarData;

// System.Globalization.CultureInfo
typedef struct Il2CppCultureInfo
{
    Il2CppObject obj;
    bool is_read_only;
    int32_t lcid;
    int32_t parent_lcid;

    int32_t datetime_index;
    int32_t number_index;

    int32_t default_calendar_type;

    bool use_user_override;
    Il2CppNumberFormatInfo* number_format;
    Il2CppDateTimeFormatInfo* datetime_format;
    Il2CppObject* textinfo;
    Il2CppString* name;

    Il2CppString* englishname;
    Il2CppString* nativename;
    Il2CppString* iso3lang;
    Il2CppString* iso2lang;

    Il2CppString* win3lang;
    Il2CppString* territory;

    Il2CppArray* native_calendar_names;

    Il2CppString* compareinfo;

    const void* text_info_data;

    int dataItem;
    Il2CppObject* calendar;
    Il2CppObject* parent_culture;
    bool constructed;
    Il2CppArray* cached_serialized_form;
    Il2CppObject* cultureData;
    bool isInherited;
} Il2CppCultureInfo;

// System.Globalization.RegionInfo
typedef struct Il2CppRegionInfo
{
    Il2CppObject obj;
    int32_t geo_id;
    Il2CppString* iso2name;
    Il2CppString* iso3name;
    Il2CppString* win3name;
    Il2CppString* english_name;
    Il2CppString* native_name;
    Il2CppString* currency_symbol;
    Il2CppString* iso_currency_symbol;
    Il2CppString* currency_english_name;
    Il2CppString* currency_native_name;
} Il2CppRegionInfo;

// System.Runtime.InteropServices.SafeHandle
// Inherited by Microsoft.Win32.SafeHandles.SafeWaitHandle
typedef struct Il2CppSafeHandle
{
    Il2CppObject base;
    void* handle;

    int state;
    bool owns_handle;
    bool fullyInitialized;
} Il2CppSafeHandle;

// System.Text.StringBuilder
typedef struct Il2CppStringBuilder Il2CppStringBuilder;
typedef struct Il2CppStringBuilder
{
    Il2CppObject object;

    Il2CppArray* chunkChars;
    Il2CppStringBuilder* chunkPrevious;
    int chunkLength;
    int chunkOffset;
    int maxCapacity;
} Il2CppStringBuilder;

// System.Net.SocketAddress
typedef struct Il2CppSocketAddress
{
    Il2CppObject base;
    int m_Size;
    Il2CppArray* data;
    bool m_changed;
    int m_hash;
} Il2CppSocketAddress;

// System.Globalization.SortKey
typedef struct Il2CppSortKey
{
    Il2CppObject base;
    Il2CppString *str;
    Il2CppArray *key;
    int32_t options;
    int32_t lcid;
} Il2CppSortKey;

// System.Runtime.InteropServices.ErrorWrapper
typedef struct Il2CppErrorWrapper
{
    Il2CppObject base;
    int32_t errorCode;
} Il2CppErrorWrapper;

// System.Runtime.Remoting.Messaging.AsyncResult
typedef struct Il2CppAsyncResult
{
    Il2CppObject base;
    Il2CppObject *async_state;
    Il2CppWaitHandle *handle;
    Il2CppDelegate *async_delegate;
    void* data; // We pass delegate arguments here. This is repurposed. Depends on Mono C# code not using the field.
    Il2CppAsyncCall *object_data;
    bool  sync_completed;
    bool  completed;
    bool  endinvoke_called;
    Il2CppObject *async_callback;
    Il2CppObject *execution_context;
    Il2CppObject *original_context;
} Il2CppAsyncResult;

// System.MonoAsyncCall
typedef struct Il2CppAsyncCall
{
    Il2CppObject base;

    Il2CppMethodMessage *msg;

    MethodInfo *cb_method; // We don't set this.
    Il2CppDelegate *cb_target; // We pass the actual delegate here.
    Il2CppObject *state;
    Il2CppObject *res;
    Il2CppArray *out_args;
} Il2CppAsyncCall;

typedef struct Il2CppExceptionWrapper Il2CppExceptionWrapper;
typedef struct Il2CppExceptionWrapper
{
    Il2CppException* ex;
#ifdef __cplusplus
    Il2CppExceptionWrapper(Il2CppException* ex) : ex(ex) {}
#endif //__cplusplus
} Il2CppExceptionWrapper;

typedef struct Il2CppIOAsyncResult
{
    Il2CppObject base;
    Il2CppDelegate* callback;
    Il2CppObject* state;
    Il2CppWaitHandle* wait_handle;
    bool completed_synchronously;
    bool completed;
} Il2CppIOAsyncResult;

/// Corresponds to Mono's internal System.Net.Sockets.Socket.SocketAsyncResult
/// class. Has no relation to Il2CppAsyncResult.
typedef struct Il2CppSocketAsyncResult
{
    Il2CppIOAsyncResult base;
    Il2CppObject* socket;
    int32_t operation;
    Il2CppException* delayedException;
    Il2CppObject* endPoint;
    Il2CppArray* buffer;
    int32_t offset;
    int32_t size;
    int32_t socket_flags;
    Il2CppObject* acceptSocket;
    Il2CppArray* addresses;
    int32_t port;
    Il2CppObject* buffers;
    bool reuseSocket;
    int32_t currentAddress;
    Il2CppObject* acceptedSocket;
    int32_t total;
    int32_t error;
    int32_t endCalled;
} Il2CppSocketAsyncResult;

typedef enum Il2CppResourceLocation
{
    IL2CPP_RESOURCE_LOCATION_EMBEDDED = 1,
    IL2CPP_RESOURCE_LOCATION_ANOTHER_ASSEMBLY = 2,
    IL2CPP_RESOURCE_LOCATION_IN_MANIFEST = 4
} Il2CppResourceLocation;

// System.Reflection.ManifestResourceInfo
typedef struct Il2CppManifestResourceInfo
{
    Il2CppObject object;
    Il2CppReflectionAssembly* assembly;
    Il2CppString* filename;
    uint32_t location;
} Il2CppManifestResourceInfo;

#define IL2CPP_CHECK_ARG_NULL(arg)  do {    \
    if (arg == NULL)    \
    {   \
        il2cpp::vm::Exception::Raise (il2cpp::vm::Exception::GetArgumentNullException (#arg));  \
    };  } while (0)

// System.Runtime.Remoting.Contexts.Context
typedef struct Il2CppAppContext
{
    Il2CppObject obj;
    int32_t domain_id;
    int32_t context_id;
    void* static_data;
} Il2CppAppContext;

typedef struct Il2CppDecimal
{
    // Decimal.cs treats the first two shorts as one long
    // And they seriable the data so we need to little endian
    // seriliazation
    // The wReserved overlaps with Variant's vt member
#if IL2CPP_BYTE_ORDER == IL2CPP_BIG_ENDIAN
    union
    {
        struct
        {
            uint8_t sign;
            uint8_t scale;
        } u;
        uint16_t signscale;
    } u;
    uint16_t reserved;
#else
    uint16_t reserved;
    union
    {
        struct
        {
            uint8_t scale;
            uint8_t sign;
        } u;
        uint16_t signscale;
    } u;
#endif
    uint32_t Hi32;
    union
    {
        struct
        {
            uint32_t Lo32;
            uint32_t Mid32;
        } v;
        uint64_t Lo64;
    } v;
} Il2CppDecimal;

// Structure to access an encoded double floating point
typedef struct Il2CppDouble
{
#if IL2CPP_BYTE_ORDER == IL2CPP_BIG_ENDIAN
    uint32_t sign : 1;
    uint32_t exp : 11;
    uint32_t mantHi : 20;
    uint32_t mantLo : 32;
#else // BIGENDIAN
    uint32_t mantLo : 32;
    uint32_t mantHi : 20;
    uint32_t exp : 11;
    uint32_t sign : 1;
#endif
} Il2CppDouble;

typedef union Il2CppDouble_double
{
    Il2CppDouble s;
    double d;
} Il2CppDouble_double;

typedef enum Il2CppDecimalCompareResult
{
    IL2CPP_DECIMAL_CMP_LT = -1,
    IL2CPP_DECIMAL_CMP_EQ,
    IL2CPP_DECIMAL_CMP_GT
} Il2CppDecimalCompareResult;

// Structure to access an encoded single floating point
typedef struct Il2CppSingle
{
#if IL2CPP_BYTE_ORDER == IL2CPP_BIG_ENDIAN
    uint32_t sign : 1;
    uint32_t exp : 8;
    uint32_t mant : 23;
#else
    uint32_t mant : 23;
    uint32_t exp : 8;
    uint32_t sign : 1;
#endif
} Il2CppSingle;

typedef union Il2CppSingle_float
{
    Il2CppSingle s;
    float f;
} Il2CppSingle_float;

// System.ByReference
typedef struct Il2CppByReference
{
    intptr_t value;
} Il2CppByReference;

#endif // !RUNTIME_TINY
