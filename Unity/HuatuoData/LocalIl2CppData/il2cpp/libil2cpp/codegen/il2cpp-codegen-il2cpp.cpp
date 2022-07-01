#include <string>

#include "il2cpp-config.h"
#include "il2cpp-codegen.h"

#include "utils/Exception.h"

#if !RUNTIME_TINY

#include "os/Atomic.h"
#include "vm/Class.h"
#include "vm/LastError.h"
#include "vm/ThreadPoolMs.h"
#include "vm/ThreadPool.h"
#include "vm/InternalCalls.h"
#include "vm/Reflection.h"
#include "vm/MetadataCache.h"
#include "vm/Thread.h"
#include "vm/Array.h"
#include "vm/Method.h"
#include "vm/Runtime.h"
#include "vm/Object.h"
#include "vm/MarshalAlloc.h"
#include "vm/Profiler.h"
#include "vm/Exception.h"
#include "vm/COM.h"
#include "vm/CCW.h"
#include "vm/RCW.h"
#include "vm/String.h"
#include "vm/Type.h"
#include "vm/Class.h"
#include "vm/PlatformInvoke.h"
#include "vm/WindowsRuntime.h"
#include "vm/StackTrace.h"
#include "vm/Field.h"

void* il2cpp_codegen_atomic_compare_exchange_pointer(void** dest, void* exchange, void* comparand)
{
    return il2cpp::os::Atomic::CompareExchangePointer(dest, exchange, comparand);
}

void il2cpp_codegen_marshal_store_last_error()
{
    il2cpp::vm::LastError::StoreLastError();
}

Il2CppAsyncResult* il2cpp_codegen_delegate_begin_invoke(RuntimeDelegate* delegate, void** params, RuntimeDelegate* asyncCallback, RuntimeObject* state)
{
    return il2cpp::vm::ThreadPoolMs::DelegateBeginInvoke(delegate, params, asyncCallback, state);
}

RuntimeObject* il2cpp_codegen_delegate_end_invoke(Il2CppAsyncResult* asyncResult, void **out_args)
{
    return il2cpp::vm::ThreadPoolMs::DelegateEndInvoke(asyncResult, out_args);
}

Il2CppMethodPointer il2cpp_codegen_resolve_icall(const char* name)
{
    Il2CppMethodPointer method = il2cpp::vm::InternalCalls::Resolve(name);
    if (!method)
    {
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetMissingMethodException(name));
    }
    return method;
}

Type_t* il2cpp_codegen_type_get_object(const RuntimeType* type)
{
    return (Type_t*)il2cpp::vm::Reflection::GetTypeObject(type);
}

MethodBase_t* il2cpp_codegen_get_method_object_internal(const RuntimeMethod* method, RuntimeClass* refclass)
{
    return (MethodBase_t*)il2cpp::vm::Reflection::GetMethodObject(method, method->klass);
}

Assembly_t* il2cpp_codegen_get_executing_assembly(const RuntimeMethod* method)
{
    return (Assembly_t*)il2cpp::vm::Reflection::GetAssemblyObject(method->klass->image->assembly);
}

void il2cpp_codegen_register(const Il2CppCodeRegistration* const codeRegistration, const Il2CppMetadataRegistration* const metadataRegistration, const Il2CppCodeGenOptions* const codeGenOptions)
{
    il2cpp::vm::MetadataCache::Register(codeRegistration, metadataRegistration, codeGenOptions);
}

extern MetadataInitializerCleanupFunc g_ClearMethodMetadataInitializedFlags;
void il2cpp_codegen_register_metadata_initialized_cleanup(MetadataInitializerCleanupFunc cleanup)
{
    g_ClearMethodMetadataInitializedFlags = cleanup;
}

void il2cpp_codegen_initialize_runtime_metadata(uintptr_t* metadataPointer)
{
    il2cpp::vm::MetadataCache::InitializeRuntimeMetadata(metadataPointer);
}

void* il2cpp_codegen_initialize_runtime_metadata_inline(uintptr_t* metadataPointer)
{
    return il2cpp::vm::MetadataCache::InitializeRuntimeMetadata(metadataPointer);
}

