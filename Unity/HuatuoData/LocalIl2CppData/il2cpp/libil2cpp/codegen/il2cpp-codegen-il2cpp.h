#pragma once

#include "il2cpp-codegen-common-small.h"
#include "il2cpp-codegen-common-big.h"
#include "il2cpp-pinvoke-support.h"
#include "icalls/mscorlib/System.Runtime.InteropServices/Marshal.h"
#include "vm-utils/icalls/mscorlib/System.Threading/Interlocked.h"
#include "vm-utils/VmThreadUtils.h"

#include "vm/ClassInlines.h"
#include "vm/ScopedThreadAttacher.h"
#include "vm/Il2CppHStringReference.h"
#include "vm/String.h"

#include <cstdlib>
#include <cstddef>

#if IL2CPP_ENABLE_PROFILER

void il2cpp_codegen_profiler_method_enter(const RuntimeMethod* method);

void il2cpp_codegen_profiler_method_exit(const RuntimeMethod* method);

#endif

struct ProfilerMethodSentry
{
    ProfilerMethodSentry(const RuntimeMethod* method)
#if IL2CPP_ENABLE_PROFILER
        : m_method(method)
#endif
    {
#if IL2CPP_ENABLE_PROFILER
        il2cpp_codegen_profiler_method_enter(m_method);
#endif
    }

    ~ProfilerMethodSentry()
    {
#if IL2CPP_ENABLE_PROFILER
        il2cpp_codegen_profiler_method_exit(m_method);
#endif
    }

private:
    const RuntimeMethod* m_method;
};

void il2cpp_codegen_stacktrace_push_frame(Il2CppStackFrameInfo& frame);

void il2cpp_codegen_stacktrace_pop_frame();

uint32_t il2cpp_codegen_sizeof(RuntimeClass* klass);

struct StackTraceSentry
{
    StackTraceSentry(const RuntimeMethod* method) : m_method(method)
    {
        Il2CppStackFrameInfo frame_info = { 0 };

        frame_info.method = method;

        il2cpp_codegen_stacktrace_push_frame(frame_info);
    }

    ~StackTraceSentry()
    {
        il2cpp_codegen_stacktrace_pop_frame();
    }

private:
    const RuntimeMethod* m_method;
};

#define IL2CPP_FAKE_BOX_SENTRY (MonitorData*)UINTPTR_MAX

template<typename T>
struct Il2CppFakeBox : RuntimeObject
{
    T m_Value;

    Il2CppFakeBox(RuntimeClass* boxedType, T* value)
    {
        klass = boxedType;
        monitor = IL2CPP_FAKE_BOX_SENTRY;
        m_Value = *value;
    }
};

struct Il2CppFakeBoxBuffer : RuntimeObject
{
    char m_Value[];

    Il2CppFakeBoxBuffer(RuntimeClass* boxedType, void* value)
    {
        klass = boxedType;
        monitor = IL2CPP_FAKE_BOX_SENTRY;
        memcpy(&m_Value, value, il2cpp_codegen_sizeof(boxedType));
    }

    static uint32_t SizeNeededFor(RuntimeClass* boxedType)
    {
        return sizeof(Il2CppFakeBoxBuffer) + il2cpp_codegen_sizeof(boxedType);
    }
};


inline bool il2cpp_codegen_is_fake_boxed_object(RuntimeObject* object)
{
    return object->monitor == IL2CPP_FAKE_BOX_SENTRY;
}

// TODO: This file should contain all the functions and type declarations needed for the generated code.
// Hopefully, we stop including everything in the generated code and know exactly what dependencies we have.
// Note that all parameter and return types should match the generated types not the runtime types.

void il2cpp_codegen_register(const Il2CppCodeRegistration* const codeRegistration, const Il2CppMetadataRegistration* const metadataRegistration, const Il2CppCodeGenOptions* const codeGenOptions);

typedef void (*MetadataInitializerCleanupFunc)();
void il2cpp_codegen_register_metadata_initialized_cleanup(MetadataInitializerCleanupFunc cleanup);

// type registration

void* il2cpp_codegen_get_thread_static_data(RuntimeClass* klass);

String_t* il2cpp_codegen_string_new_wrapper(const char* str);

String_t* il2cpp_codegen_string_new_utf16(const il2cpp::utils::StringView<Il2CppChar>& str);

Type_t* il2cpp_codegen_type_get_object(const RuntimeType* type);

NORETURN void il2cpp_codegen_raise_exception(Exception_t *ex, MethodInfo* lastManagedFrame);

NORETURN void il2cpp_codegen_rethrow_exception(Exception_t *ex);

NORETURN void il2cpp_codegen_raise_exception(il2cpp_hresult_t hresult, bool defaultToCOMException);

void il2cpp_codegen_raise_execution_engine_exception_if_method_is_not_found(const RuntimeMethod* method);

void il2cpp_codegen_raise_execution_engine_exception(const RuntimeMethod* method);
void il2cpp_codegen_raise_execution_engine_exception_missing_virtual(const RuntimeMethod* method);

NORETURN void il2cpp_codegen_raise_out_of_memory_exception();

NORETURN void il2cpp_codegen_raise_null_reference_exception();

NORETURN void il2cpp_codegen_raise_divide_by_zero_exception();

NORETURN void il2cpp_codegen_raise_index_out_of_range_exception();

Exception_t* il2cpp_codegen_get_argument_exception(const char* param, const char* msg);

Exception_t* il2cpp_codegen_get_argument_null_exception(const char* param);

Exception_t* il2cpp_codegen_get_overflow_exception();

Exception_t* il2cpp_codegen_get_not_supported_exception(const char* msg);

Exception_t* il2cpp_codegen_get_array_type_mismatch_exception();

Exception_t* il2cpp_codegen_get_invalid_cast_exception(const char* msg);

Exception_t* il2cpp_codegen_get_invalid_operation_exception(const char* msg);

Exception_t* il2cpp_codegen_get_marshal_directive_exception(const char* msg);

Exception_t* il2cpp_codegen_get_missing_method_exception(const char* msg);

