#pragma once
#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"

#include "Baselib.h"
#include "vm/Array.h"
#include "vm/Type.h"
#include "vm/Runtime.h"
#include "icalls/mscorlib/System/Type.h"
#include "icalls/mscorlib/System/MonoType.h"

#if IL2CPP_BYTE_ORDER != IL2CPP_LITTLE_ENDIAN
#error "only support litten endian"
#endif

#if	PLATFORM_ARCH_64 != 1
#error "only support 64bit"
#endif


#ifndef ENABLE_PLACEHOLDER_DLL
#define ENABLE_PLACEHOLDER_DLL 1
#endif

#define HUATUO_UNITY_2020_OR_NEW

#define IS_CLASS_VALUE_TYPE(klass) ((klass)->valuetype)
#define IS_CCTOR_FINISH_OR_NO_CCTOR(klass) ((klass)->cctor_finished) || !((klass)->has_cctor)
#define GET_METHOD_PARAMETER_TYPE(param) param.parameter_type
#define GET_ARRAY_ELEMENT_ADDRESS load_array_elema
#define GET_CUSTOM_ATTRIBUTE_TYPE_RANGE_START(tr) ((tr).start)

#define SET_IL2CPPTYPE_VALUE_TYPE(type, v) 
#define GET_IL2CPPTYPE_VALUE_TYPE(type)

#define VALUE_TYPE_METHOD_POINTER_IS_ADJUST_METHOD 1

//#define ADJUST_VALUE_TYPE_THIS_POINTER(newPtr, oldPtr) newPtr = oldPtr - 1
//// #define RECOVERY_VALUE_TYPE_THIS_POINTER(newPtr, oldPtr) newPtr = oldPtr + 1
//#define CHECK_UNADJUST_VALUE_TYPE_THIS_POINTER(klass, ptr)

namespace huatuo
{

	inline Il2CppReflectionType* GetReflectionTypeFromName(Il2CppString* name)
	{
		return il2cpp::icalls::mscorlib::System::Type::internal_from_name(name, true, false);
	}

	inline void ConstructDelegate(Il2CppDelegate* delegate, Il2CppObject* target, const MethodInfo* method)
	{
		il2cpp::vm::Type::ConstructDelegate(delegate, target, method->methodPointer, method);
	}

	inline const MethodInfo* GetGenericVirtualMethod(const MethodInfo* result, const MethodInfo* inflateMethod)
	{
		return il2cpp::vm::Runtime::GetGenericVirtualMethod(result, inflateMethod);
	}

	inline void InitNullableValueType(void* nullableValueTypeObj, void* data, Il2CppClass* klass)
	{
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		std::memmove(nullableValueTypeObj, data, size);
		*((uint8_t*)nullableValueTypeObj + size) = 1;
	}

	inline void NewNullableValueType(void* nullableValueTypeObj, void* data, Il2CppClass* klass)
	{
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		std::memmove(nullableValueTypeObj, data, size);
		*((uint8_t*)nullableValueTypeObj + size) = 1;
	}

	inline bool IsNullableHasValue(void* nullableValueObj, Il2CppClass* klass)
	{
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		return *((uint8_t*)nullableValueObj + size);
	}

	inline void GetNullableValueOrDefault(void* dst, void* nullableValueObj, Il2CppClass* klass)
	{
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		if (*((uint8_t*)nullableValueObj + size))
		{
			std::memmove(dst, nullableValueObj, size);
		}
		else
		{
			std::memset(dst, 0, size);
		}
	}

	inline void GetNullableValueOrDefault(void* dst, void* nullableValueObj, void* defaultData, Il2CppClass* klass)
	{
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		std::memmove(dst, *((uint8_t*)nullableValueObj + size) ? nullableValueObj : defaultData, size);
	}

	inline void GetNullableValue(void* dst, void* nullableValueObj, Il2CppClass* klass)
	{
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		if (*((uint8_t*)nullableValueObj + size))
		{
			std::memmove(dst, nullableValueObj, size);
		}
		else
		{
			il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Nullable object must have a value."));
		}
	}

	inline Il2CppString* GetKlassFullName(const Il2CppType* type)
	{
		Il2CppReflectionType* refType = il2cpp::icalls::mscorlib::System::Type::internal_from_handle((intptr_t)type);
		return il2cpp::icalls::mscorlib::System::MonoType::getFullName(refType, false, false);
	}
}