const RuntimeMethod* il2cpp_codegen_get_generic_method_definition(const RuntimeMethod* method)
{
    return il2cpp::vm::MetadataCache::GetGenericMethodDefinition(method);
}

void* il2cpp_codegen_get_thread_static_data(RuntimeClass* klass)
{
    return il2cpp::vm::Thread::GetThreadStaticData(klass->thread_static_fields_offset);
}

void il2cpp_codegen_memory_barrier()
{
    il2cpp::vm::Thread::FullMemoryBarrier();
}

RuntimeArray* SZArrayNew(RuntimeClass* arrayType, uint32_t length)
{
    return il2cpp::vm::Array::NewSpecific(arrayType, length);
}

RuntimeArray* GenArrayNew(RuntimeClass* arrayType, il2cpp_array_size_t* dimensions)
{
    return il2cpp::vm::Array::NewFull(arrayType, dimensions, NULL);
}

bool il2cpp_codegen_method_is_generic_instance(RuntimeMethod* method)
{
    return il2cpp::vm::Method::IsGenericInstance(method);
}

RuntimeClass* il2cpp_codegen_method_get_declaring_type(RuntimeMethod* method)
{
    return il2cpp::vm::Method::GetClass(method);
}

bool MethodIsStatic(const RuntimeMethod* method)
{
    return !il2cpp::vm::Method::IsInstance(method);
}

bool MethodHasParameters(const RuntimeMethod* method)
{
    return il2cpp::vm::Method::GetParamCount(method) != 0;
}

NORETURN void il2cpp_codegen_raise_profile_exception(const RuntimeMethod* method)
{
    std::string methodName = il2cpp::vm::Method::GetFullName(method);
    il2cpp_codegen_raise_exception(il2cpp_codegen_get_not_supported_exception(methodName.c_str()));
}

const RuntimeMethod* il2cpp_codegen_get_generic_virtual_method_internal(const RuntimeMethod* methodDefinition, const RuntimeMethod* inflatedMethod)
{
    return il2cpp::vm::Runtime::GetGenericVirtualMethod(methodDefinition, inflatedMethod);
}

void il2cpp_codegen_runtime_class_init(RuntimeClass* klass)
{
    il2cpp::vm::Runtime::ClassInit(klass);
}

void il2cpp_codegen_raise_execution_engine_exception(const RuntimeMethod* method)
{
    il2cpp::vm::Runtime::AlwaysRaiseExecutionEngineException(method);
}

void il2cpp_codegen_raise_execution_engine_exception_if_method_is_not_found(const RuntimeMethod* method)
{
    il2cpp::vm::Runtime::RaiseExecutionEngineExceptionIfMethodIsNotFound(method);
}

RuntimeObject* IsInst(RuntimeObject *obj, RuntimeClass* targetType)
{
    return il2cpp::vm::Object::IsInst(obj, targetType);
}

RuntimeObject* Box(RuntimeClass* type, void* data)
{
    return il2cpp::vm::Object::Box(type, data);
}

void* Unbox_internal(Il2CppObject* obj)
{
    return il2cpp::vm::Object::Unbox(obj);
}

void UnBoxNullable_internal(RuntimeObject* obj, RuntimeClass* expectedBoxedClass, void* storage)
{
    il2cpp::vm::Object::UnboxNullable(obj, expectedBoxedClass, storage);
}

RuntimeObject* il2cpp_codegen_object_new(RuntimeClass *klass)
{
    return il2cpp::vm::Object::New(klass);
}

void* il2cpp_codegen_marshal_allocate(size_t size)
{
    return il2cpp::vm::MarshalAlloc::Allocate(size);
}

#if _DEBUG

void il2cpp_codegen_marshal_allocate_push_allocation_frame()
{
    il2cpp::vm::MarshalAlloc::PushAllocationFrame();
}

void il2cpp_codegen_marshal_allocate_pop_allocation_frame()
{
    il2cpp::vm::MarshalAlloc::PopAllocationFrame();
}

bool il2cpp_codegen_marshal_allocate_has_unfreed_allocations()
{
    return il2cpp::vm::MarshalAlloc::HasUnfreedAllocations();
}