Exception_t* il2cpp_codegen_get_maximum_nested_generics_exception();

Exception_t* il2cpp_codegen_get_index_out_of_range_exception();

Exception_t* il2cpp_codegen_get_exception(il2cpp_hresult_t hresult, bool defaultToCOMException);

inline RuntimeClass* il2cpp_codegen_object_class(RuntimeObject* obj)
{
    return obj->klass;
}

// OpCode.IsInst

RuntimeObject* IsInst(RuntimeObject *obj, RuntimeClass* targetType);

inline RuntimeObject* IsInstSealed(RuntimeObject *obj, RuntimeClass* targetType)
{
#if IL2CPP_DEBUG
    IL2CPP_ASSERT((targetType->flags & TYPE_ATTRIBUTE_SEALED) != 0);
    IL2CPP_ASSERT((targetType->flags & TYPE_ATTRIBUTE_INTERFACE) == 0);
#endif
    if (!obj)
        return NULL;

    // optimized version to compare sealed classes
    return (obj->klass == targetType ? obj : NULL);
}

inline RuntimeObject* IsInstClass(RuntimeObject *obj, RuntimeClass* targetType)
{
#if IL2CPP_DEBUG
    IL2CPP_ASSERT((targetType->flags & TYPE_ATTRIBUTE_INTERFACE) == 0);
#endif
    if (!obj)
        return NULL;

    // optimized version to compare classes
    return il2cpp::vm::ClassInlines::HasParentUnsafe(obj->klass, targetType) ? obj : NULL;
}

// OpCode.Castclass

NORETURN void RaiseInvalidCastException(RuntimeObject* obj, RuntimeClass* targetType);

inline RuntimeObject* Castclass(RuntimeObject *obj, RuntimeClass* targetType)
{
    if (!obj)
        return NULL;

    RuntimeObject* result = IsInst(obj, targetType);
    if (result)
        return result;

    RaiseInvalidCastException(obj, targetType);
    return NULL;
}

inline RuntimeObject* CastclassSealed(RuntimeObject *obj, RuntimeClass* targetType)
{
    if (!obj)
        return NULL;

    RuntimeObject* result = IsInstSealed(obj, targetType);
    if (result)
        return result;

    RaiseInvalidCastException(obj, targetType);
    return NULL;
}

inline RuntimeObject* CastclassClass(RuntimeObject *obj, RuntimeClass* targetType)
{
    if (!obj)
        return NULL;

    RuntimeObject* result = IsInstClass(obj, targetType);
    if (result)
        return result;

    RaiseInvalidCastException(obj, targetType);
    return NULL;
}

inline void NullCheck(void* this_ptr)
{
    if (this_ptr != NULL)
        return;

    il2cpp_codegen_raise_null_reference_exception();
}

// OpCode.Box

RuntimeObject* Box(RuntimeClass* type, void* data);

// OpCode.UnBox

void* Unbox_internal(Il2CppObject* obj);

inline void* UnBox(RuntimeObject* obj)
{
    NullCheck(obj);
    return Unbox_internal(obj);
}

inline void* UnBox(RuntimeObject* obj, RuntimeClass* expectedBoxedClass)
{
    NullCheck(obj);

    if (obj->klass->element_class == expectedBoxedClass->element_class)
        return Unbox_internal(obj);

    RaiseInvalidCastException(obj, expectedBoxedClass);
    return NULL;
}

void UnBoxNullable_internal(RuntimeObject* obj, RuntimeClass* nullableClass, void* storage);

inline void UnBoxNullable(RuntimeObject* obj, RuntimeClass* nullableClass, void* storage)
{
    // We only need to do type checks if obj is not null
    // Unboxing null nullable is perfectly valid and returns an instance that has no value
    if (obj != NULL)
    {
        if (obj->klass != nullableClass->element_class)
            RaiseInvalidCastException(obj, nullableClass->element_class);
    }

    UnBoxNullable_internal(obj, nullableClass, storage);
}

// Implements OpCode.UnBox_Any at runtime, obj may be any type, even a reference type
// unboxStorage must always be a buffer large enough to store the unboxed result
// It always returns a pointer to a data buffer - either a pointer to a value type or a pointer to a reference type
void* UnBox_Any(RuntimeObject* obj, RuntimeClass* expectedBoxedClass, void* unboxStorage);

// objBuffer is a pointer to the obj - either a pointer to a value type or a pointer to a reference type
bool il2cpp_codegen_would_box_to_non_null(RuntimeClass* klass, void* objBuffer);

int32_t il2cpp_codgen_class_get_instance_size(RuntimeClass* klass);

inline uint32_t il2cpp_codegen_sizeof(RuntimeClass* klass)
{
    if (!klass->byval_arg.valuetype)
    {
        return sizeof(void*);
    }

    return il2cpp_codgen_class_get_instance_size(klass) - sizeof(RuntimeObject);
}

inline bool il2cpp_codegen_method_is_virtual(RuntimeMethod* method)
{
    return method->slot != kInvalidIl2CppMethodSlot;
}

inline bool il2cpp_codegen_object_is_of_sealed_type(RuntimeObject* obj)
{
    IL2CPP_ASSERT(obj);
    return (obj->klass->flags & TYPE_ATTRIBUTE_SEALED) != 0;
}

bool il2cpp_codegen_method_is_generic_instance_method(RuntimeMethod* method);

inline bool il2cpp_codegen_call_method_via_invoker(const RuntimeMethod* method)
{
    return method->indirect_call_via_invokers;
}

RuntimeClass* il2cpp_codegen_method_get_declaring_type(const RuntimeMethod* method);

bool il2cpp_codegen_method_is_interface_method(RuntimeMethod* method);

inline uint16_t il2cpp_codegen_method_get_slot(const RuntimeMethod* method)
{
    return method->slot;
}

IL2CPP_FORCE_INLINE const VirtualInvokeData& il2cpp_codegen_get_virtual_invoke_data(Il2CppMethodSlot slot, const RuntimeObject* obj)
{
    Assert(slot != kInvalidIl2CppMethodSlot && "il2cpp_codegen_get_virtual_invoke_data got called on a non-virtual method");
    return obj->klass->vtable[slot];
}