void il2cpp_codegen_marshal_allocate_clear_all_tracked_allocations()
{
    il2cpp::vm::MarshalAlloc::ClearAllTrackedAllocations();
}

#endif

#if IL2CPP_ENABLE_PROFILER

void il2cpp_codegen_profiler_method_enter(const RuntimeMethod* method)
{
    il2cpp::vm::Profiler::MethodEnter(method);
}

void il2cpp_codegen_profiler_method_exit(const RuntimeMethod* method)
{
    il2cpp::vm::Profiler::MethodExit(method);
}

#endif

NORETURN void il2cpp_codegen_raise_exception(Exception_t *ex, MethodInfo* lastManagedFrame)
{
    il2cpp::vm::Exception::Raise((RuntimeException*)ex, lastManagedFrame);
}

NORETURN void il2cpp_codegen_raise_exception(il2cpp_hresult_t hresult, bool defaultToCOMException)
{
    il2cpp::vm::Exception::Raise(hresult, defaultToCOMException);
}

NORETURN void il2cpp_codegen_raise_out_of_memory_exception()
{
    il2cpp::vm::Exception::RaiseOutOfMemoryException();
}

NORETURN void il2cpp_codegen_raise_null_reference_exception()
{
    il2cpp::vm::Exception::RaiseNullReferenceException();
}

NORETURN void il2cpp_codegen_raise_divide_by_zero_exception()
{
    il2cpp::vm::Exception::RaiseDivideByZeroException();
}

Exception_t* il2cpp_codegen_get_argument_exception(const char* param, const char* msg)
{
    return (Exception_t*)il2cpp::vm::Exception::GetArgumentException(param, msg);
}

Exception_t* il2cpp_codegen_get_argument_null_exception(const char* param)
{
    return (Exception_t*)il2cpp::vm::Exception::GetArgumentNullException(param);
}

Exception_t* il2cpp_codegen_get_overflow_exception()
{
    return (Exception_t*)il2cpp::vm::Exception::GetOverflowException("Arithmetic operation resulted in an overflow.");
}

Exception_t* il2cpp_codegen_get_not_supported_exception(const char* msg)
{
    return (Exception_t*)il2cpp::vm::Exception::GetNotSupportedException(msg);
}

Exception_t* il2cpp_codegen_get_array_type_mismatch_exception()
{
    return (Exception_t*)il2cpp::vm::Exception::GetArrayTypeMismatchException();
}

Exception_t* il2cpp_codegen_get_invalid_cast_exception(const char* msg)
{
    return (Exception_t*)il2cpp::vm::Exception::GetInvalidCastException(msg);
}

Exception_t* il2cpp_codegen_get_invalid_operation_exception(const char* msg)
{
    return (Exception_t*)il2cpp::vm::Exception::GetInvalidOperationException(msg);
}

Exception_t* il2cpp_codegen_get_marshal_directive_exception(const char* msg)
{
    return (Exception_t*)il2cpp::vm::Exception::GetMarshalDirectiveException(msg);
}

Exception_t* il2cpp_codegen_get_missing_method_exception(const char* msg)
{
    return (Exception_t*)il2cpp::vm::Exception::GetMissingMethodException(msg);
}

Exception_t* il2cpp_codegen_get_maximum_nested_generics_exception()
{
    return (Exception_t*)il2cpp::vm::Exception::GetMaxmimumNestedGenericsException();
}

Exception_t* il2cpp_codegen_get_index_out_of_range_exception()
{
    return (Exception_t*)il2cpp::vm::Exception::GetIndexOutOfRangeException();
}

Exception_t* il2cpp_codegen_get_exception(il2cpp_hresult_t hresult, bool defaultToCOMException)
{
    return (Exception_t*)il2cpp::vm::Exception::Get(hresult, defaultToCOMException);
}

void il2cpp_codegen_store_exception_info(RuntimeException* ex, String_t* exceptionString)
{
    il2cpp::vm::Exception::StoreExceptionInfo(ex, reinterpret_cast<RuntimeString*>(exceptionString));
}

void il2cpp_codegen_com_marshal_variant(RuntimeObject* obj, Il2CppVariant* variant)
{
    il2cpp::vm::COM::MarshalVariant(obj, variant);
}

RuntimeObject* il2cpp_codegen_com_marshal_variant_result(const Il2CppVariant* variant)
{
    return il2cpp::vm::COM::MarshalVariantResult(variant);
}

void il2cpp_codegen_com_destroy_variant(Il2CppVariant* variant)
{
    il2cpp::vm::COM::DestroyVariant(variant);
}

Il2CppSafeArray* il2cpp_codegen_com_marshal_safe_array(Il2CppChar type, RuntimeArray* managedArray)
{
    return il2cpp::vm::COM::MarshalSafeArray(type, managedArray);
}

RuntimeArray* il2cpp_codegen_com_marshal_safe_array_result(Il2CppChar variantType, RuntimeClass* type, Il2CppSafeArray* safeArray)
{
    return il2cpp::vm::COM::MarshalSafeArrayResult(variantType, type, safeArray);
}

Il2CppSafeArray* il2cpp_codegen_com_marshal_safe_array_bstring(RuntimeArray* managedArray)
{
    return il2cpp::vm::COM::MarshalSafeArrayBString(managedArray);
}

RuntimeArray* il2cpp_codegen_com_marshal_safe_array_bstring_result(RuntimeClass* type, Il2CppSafeArray* safeArray)
{
    return il2cpp::vm::COM::MarshalSafeArrayBStringResult(type, safeArray);
}

void il2cpp_codegen_com_destroy_safe_array(Il2CppSafeArray* safeArray)
{
    il2cpp::vm::COM::DestroySafeArray(safeArray);
}

void il2cpp_codegen_com_create_instance(const Il2CppGuid& clsid, Il2CppIUnknown** identity)
{
    il2cpp::vm::COM::CreateInstance(clsid, identity);
}

il2cpp_hresult_t il2cpp_codegen_com_handle_invalid_iproperty_conversion(const char* fromType, const char* toType)
{
    return il2cpp::vm::CCW::HandleInvalidIPropertyConversion(fromType, toType);
}

il2cpp_hresult_t il2cpp_codegen_com_handle_invalid_iproperty_conversion(RuntimeObject* value, const char* fromType, const char* toType)
{
    return il2cpp::vm::CCW::HandleInvalidIPropertyConversion(value, fromType, toType);
}

il2cpp_hresult_t il2cpp_codegen_com_handle_invalid_ipropertyarray_conversion(const char* fromArrayType, const char* fromElementType, const char* toElementType, il2cpp_array_size_t index)
{
    return il2cpp::vm::CCW::HandleInvalidIPropertyArrayConversion(fromArrayType, fromElementType, toElementType, index);
}

il2cpp_hresult_t il2cpp_codegen_com_handle_invalid_ipropertyarray_conversion(RuntimeObject* value, const char* fromArrayType, const char* fromElementType, const char* toElementType, il2cpp_array_size_t index)
{
    return il2cpp::vm::CCW::HandleInvalidIPropertyArrayConversion(value, fromArrayType, fromElementType, toElementType, index);
}

Il2CppIUnknown* il2cpp_codegen_com_get_or_create_ccw_internal(RuntimeObject* obj, const Il2CppGuid& iid)
{
    return il2cpp::vm::CCW::GetOrCreate(obj, iid);
}

Il2CppObject* il2cpp_codegen_com_unpack_ccw(Il2CppIUnknown* obj)
{
    return il2cpp::vm::CCW::Unpack(obj);
}

void il2cpp_codegen_com_register_rcw(Il2CppComObject* rcw)
{
    il2cpp::vm::RCW::Register(rcw);
}

RuntimeObject* il2cpp_codegen_com_get_or_create_rcw_from_iunknown_internal(Il2CppIUnknown* unknown, RuntimeClass* fallbackClass)
{
    return il2cpp::vm::RCW::GetOrCreateFromIUnknown(unknown, fallbackClass);
}