IL2CPP_FORCE_INLINE const VirtualInvokeData& il2cpp_codegen_get_interface_invoke_data(Il2CppMethodSlot slot, RuntimeObject* obj, const RuntimeClass* declaringInterface)
{
    Assert(slot != kInvalidIl2CppMethodSlot && "il2cpp_codegen_get_interface_invoke_data got called on a non-virtual method");
    return il2cpp::vm::ClassInlines::GetInterfaceInvokeDataFromVTable(obj, declaringInterface, slot);
}

void il2cpp_codegen_get_generic_virtual_method_internal(const RuntimeMethod* methodDefinition, const RuntimeMethod* inflatedMethod, VirtualInvokeData* invokeData);

IL2CPP_FORCE_INLINE void il2cpp_codegen_get_generic_virtual_method(const RuntimeMethod* method, const RuntimeObject* obj, VirtualInvokeData* invokeData)
{
    uint16_t slot = method->slot;
    const RuntimeMethod* methodDefinition = obj->klass->vtable[slot].method;
    il2cpp_codegen_get_generic_virtual_method_internal(methodDefinition, method, invokeData);
}

IL2CPP_FORCE_INLINE const RuntimeMethod* il2cpp_codegen_get_generic_virtual_method(const RuntimeMethod* method, const RuntimeObject* obj)
{
    VirtualInvokeData invokeData;
    il2cpp_codegen_get_generic_virtual_method(method, obj, &invokeData);
    return invokeData.method;
}

IL2CPP_FORCE_INLINE void il2cpp_codegen_get_generic_virtual_invoke_data(const RuntimeMethod* method, const RuntimeObject* obj, VirtualInvokeData* invokeData)
{
    il2cpp_codegen_get_generic_virtual_method(method, obj , invokeData);

#if IL2CPP_DEBUG
    IL2CPP_ASSERT(invokeData->method);
#endif
}

IL2CPP_FORCE_INLINE void il2cpp_codegen_get_generic_interface_method(const RuntimeMethod* method, RuntimeObject* obj, VirtualInvokeData* invokeData)
{
    const RuntimeMethod* methodDefinition = il2cpp::vm::ClassInlines::GetInterfaceInvokeDataFromVTable(obj, method->klass, method->slot).method;
    il2cpp_codegen_get_generic_virtual_method_internal(methodDefinition, method, invokeData);
}

IL2CPP_FORCE_INLINE const RuntimeMethod* il2cpp_codegen_get_generic_interface_method(const RuntimeMethod* method, RuntimeObject* obj)
{
    VirtualInvokeData invokeData;
    il2cpp_codegen_get_generic_interface_method(method, obj, &invokeData);
    return invokeData.method;
}

IL2CPP_FORCE_INLINE void il2cpp_codegen_get_generic_interface_invoke_data(const RuntimeMethod* method, RuntimeObject* obj, VirtualInvokeData* invokeData)
{
    il2cpp_codegen_get_generic_interface_method(method, obj, invokeData);

#if IL2CPP_DEBUG
    IL2CPP_ASSERT(invokeData->method);
#endif
}

inline RuntimeClass* InitializedTypeInfo(RuntimeClass* klass)
{
    return il2cpp::vm::ClassInlines::InitFromCodegen(klass);
}

RuntimeClass* il2cpp_codegen_class_from_type_internal(const RuntimeType* type);

inline RuntimeClass* il2cpp_codegen_class_from_type(const RuntimeType *type)
{
    return InitializedTypeInfo(il2cpp_codegen_class_from_type_internal(type));
}

inline const RuntimeType* il2cpp_codegen_type_from_class(RuntimeClass *klass)
{
    return &klass->byval_arg;
}

template<typename T>
inline bool il2cpp_codegen_enum_has_flag(T enumValue, T flag)
{
    return (enumValue & flag) == flag;
}

inline void* InterlockedExchangeImplRef(void** location, void* value)
{
    return il2cpp::icalls::mscorlib::System::Threading::Interlocked::ExchangePointer(location, value);
}

template<typename T>
inline T InterlockedCompareExchangeImpl(T* location, T value, T comparand)
{
    return (T)il2cpp::icalls::mscorlib::System::Threading::Interlocked::CompareExchange_T((void**)location, value, comparand);
}

template<typename T>
inline T InterlockedExchangeImpl(T* location, T value)
{
    return (T)InterlockedExchangeImplRef((void**)location, value);
}

void il2cpp_codegen_memory_barrier();

inline void GetGenericValueImpl(RuntimeArray* thisPtr, int32_t pos, void* value)
{
    // GetGenericValueImpl is only called from the class libs internally and T is never a field
    IL2CPP_ASSERT_STACK_PTR(value);
    memcpy(value, il2cpp_array_addr_with_size(thisPtr, thisPtr->klass->element_size, pos), thisPtr->klass->element_size);
}

void SetGenericValueImpl(RuntimeArray* thisPtr, int32_t pos, void* value);

RuntimeArray* SZArrayNew(RuntimeClass* arrayType, uint32_t length);

RuntimeArray* GenArrayNew(RuntimeClass* arrayType, il2cpp_array_size_t* dimensions);

// Performance optimization as detailed here: http://blogs.msdn.com/b/clrcodegeneration/archive/2009/08/13/array-bounds-check-elimination-in-the-clr.aspx
// Since array size is a signed int32_t, a single unsigned check can be performed to determine if index is less than array size.
// Negative indices will map to a unsigned number greater than or equal to 2^31 which is larger than allowed for a valid array.
#define IL2CPP_ARRAY_BOUNDS_CHECK(index, length) \
    do { \
        if (((uint32_t)(index)) >= ((uint32_t)length)) il2cpp_codegen_raise_index_out_of_range_exception(); \
    } while (0)

bool il2cpp_codegen_class_is_assignable_from(RuntimeClass *klass, RuntimeClass *oklass);

RuntimeObject* il2cpp_codegen_object_new(RuntimeClass *klass);