RuntimeObject* il2cpp_codegen_com_get_or_create_rcw_from_iinspectable_internal(Il2CppIInspectable* unknown, RuntimeClass* fallbackClass)
{
    return il2cpp::vm::RCW::GetOrCreateFromIInspectable(unknown, fallbackClass);
}

RuntimeObject* il2cpp_codegen_com_get_or_create_rcw_for_sealed_class_internal(Il2CppIUnknown* unknown, RuntimeClass* objectClass)
{
    return il2cpp::vm::RCW::GetOrCreateForSealedClass(unknown, objectClass);
}

Il2CppIUnknown* il2cpp_codegen_com_query_interface_internal(Il2CppComObject* rcw, const Il2CppGuid& guid)
{
    return il2cpp::vm::RCW::QueryInterfaceNoAddRef<true>(rcw, guid);
}

Il2CppIUnknown* il2cpp_codegen_com_query_interface_no_throw_internal(Il2CppComObject* rcw, const Il2CppGuid& guid)
{
    return il2cpp::vm::RCW::QueryInterfaceNoAddRef<false>(rcw, guid);
}

void il2cpp_codegen_com_cache_queried_interface(Il2CppComObject* rcw, const Il2CppGuid& iid, Il2CppIUnknown* queriedInterface)
{
    if (il2cpp::vm::RCW::CacheQueriedInterface(rcw, iid, queriedInterface))
        queriedInterface->AddRef();
}

void il2cpp_codegen_il2cpp_com_object_cleanup(Il2CppComObject* rcw)
{
    il2cpp::vm::RCW::Cleanup(rcw);
}

String_t* il2cpp_codegen_string_new_wrapper(const char* str)
{
    return (String_t*)il2cpp::vm::String::NewWrapper(str);
}

String_t* il2cpp_codegen_string_new_utf16(const il2cpp::utils::StringView<Il2CppChar>& str)
{
    return (String_t*)il2cpp::vm::String::NewUtf16(str.Str(), static_cast<int32_t>(str.Length()));
}

RuntimeString* il2cpp_codegen_type_append_assembly_name_if_necessary(RuntimeString* typeName, const RuntimeMethod* callingMethod)
{
    return il2cpp::vm::Type::AppendAssemblyNameIfNecessary(typeName, callingMethod);
}

Type_t* il2cpp_codegen_get_type(const RuntimeMethod* getTypeMethod, String_t* typeName, const RuntimeMethod* callingMethod)
{
    RuntimeString* assemblyQualifiedTypeName = il2cpp_codegen_type_append_assembly_name_if_necessary((RuntimeString*)typeName, callingMethod);

    // Try to find the type using a hint about about calling assembly. If it is not found, fall back to calling GetType without the hint.
    Il2CppException* exc = NULL;
    void* params[] = {assemblyQualifiedTypeName};
    Type_t* type = (Type_t*)il2cpp::vm::Runtime::Invoke(getTypeMethod, NULL, params, &exc);
    if (exc)
        il2cpp::vm::Exception::Raise(exc);
    if (type == NULL)
    {
        params[0] = typeName;
        type = (Type_t*)il2cpp::vm::Runtime::Invoke(getTypeMethod, NULL, params, &exc);
        if (exc)
            il2cpp::vm::Exception::Raise(exc);
    }
    return type;
}

Type_t* il2cpp_codegen_get_type(const RuntimeMethod* getTypeMethod, String_t* typeName, bool throwOnError, const RuntimeMethod* callingMethod)
{
    typedef Type_t* (*getTypeFuncType)(String_t*, bool);
    RuntimeString* assemblyQualifiedTypeName = il2cpp_codegen_type_append_assembly_name_if_necessary((RuntimeString*)typeName, callingMethod);

    // Try to find the type using a hint about about calling assembly. If it is not found, fall back to calling GetType without the hint.
    Il2CppException* exc = NULL;
    void* params[] = {assemblyQualifiedTypeName, &throwOnError};
    Type_t* type = (Type_t*)il2cpp::vm::Runtime::Invoke(getTypeMethod, NULL, params, &exc);
    if (exc)
        il2cpp::vm::Exception::Raise(exc);

    if (type == NULL)
    {
        params[0] = typeName;
        type = (Type_t*)il2cpp::vm::Runtime::Invoke(getTypeMethod, NULL, params, &exc);
        if (exc)
            il2cpp::vm::Exception::Raise(exc);
    }
    return type;
}