Il2CppMethodPointer il2cpp_codegen_resolve_icall(const char* name);

Il2CppMethodPointer il2cpp_codegen_resolve(const PInvokeArguments& pinvokeArgs);

template<typename FunctionPointerType, size_t dynamicLibraryLength, size_t entryPointLength>
inline FunctionPointerType il2cpp_codegen_resolve_pinvoke(const Il2CppNativeChar(&nativeDynamicLibrary)[dynamicLibraryLength], const char(&entryPoint)[entryPointLength],
    Il2CppCallConvention callingConvention, Il2CppCharSet charSet, int parameterSize, bool isNoMangle)
{
    const PInvokeArguments pinvokeArgs =
    {
        il2cpp::utils::StringView<Il2CppNativeChar>(nativeDynamicLibrary),
        il2cpp::utils::StringView<char>(entryPoint),
        callingConvention,
        charSet,
        parameterSize,
        isNoMangle
    };

    return reinterpret_cast<FunctionPointerType>(il2cpp_codegen_resolve(pinvokeArgs));
}

void* il2cpp_codegen_marshal_allocate(size_t size);

template<typename T>
inline T* il2cpp_codegen_marshal_allocate_array(size_t length)
{
    return static_cast<T*>(il2cpp_codegen_marshal_allocate((il2cpp_array_size_t)(sizeof(T) * length)));
}

template<typename T>
inline T* il2cpp_codegen_marshal_allocate()
{
    return static_cast<T*>(il2cpp_codegen_marshal_allocate(sizeof(T)));
}

char* il2cpp_codegen_marshal_string(String_t* string);

void il2cpp_codegen_marshal_string_fixed(String_t* string, char* buffer, int numberOfCharacters);

Il2CppChar* il2cpp_codegen_marshal_wstring(String_t* string);

void il2cpp_codegen_marshal_wstring_fixed(String_t* string, Il2CppChar* buffer, int numberOfCharacters);

Il2CppChar* il2cpp_codegen_marshal_bstring(String_t* string);

String_t* il2cpp_codegen_marshal_string_result(const char* value);

String_t* il2cpp_codegen_marshal_wstring_result(const Il2CppChar* value);

String_t* il2cpp_codegen_marshal_bstring_result(const Il2CppChar* value);

void il2cpp_codegen_marshal_free_bstring(Il2CppChar* value);

char* il2cpp_codegen_marshal_empty_string_builder(StringBuilder_t* stringBuilder);

char* il2cpp_codegen_marshal_string_builder(StringBuilder_t* stringBuilder);

Il2CppChar* il2cpp_codegen_marshal_empty_wstring_builder(StringBuilder_t* stringBuilder);

Il2CppChar* il2cpp_codegen_marshal_wstring_builder(StringBuilder_t* stringBuilder);

void il2cpp_codegen_marshal_string_builder_result(StringBuilder_t* stringBuilder, char* buffer);

void il2cpp_codegen_marshal_wstring_builder_result(StringBuilder_t* stringBuilder, Il2CppChar* buffer);

Il2CppHString il2cpp_codegen_create_hstring(String_t* str);

String_t* il2cpp_codegen_marshal_hstring_result(Il2CppHString hstring);

void il2cpp_codegen_marshal_free_hstring(Il2CppHString hstring);

void il2cpp_codegen_marshal_type_to_native(Type_t* type, Il2CppWindowsRuntimeTypeName& nativeType);

const Il2CppType* il2cpp_codegen_marshal_type_from_native_internal(Il2CppWindowsRuntimeTypeName& nativeType);

inline Type_t* il2cpp_codegen_marshal_type_from_native(Il2CppWindowsRuntimeTypeName& nativeType)
{
    const Il2CppType* type = il2cpp_codegen_marshal_type_from_native_internal(nativeType);
    if (type == NULL)
        return NULL;

    return il2cpp_codegen_type_get_object(type);
}

void il2cpp_codegen_delete_native_type(Il2CppWindowsRuntimeTypeName& nativeType);

void il2cpp_codegen_marshal_free(void* ptr);

Il2CppMethodPointer il2cpp_codegen_marshal_delegate(MulticastDelegate_t* d);

Il2CppDelegate* il2cpp_codegen_marshal_function_ptr_to_delegate_internal(void* functionPtr, Il2CppClass* delegateType);

bool il2cpp_codegen_is_marshalled_delegate(MulticastDelegate_t* d);

#if !IL2CPP_TINY
inline void* il2cpp_codegen_get_reverse_pinvoke_function_ptr(const MulticastDelegate_t* d)
{
    return ((Il2CppDelegate*)d)->delegate_trampoline;
}

#endif

template<typename T>
inline T* il2cpp_codegen_marshal_function_ptr_to_delegate(Il2CppMethodPointer functionPtr, RuntimeClass* delegateType)
{
    return (T*)il2cpp_codegen_marshal_function_ptr_to_delegate_internal(reinterpret_cast<void*>(functionPtr), delegateType);
}

void il2cpp_codegen_marshal_store_last_error();

template<typename R, typename S>
inline R il2cpp_codegen_cast_struct(S* s)
{
    static_assert(sizeof(S) == sizeof(R), "Types with different sizes passed to il2cpp_codegen_cast_struct");
    R r;
    il2cpp_codegen_memcpy(&r, s, sizeof(R));
    return r;
}

#if _DEBUG

void il2cpp_codegen_marshal_allocate_push_allocation_frame();

void il2cpp_codegen_marshal_allocate_pop_allocation_frame();

bool il2cpp_codegen_marshal_allocate_has_unfreed_allocations();

void il2cpp_codegen_marshal_allocate_clear_all_tracked_allocations();

struct ScopedMarshallingAllocationFrame
{
    ScopedMarshallingAllocationFrame()
    {
        il2cpp_codegen_marshal_allocate_push_allocation_frame();
    }

    ~ScopedMarshallingAllocationFrame()
    {
        il2cpp_codegen_marshal_allocate_pop_allocation_frame();
    }
};