Type_t* il2cpp_codegen_get_type(const RuntimeMethod* getTypeMethod, String_t* typeName, bool throwOnError, bool ignoreCase, const RuntimeMethod* callingMethod)
{
    typedef Type_t* (*getTypeFuncType)(String_t*, bool, bool);
    RuntimeString* assemblyQualifiedTypeName = il2cpp_codegen_type_append_assembly_name_if_necessary((RuntimeString*)typeName, callingMethod);
    // Try to find the type using a hint about about calling assembly. If it is not found, fall back to calling GetType without the hint.

    Il2CppException* exc = NULL;
    void* params[] = {assemblyQualifiedTypeName, &throwOnError, &ignoreCase};
    Type_t* type = (Type_t*)il2cpp::vm::Runtime::Invoke(getTypeMethod, NULL, params, &exc);
    if (exc)
        il2cpp::vm::Exception::Raise(exc);

    if (type == NULL)
    {
        params[0] = typeName;
        type = (Type_t*)il2cpp::vm::Runtime::Invoke(getTypeMethod, NULL, params, &exc);
        if (exc)
            il2cpp::vm::Exception::Raise(exc);
    }
    return type;
}

NORETURN void RaiseInvalidCastException(RuntimeObject* obj, RuntimeClass* targetType)
{
    std::string exceptionMessage = il2cpp::utils::Exception::FormatInvalidCastException(obj->klass->element_class, targetType);
    Exception_t* exception = il2cpp_codegen_get_invalid_cast_exception(exceptionMessage.c_str());
    il2cpp_codegen_raise_exception(exception);
}

bool il2cpp_codegen_method_is_interface_method(RuntimeMethod* method)
{
    return il2cpp::vm::Class::IsInterface(il2cpp_codegen_method_get_declaring_type(method));
}

bool il2cpp_codegen_class_is_assignable_from(RuntimeClass *klass, RuntimeClass *oklass)
{
    return il2cpp::vm::Class::IsAssignableFrom(klass, oklass);
}

bool il2cpp_codegen_class_is_value_type(RuntimeClass* type)
{
    return il2cpp::vm::Class::IsValuetype(type);
}

RuntimeClass* il2cpp_codegen_inflate_generic_class(RuntimeClass* genericClassDefinition, const Il2CppGenericInst* genericInst)
{
    return il2cpp::vm::Class::GetInflatedGenericInstanceClass(genericClassDefinition, genericInst);
}

int32_t il2cpp_codgen_class_get_instance_size(RuntimeClass* klass)
{
    return il2cpp::vm::Class::GetInstanceSize(klass);
}

RuntimeClass* il2cpp_codegen_class_from_type_internal(const RuntimeType* type)
{
    return il2cpp::vm::Class::FromIl2CppType(type);
}

char* il2cpp_codegen_marshal_string(String_t* string)
{
    return il2cpp::vm::PlatformInvoke::MarshalCSharpStringToCppString((RuntimeString*)string);
}

void il2cpp_codegen_marshal_string_fixed(String_t* string, char* buffer, int numberOfCharacters)
{
    return il2cpp::vm::PlatformInvoke::MarshalCSharpStringToCppStringFixed((RuntimeString*)string, buffer, numberOfCharacters);
}

Il2CppChar* il2cpp_codegen_marshal_wstring(String_t* string)
{
    return il2cpp::vm::PlatformInvoke::MarshalCSharpStringToCppWString((RuntimeString*)string);
}

void il2cpp_codegen_marshal_wstring_fixed(String_t* string, Il2CppChar* buffer, int numberOfCharacters)
{
    return il2cpp::vm::PlatformInvoke::MarshalCSharpStringToCppWStringFixed((RuntimeString*)string, buffer, numberOfCharacters);
}

Il2CppChar* il2cpp_codegen_marshal_bstring(String_t* string)
{
    return il2cpp::vm::PlatformInvoke::MarshalCSharpStringToCppBString((RuntimeString*)string);
}

String_t* il2cpp_codegen_marshal_string_result(const char* value)
{
    return (String_t*)il2cpp::vm::PlatformInvoke::MarshalCppStringToCSharpStringResult(value);
}

String_t* il2cpp_codegen_marshal_wstring_result(const Il2CppChar* value)
{
    return (String_t*)il2cpp::vm::PlatformInvoke::MarshalCppWStringToCSharpStringResult(value);
}

String_t* il2cpp_codegen_marshal_bstring_result(const Il2CppChar* value)
{
    return (String_t*)il2cpp::vm::PlatformInvoke::MarshalCppBStringToCSharpStringResult(value);
}

void il2cpp_codegen_marshal_free_bstring(Il2CppChar* value)
{
    il2cpp::vm::PlatformInvoke::MarshalFreeBString(value);
}

char* il2cpp_codegen_marshal_empty_string_builder(StringBuilder_t* stringBuilder)
{
    return il2cpp::vm::PlatformInvoke::MarshalEmptyStringBuilder((RuntimeStringBuilder*)stringBuilder);
}

char* il2cpp_codegen_marshal_string_builder(StringBuilder_t* stringBuilder)
{
    return il2cpp::vm::PlatformInvoke::MarshalStringBuilder((RuntimeStringBuilder*)stringBuilder);
}

Il2CppChar* il2cpp_codegen_marshal_empty_wstring_builder(StringBuilder_t* stringBuilder)
{
    return il2cpp::vm::PlatformInvoke::MarshalEmptyWStringBuilder((RuntimeStringBuilder*)stringBuilder);
}

Il2CppChar* il2cpp_codegen_marshal_wstring_builder(StringBuilder_t* stringBuilder)
{
    return il2cpp::vm::PlatformInvoke::MarshalWStringBuilder((RuntimeStringBuilder*)stringBuilder);
}

void il2cpp_codegen_marshal_string_builder_result(StringBuilder_t* stringBuilder, char* buffer)
{
    il2cpp::vm::PlatformInvoke::MarshalStringBuilderResult((RuntimeStringBuilder*)stringBuilder, buffer);
}

void il2cpp_codegen_marshal_wstring_builder_result(StringBuilder_t* stringBuilder, Il2CppChar* buffer)
{
    il2cpp::vm::PlatformInvoke::MarshalWStringBuilderResult((RuntimeStringBuilder*)stringBuilder, buffer);
}

void il2cpp_codegen_marshal_free(void* ptr)
{
    il2cpp::vm::PlatformInvoke::MarshalFree(ptr);
}

Il2CppMethodPointer il2cpp_codegen_marshal_delegate(MulticastDelegate_t* d)
{
    return (Il2CppMethodPointer)il2cpp::vm::PlatformInvoke::MarshalDelegate((RuntimeDelegate*)d);
}

Il2CppDelegate* il2cpp_codegen_marshal_function_ptr_to_delegate_internal(void* functionPtr, Il2CppClass* delegateType)
{
    return il2cpp::vm::PlatformInvoke::MarshalFunctionPointerToDelegate(functionPtr, delegateType);
}

Il2CppMethodPointer il2cpp_codegen_resolve(const PInvokeArguments& pinvokeArgs)
{
    return il2cpp::vm::PlatformInvoke::Resolve(pinvokeArgs);
}

Il2CppHString il2cpp_codegen_create_hstring(String_t* str)
{
    return il2cpp::vm::WindowsRuntime::CreateHString(reinterpret_cast<RuntimeString*>(str));
}

String_t* il2cpp_codegen_marshal_hstring_result(Il2CppHString hstring)
{
    return reinterpret_cast<String_t*>(il2cpp::vm::WindowsRuntime::HStringToManagedString(hstring));
}

void il2cpp_codegen_marshal_free_hstring(Il2CppHString hstring)
{
    il2cpp::vm::WindowsRuntime::DeleteHString(hstring);
}