struct ScopedMarshallingAllocationCheck
{
    ~ScopedMarshallingAllocationCheck()
    {
        if (il2cpp_codegen_marshal_allocate_has_unfreed_allocations())
            il2cpp_codegen_raise_exception(il2cpp_codegen_get_invalid_operation_exception("Error in marshaling allocation. Some memory has been leaked."));
    }

private:
    ScopedMarshallingAllocationFrame m_AllocationFrame;
};

struct ScopedMarshalingAllocationClearer
{
    ~ScopedMarshalingAllocationClearer()
    {
        il2cpp_codegen_marshal_allocate_clear_all_tracked_allocations();
    }

private:
    ScopedMarshallingAllocationFrame m_AllocationFrame;
};

#endif

inline void DivideByZeroCheck(int64_t denominator)
{
    if (denominator != 0)
        return;

    il2cpp_codegen_raise_divide_by_zero_exception();
}

bool MethodIsStatic(const RuntimeMethod* method);

bool MethodHasParameters(const RuntimeMethod* method);

void il2cpp_codegen_runtime_class_init(RuntimeClass* klass);

inline void il2cpp_codegen_runtime_class_init_inline(RuntimeClass* klass)
{
    if (!klass->cctor_finished_or_no_cctor)
        il2cpp_codegen_runtime_class_init(klass);
}

// generic sharing
inline RuntimeClass* il2cpp_rgctx_data(const Il2CppRGCTXData* rgctxVar, int32_t index)
{
    return InitializedTypeInfo(rgctxVar[index].klass);
}

inline const Il2CppType* il2cpp_rgctx_type(const Il2CppRGCTXData* rgctxVar, int32_t index)
{
    return rgctxVar[index].type;
}

inline const MethodInfo* il2cpp_rgctx_method(const Il2CppRGCTXData* rgctxVar, int32_t index)
{
    return rgctxVar[index].method;
}

inline FieldInfo* il2cpp_rgctx_field(RuntimeClass* klass, int32_t index)
{
    return klass->fields + index;
}

inline bool il2cpp_rgctx_is_initialized(const RuntimeMethod* method)
{
    IL2CPP_ASSERT(method->is_inflated);
    return method->rgctx_data != NULL;
}

inline void il2cpp_rgctx_method_init(const RuntimeMethod* method)
{
    il2cpp::vm::ClassInlines::InitRgcxFromCodegen(method);
}

inline uintptr_t il2cpp_array_calc_byte_offset(RuntimeArray* runtimeArray, il2cpp_array_size_t index)
{
    return runtimeArray->klass->element_size * index;
}

inline void ArrayElementTypeCheck(RuntimeArray* array, void* value)
{
#if !IL2CPP_TINY
    if (value != NULL && IsInst((RuntimeObject*)value, array->klass->element_class) == NULL)
        il2cpp_codegen_raise_exception(il2cpp_codegen_get_array_type_mismatch_exception());
#endif
}

inline const RuntimeMethod* GetVirtualMethodInfo(RuntimeObject* pThis, Il2CppMethodSlot slot)
{
    if (!pThis)
        il2cpp_codegen_raise_null_reference_exception();

    return pThis->klass->vtable[slot].method;
}

inline const RuntimeMethod* GetInterfaceMethodInfo(RuntimeObject* pThis, Il2CppMethodSlot slot, RuntimeClass* declaringInterface)
{
    if (!pThis)
        il2cpp_codegen_raise_null_reference_exception();

    return il2cpp::vm::ClassInlines::GetInterfaceInvokeDataFromVTable(pThis, declaringInterface, slot).method;
}

void il2cpp_codegen_initialize_runtime_metadata(uintptr_t* metadataPointer);

void* il2cpp_codegen_initialize_runtime_metadata_inline(uintptr_t* metadataPointer);

inline bool il2cpp_codegen_type_is_value_type(const RuntimeType* type)
{
    return type->valuetype;
}

inline bool il2cpp_codegen_class_is_value_type(RuntimeClass* type)
{
    return il2cpp_codegen_type_is_value_type(&type->byval_arg);
}

bool il2cpp_codegen_class_is_nullable(RuntimeClass* type);

inline bool il2cpp_codegen_type_implements_virtual_method(RuntimeClass* type, const RuntimeMethod* method)
{
    IL2CPP_ASSERT(il2cpp_codegen_class_is_value_type(type));
    return method->klass == type;
}

MethodBase_t* il2cpp_codegen_get_method_object_internal(const RuntimeMethod* method, RuntimeClass* refclass);

const RuntimeClass* il2cpp_codegen_get_generic_type_definition(const RuntimeClass* klass);
const RuntimeMethod* il2cpp_codegen_get_generic_method_definition(const RuntimeMethod* method);
const RuntimeMethod* il2cpp_codegen_get_generic_instance_method_from_method_definition(RuntimeClass* genericInstanceClass, const RuntimeMethod* methodDefinition);

inline MethodBase_t* il2cpp_codegen_get_method_object(const RuntimeMethod* method)
{
    if (method->is_inflated)
        method = il2cpp_codegen_get_generic_method_definition(method);
    return il2cpp_codegen_get_method_object_internal(method, method->klass);
}

Type_t* il2cpp_codegen_get_type(String_t* typeName, const RuntimeMethod* getTypeMethod, const RuntimeMethod* callingMethod);
Type_t* il2cpp_codegen_get_type(String_t* typeName, bool throwOnError, const RuntimeMethod* getTypeMethod, const RuntimeMethod* callingMethod);
Type_t* il2cpp_codegen_get_type(String_t* typeName, bool throwOnError, bool ignoreCase, const RuntimeMethod* getTypeMethod, const RuntimeMethod* callingMethod);

Assembly_t* il2cpp_codegen_get_executing_assembly(const RuntimeMethod* method);

// Atomic

void* il2cpp_codegen_atomic_compare_exchange_pointer(void** dest, void* exchange, void* comparand);

// COM

void il2cpp_codegen_com_marshal_variant(RuntimeObject* obj, Il2CppVariant* variant);

RuntimeObject* il2cpp_codegen_com_marshal_variant_result(const Il2CppVariant* variant);

void il2cpp_codegen_com_destroy_variant(Il2CppVariant* variant);

Il2CppSafeArray* il2cpp_codegen_com_marshal_safe_array(Il2CppChar type, RuntimeArray* managedArray);

RuntimeArray* il2cpp_codegen_com_marshal_safe_array_result(Il2CppChar variantType, RuntimeClass* type, Il2CppSafeArray* safeArray);

Il2CppSafeArray* il2cpp_codegen_com_marshal_safe_array_bstring(RuntimeArray* managedArray);

RuntimeArray* il2cpp_codegen_com_marshal_safe_array_bstring_result(RuntimeClass* type, Il2CppSafeArray* safeArray);

void il2cpp_codegen_com_destroy_safe_array(Il2CppSafeArray* safeArray);

void il2cpp_codegen_com_create_instance(const Il2CppGuid& clsid, Il2CppIUnknown** identity);

void il2cpp_codegen_com_register_rcw(Il2CppComObject* rcw);

RuntimeObject* il2cpp_codegen_com_get_or_create_rcw_from_iunknown_internal(Il2CppIUnknown* unknown, RuntimeClass* fallbackClass);

template<typename T>
inline T* il2cpp_codegen_com_get_or_create_rcw_from_iunknown(Il2CppIUnknown* unknown, RuntimeClass* fallbackClass)
{
    return static_cast<T*>(il2cpp_codegen_com_get_or_create_rcw_from_iunknown_internal(unknown, fallbackClass));
}

RuntimeObject* il2cpp_codegen_com_get_or_create_rcw_from_iinspectable_internal(Il2CppIInspectable* unknown, RuntimeClass* fallbackClass);

template<typename T>
inline T* il2cpp_codegen_com_get_or_create_rcw_from_iinspectable(Il2CppIInspectable* unknown, RuntimeClass* fallbackClass)
{
    return static_cast<T*>(il2cpp_codegen_com_get_or_create_rcw_from_iinspectable_internal(unknown, fallbackClass));
}

RuntimeObject* il2cpp_codegen_com_get_or_create_rcw_for_sealed_class_internal(Il2CppIUnknown* unknown, RuntimeClass* objectClass);

template<typename T>
inline T* il2cpp_codegen_com_get_or_create_rcw_for_sealed_class(Il2CppIUnknown* unknown, RuntimeClass* objectClass)
{
    return static_cast<T*>(il2cpp_codegen_com_get_or_create_rcw_for_sealed_class_internal(unknown, objectClass));
}

Il2CppIUnknown* il2cpp_codegen_com_query_interface_internal(Il2CppComObject* rcw, const Il2CppGuid& guid);
Il2CppIUnknown* il2cpp_codegen_com_query_interface_no_throw_internal(Il2CppComObject* rcw, const Il2CppGuid& guid);
void il2cpp_codegen_com_cache_queried_interface(Il2CppComObject* rcw, const Il2CppGuid& guid, Il2CppIUnknown* queriedInterface);

template<typename T>
inline T* il2cpp_codegen_com_query_interface(Il2CppComObject* rcw)
{
    return static_cast<T*>(il2cpp_codegen_com_query_interface_internal(rcw, T::IID));
}

template<typename T>
inline T* il2cpp_codegen_com_query_interface_no_throw(Il2CppComObject* rcw)
{
    return static_cast<T*>(il2cpp_codegen_com_query_interface_no_throw_internal(rcw, T::IID));
}

void il2cpp_codegen_il2cpp_com_object_cleanup(Il2CppComObject* rcw);

Il2CppIUnknown* il2cpp_codegen_com_get_or_create_ccw_internal(RuntimeObject* obj, const Il2CppGuid& iid);

template<typename InterfaceType>
inline InterfaceType* il2cpp_codegen_com_get_or_create_ccw(RuntimeObject* obj)
{
    return static_cast<InterfaceType*>(il2cpp_codegen_com_get_or_create_ccw_internal(obj, InterfaceType::IID));
}

inline intptr_t il2cpp_codegen_com_get_iunknown_for_object(RuntimeObject* obj)
{
    return reinterpret_cast<intptr_t>(il2cpp_codegen_com_get_or_create_ccw_internal(obj, Il2CppIUnknown::IID));
}

Il2CppObject* il2cpp_codegen_com_unpack_ccw(Il2CppIUnknown* obj);

inline void il2cpp_codegen_com_raise_exception_if_failed(il2cpp_hresult_t hr, bool defaultToCOMException)
{
    // Copied from il2cpp::vm::Exception::RaiseIfFailed to keep inlined
    if (IL2CPP_HR_FAILED(hr))
        il2cpp_codegen_raise_exception(hr, defaultToCOMException);
}

inline RuntimeException* il2cpp_codegen_com_get_exception(il2cpp_hresult_t hr, bool defaultToCOMException)
{
    return (RuntimeException*)il2cpp_codegen_get_exception(hr, defaultToCOMException);
}

il2cpp_hresult_t il2cpp_codegen_com_handle_invalid_iproperty_conversion(const char* fromType, const char* toType);

il2cpp_hresult_t il2cpp_codegen_com_handle_invalid_iproperty_conversion(RuntimeObject* value, const char* fromType, const char* toType);

il2cpp_hresult_t il2cpp_codegen_com_handle_invalid_ipropertyarray_conversion(const char* fromArrayType, const char* fromElementType, const char* toElementType, il2cpp_array_size_t index);

il2cpp_hresult_t il2cpp_codegen_com_handle_invalid_ipropertyarray_conversion(RuntimeObject* value, const char* fromArrayType, const char* fromElementType, const char* toElementType, il2cpp_array_size_t index);

void il2cpp_codegen_store_exception_info(RuntimeException* ex, String_t* exceptionString);

Il2CppIActivationFactory* il2cpp_codegen_windows_runtime_get_activation_factory(const il2cpp::utils::StringView<Il2CppNativeChar>& runtimeClassName);

// delegate

Il2CppAsyncResult* il2cpp_codegen_delegate_begin_invoke(RuntimeDelegate* delegate, void** params, RuntimeDelegate* asyncCallback, RuntimeObject* state);