void il2cpp_codegen_marshal_type_to_native(Type_t* type, Il2CppWindowsRuntimeTypeName& nativeType)
{
    return il2cpp::vm::WindowsRuntime::MarshalTypeToNative(type != NULL ? reinterpret_cast<Il2CppReflectionType*>(type)->type : NULL, nativeType);
}

const Il2CppType* il2cpp_codegen_marshal_type_from_native_internal(Il2CppWindowsRuntimeTypeName& nativeType)
{
    return il2cpp::vm::WindowsRuntime::MarshalTypeFromNative(nativeType);
}

void il2cpp_codegen_delete_native_type(Il2CppWindowsRuntimeTypeName& nativeType)
{
    return il2cpp::vm::WindowsRuntime::DeleteNativeType(nativeType);
}

Il2CppIActivationFactory* il2cpp_codegen_windows_runtime_get_activation_factory(const il2cpp::utils::StringView<Il2CppNativeChar>& runtimeClassName)
{
    return il2cpp::vm::WindowsRuntime::GetActivationFactory(runtimeClassName);
}

void il2cpp_codegen_stacktrace_push_frame(Il2CppStackFrameInfo& frame)
{
    il2cpp::vm::StackTrace::PushFrame(frame);
}

void il2cpp_codegen_stacktrace_pop_frame()
{
    il2cpp::vm::StackTrace::PopFrame();
}

const char* il2cpp_codegen_get_field_data(RuntimeField* field)
{
    return il2cpp::vm::Field::GetData(field);
}

#endif // !RUNTIME_TINY

#if IL2CPP_TINY_DEBUGGER

MulticastDelegate_t* il2cpp_codegen_create_combined_delegate(Type_t* type, Il2CppArray* delegates, int delegateCount)
{
    Il2CppClass* klass = il2cpp::vm::Class::FromSystemType((Il2CppReflectionType*)type);
    Il2CppMulticastDelegate* result = reinterpret_cast<Il2CppMulticastDelegate*>(il2cpp_codegen_object_new(klass));
    result->delegates = delegates;
    result->delegateCount = delegateCount;
    return reinterpret_cast<MulticastDelegate_t*>(result);
}

Type_t* il2cpp_codegen_get_type(Il2CppObject* obj)
{
    return (Type_t*)il2cpp::vm::Reflection::GetTypeObject(&obj->klass->byval_arg);
}

Type_t* il2cpp_codegen_get_base_type(const Type_t* t)
{
    Il2CppClass* klass = il2cpp::vm::Class::FromSystemType((Il2CppReflectionType*)t);
    if (klass->parent == NULL)
        return NULL;
    return (Type_t*)il2cpp::vm::Reflection::GetTypeObject(&klass->parent->byval_arg);
}

bool il2cpp_codegen_is_assignable_from(Type_t* left, Type_t* right)
{
    return il2cpp::vm::Class::IsAssignableFrom((Il2CppReflectionType*)left, (Il2CppReflectionType*)right);
}

void il2cpp_codegen_no_reverse_pinvoke_wrapper(const char* methodName, const char* reason)
{
    std::string message = "No reverse pinvoke wrapper exists for method: '";
    message += methodName;
    message += "' because ";
    message += reason;
    il2cpp_codegen_raise_exception(il2cpp_codegen_get_invalid_operation_exception(message.c_str()));
}

bool il2cpp_codegen_type_is_interface(Type_t* t)
{
    Il2CppClass* klass = il2cpp::vm::Class::FromSystemType((Il2CppReflectionType*)t);
    return il2cpp::vm::Class::IsInterface(klass);
}

bool il2cpp_codegen_type_is_abstract(Type_t* t)
{
    Il2CppClass* klass = il2cpp::vm::Class::FromSystemType((Il2CppReflectionType*)t);
    return il2cpp::vm::Class::IsAbstract(klass);
}

bool il2cpp_codegen_type_is_pointer(Type_t* t)
{
    Il2CppClass* klass = il2cpp::vm::Class::FromSystemType((Il2CppReflectionType*)t);
    return il2cpp::vm::Class::GetType(klass)->type == IL2CPP_TYPE_PTR;
}

#endif