RuntimeObject* il2cpp_codegen_delegate_end_invoke(Il2CppAsyncResult* asyncResult, void **out_args);

void il2cpp_codegen_set_closed_delegate_invoke(RuntimeObject* delegate, RuntimeObject* target, void* methodPtr);

inline const Il2CppGenericInst* il2cpp_codegen_get_generic_class_inst(RuntimeClass* genericClass)
{
    IL2CPP_ASSERT(genericClass->generic_class);
    return genericClass->generic_class->context.class_inst;
}

RuntimeClass* il2cpp_codegen_get_generic_argument(RuntimeClass* klass, uint32_t argNum);

// Inflate a generic class from an existing generic instance
RuntimeClass* il2cpp_codegen_inflate_generic_class(RuntimeClass* genericClassDefinition, const Il2CppGenericInst* genericInst);

// Inflate a generic class from types
RuntimeClass* il2cpp_codegen_inflate_generic_class(RuntimeClass* genericClassDefinition, const RuntimeType*, /*const RuntimeType*, const RuntimeType* */ ...);

inline void* il2cpp_codegen_static_fields_for(RuntimeClass* klass)
{
    return klass->static_fields;
}

void il2cpp_codegen_assert_field_size(RuntimeField* field, size_t size);

void* il2cpp_codegen_get_instance_field_data_pointer(void* instance, RuntimeField* field);
inline void* il2cpp_codegen_get_instance_field_data_pointer(intptr_t instance, RuntimeField* field)
{
    return il2cpp_codegen_get_instance_field_data_pointer((void*)instance, field);
}

inline void* il2cpp_codegen_get_instance_field_data_pointer(uintptr_t instance, RuntimeField* field)
{
    return il2cpp_codegen_get_instance_field_data_pointer((void*)instance, field);
}

void il2cpp_codegen_write_instance_field_data(void* instance, RuntimeField* field, void* data, uint32_t size);
inline void il2cpp_codegen_write_instance_field_data(intptr_t instance, RuntimeField* field, void* data, uint32_t size)
{
    il2cpp_codegen_write_instance_field_data((void*)instance, field, data, size);
}

inline void il2cpp_codegen_write_instance_field_data(uintptr_t instance, RuntimeField* field, void* data, uint32_t size)
{
    il2cpp_codegen_write_instance_field_data((void*)instance, field, data, size);
}

void* il2cpp_codegen_get_static_field_data_pointer(RuntimeField* field);
void il2cpp_codegen_write_static_field_data(RuntimeField* field, void* data, uint32_t size);
void* il2cpp_codegen_get_thread_static_field_data_pointer(RuntimeField* field);
void il2cpp_codegen_write_thread_static_field_data(RuntimeField* field, void* data, uint32_t size);

template<typename T>
void il2cpp_codegen_write_instance_field_data(void* instance, RuntimeField* field, T data)
{
    il2cpp_codegen_assert_field_size(field, sizeof(T));

    void* fieldPointer = il2cpp_codegen_get_instance_field_data_pointer(instance, field);
    *(T*)fieldPointer = data;
    Il2CppCodeGenWriteBarrierForType(field->type, (void**)fieldPointer, NULL);
}

template<typename T>
inline void il2cpp_codegen_write_instance_field_data(intptr_t instance, RuntimeField* field, T data)
{
    il2cpp_codegen_write_instance_field_data((void*)instance, field, data);
}

template<typename T>
inline void il2cpp_codegen_write_instance_field_data(uintptr_t instance, RuntimeField* field, T data)
{
    il2cpp_codegen_write_instance_field_data((void*)instance, field, data);
}

template<typename T>
void il2cpp_codegen_write_static_field_data(RuntimeField* field, T data)
{
    il2cpp_codegen_assert_field_size(field, sizeof(T));

    void* fieldPointer = il2cpp_codegen_get_static_field_data_pointer(field);
    *(T*)fieldPointer = data;
    Il2CppCodeGenWriteBarrierForType(field->type, (void**)fieldPointer, NULL);
}

template<typename T>
void il2cpp_codegen_write_thread_static_field_data(RuntimeField* field, T data)
{
    il2cpp_codegen_assert_field_size(field, sizeof(T));

    void* fieldPointer = il2cpp_codegen_get_thread_static_field_data_pointer(field);
    *(T*)fieldPointer = data;
    Il2CppCodeGenWriteBarrierForType(field->type, (void**)fieldPointer, NULL);
}

inline Il2CppMethodPointer il2cpp_codegen_get_method_pointer(const RuntimeMethod* method)
{
    return method->virtualMethodPointer;
}

inline Il2CppMethodPointer il2cpp_codegen_get_direct_method_pointer(const RuntimeMethod* method)
{
    return method->methodPointer;
}

Il2CppMethodPointer il2cpp_codegen_get_virtual_call_method_pointer(const RuntimeMethod* method);

inline const RuntimeType* il2cpp_codegen_method_return_type(const RuntimeMethod* method)
{
    return method->return_type;
}

inline int il2cpp_codegen_method_parameter_count(const RuntimeMethod* method)
{
    return method->parameters_count;
}

inline const RuntimeType* il2cpp_codegen_method_parameter_type(const RuntimeMethod* method, int parameterIndex)
{
    IL2CPP_ASSERT(parameterIndex >= 0 && parameterIndex < method->parameters_count);
    return method->parameters[parameterIndex];
}

inline bool il2cpp_codegen_is_import_or_windows_runtime(const RuntimeObject *object)
{
    return object->klass->is_import_or_windows_runtime;
}

inline intptr_t il2cpp_codegen_get_com_interface_for_object(Il2CppObject* object, Type_t* type)
{
    return il2cpp::icalls::mscorlib::System::Runtime::InteropServices::Marshal::GetCCW(object, reinterpret_cast<Il2CppReflectionType*>(type));
}

NORETURN void il2cpp_codegen_raise_profile_exception(const RuntimeMethod* method);

const char* il2cpp_codegen_get_field_data(RuntimeField* field);

void il2cpp_codegen_array_unsafe_mov(RuntimeClass * destClass, void* dest, RuntimeClass * srcClass, void* src);
NORETURN void il2cpp_codegen_array_unsafe_mov_type_exception(const RuntimeType * destType, const RuntimeType* srcType);

template<typename TDest>
inline void il2cpp_codegen_array_unsafe_mov_primitive(const RuntimeType * destType, TDest* dest, const RuntimeType* srcType, void* src)
{
    switch (srcType->type)
    {
        case IL2CPP_TYPE_BOOLEAN:
            *dest = (TDest)(*(bool *)src);
            break;
        case IL2CPP_TYPE_I1:
            *dest = (TDest)(*(int8_t *)src);
            break;
        case IL2CPP_TYPE_U1:
            *dest = (TDest)(*(uint8_t *)src);
            break;
        case IL2CPP_TYPE_I2:
            *dest = (TDest)(*(int16_t *)src);
            break;
        case IL2CPP_TYPE_CHAR:
        case IL2CPP_TYPE_U2:
            *dest = (TDest)(*(uint16_t *)src);
            break;
        case IL2CPP_TYPE_I4:
            *dest = (TDest)(*(int32_t *)src);
            break;
        case IL2CPP_TYPE_U4:
            *dest = (TDest)(*(uint32_t *)src);
            break;
        case IL2CPP_TYPE_I8:
            *dest = (TDest)(*(int64_t *)src);
            break;
        case IL2CPP_TYPE_U8:
            *dest = (TDest)(*(uint64_t *)src);
            break;
        case IL2CPP_TYPE_I:
            *dest = (TDest)(*(intptr_t *)src);
            break;
        case IL2CPP_TYPE_U:
            *dest = (TDest)(*(uintptr_t *)src);
            break;
        default:
            il2cpp_codegen_array_unsafe_mov_type_exception(destType, srcType);
    }
}

// objBuffer is a pointer to the obj, either a pointer to a struct's data or a pointer to a reference type pointer
void il2cpp_codegen_runtime_constrained_call(RuntimeClass* type, const RuntimeMethod* constrainedMethod, void* boxBuffer, void* objBuffer, void** args, void* retVal);

void* il2cpp_codegen_runtime_box_constrained_this(RuntimeClass* type, const RuntimeMethod* constrainedMethod, void* obj);

template<typename T>
inline void* il2cpp_codegen_unsafe_cast(T* ptr)
{
    return reinterpret_cast<void*>(ptr);
}

inline void il2cpp_codegen_by_reference_constructor(Il2CppByReference* byReference, void* value)
{
    IL2CPP_ASSERT_STACK_PTR(byReference);
    byReference->value = (intptr_t)value;
}

inline intptr_t il2cpp_codegen_by_reference_get_value(Il2CppByReference* byReference)
{
    return byReference->value;
}

#define IL2CPP_BY_REFERENCE_GET_VALUE(TReturnType, byReference) (TReturnType*)il2cpp_codegen_by_reference_get_value(byReference)

bool il2cpp_codegen_is_reference_or_contains_references(const RuntimeMethod* method);

bool il2cpp_codegen_is_unmanaged(const RuntimeMethod* method);

#if IL2CPP_TINY

// Add intrinsics used by Tiny.

#include "utils/MemoryUtils.h"

Type_t* il2cpp_codegen_get_type(Il2CppObject* obj);

inline int32_t il2cpp_codegen_get_array_length(Il2CppArray* genArray, int32_t dimension)
{
    if (genArray->bounds == NULL)
        return il2cpp_codegen_get_array_length(genArray);

    return static_cast<int32_t>(genArray->bounds[dimension].length);
}

MulticastDelegate_t* il2cpp_codegen_create_combined_delegate(Type_t* type, Il2CppArray* delegates, int delegateCount);

inline String_t* il2cpp_codegen_marshal_ptr_to_string_ansi(intptr_t ptr)
{
    return il2cpp_codegen_marshal_string_result(reinterpret_cast<const char*>(ptr));
}

inline intptr_t il2cpp_codegen_marshal_string_to_co_task_mem_ansi(String_t* ptr)
{
    return reinterpret_cast<intptr_t>(il2cpp_codegen_marshal_string(ptr));
}

inline void il2cpp_codegen_marshal_string_free_co_task_mem(intptr_t ptr)
{
    il2cpp_codegen_marshal_free(reinterpret_cast<void*>(ptr));
}

struct Delegate_t;

inline String_t* il2cpp_codegen_string_new_length(int length)
{
    return reinterpret_cast<String_t*>(il2cpp::vm::String::NewSize(length));
}

Type_t* il2cpp_codegen_get_base_type(const Type_t* t);

bool il2cpp_codegen_is_assignable_from(Type_t* left, Type_t* right);

template<typename T>
struct Il2CppReversePInvokeMethodHolder
{
    Il2CppReversePInvokeMethodHolder(T** storageAddress) :
        m_LastValue(*storageAddress),
        m_StorageAddress(storageAddress)
    {
    }

    ~Il2CppReversePInvokeMethodHolder()
    {
        *m_StorageAddress = m_LastValue;
    }

private:
    T* const m_LastValue;
    T** const m_StorageAddress;
};

void il2cpp_codegen_no_reverse_pinvoke_wrapper(const char* methodName, const char* reason);

bool il2cpp_codegen_type_is_interface(Type_t* t);
bool il2cpp_codegen_type_is_abstract(Type_t* t);
bool il2cpp_codegen_type_is_pointer(Type_t* t);

NORETURN void il2cpp_codegen_raise_exception(const char* message);

#endif

template<typename T>
inline T* il2cpp_span_get_item(T* refPtrValue, int32_t index, int32_t length)
{
    IL2CPP_ARRAY_BOUNDS_CHECK(index, length);
    return &refPtrValue[index];
}

template<typename T>
inline T* il2cpp_unsafe_unbox(RuntimeObject* obj, RuntimeClass* klass)
{
    return reinterpret_cast<T*>(UnBox(obj, klass));
}
