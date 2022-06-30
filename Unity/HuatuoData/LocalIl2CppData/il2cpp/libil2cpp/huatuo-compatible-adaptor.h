#pragma once
#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"

#include "Baselib.h"
#include "icalls/mscorlib/System/Type.h"
#include "icalls/mscorlib/System/RuntimeType.h"
#include "icalls/mscorlib/System/RuntimeTypeHandle.h"
#include "vm/Array.h"
#include "vm/Type.h"
#include "vm/Runtime.h"

#if IL2CPP_BYTE_ORDER != IL2CPP_LITTLE_ENDIAN
#error "only support litten endian"
#endif

#if	PLATFORM_ARCH_64 != 1
#error "only support 64bit"
#endif


#ifndef ENABLE_PLACEHOLDER_DLL
#define ENABLE_PLACEHOLDER_DLL 1
#endif

#define HUATUO_UNITY_2021_OR_NEW

#define IS_CLASS_VALUE_TYPE(klass) ((klass)->byval_arg.valuetype)
#define IS_CCTOR_FINISH_OR_NO_CCTOR(klass) ((klass)->cctor_finished_or_no_cctor)
#define GET_METHOD_PARAMETER_TYPE(param) param
#define GET_ARRAY_ELEMENT_ADDRESS il2cpp_array_addr_with_size
#define GET_CUSTOM_ATTRIBUTE_TYPE_RANGE_START(tr) ((tr).startOffset)

#define DECLARE_INVOKE_METHOD_BEGIN(__methodName__) void __methodName__(Il2CppMethodPointer methodPtr, const MethodInfo* method, void* __this, void** __args, void* __ret)
#define DECLARE_INVOKE_METHOD_RET(__ret__) __ret = __ret__
#define SET_IL2CPPTYPE_VALUE_TYPE(type, v) (type).valuetype = v
#define GET_IL2CPPTYPE_VALUE_TYPE(type) (type).valuetype

#define VALUE_TYPE_METHOD_POINTER_IS_ADJUST_METHOD 0

namespace huatuo
{

	inline Il2CppReflectionType* GetReflectionTypeFromName(Il2CppString* name)
	{
		return il2cpp::icalls::mscorlib::System::RuntimeTypeHandle::internal_from_name(name, nullptr, nullptr,  true, false, false);
	}

	inline void ConstructDelegate(Il2CppDelegate* delegate, Il2CppObject* target, const MethodInfo* method)
	{
		il2cpp::vm::Type::ConstructDelegate(delegate, target, method);
	}

	inline const MethodInfo* GetGenericVirtualMethod(const MethodInfo* result, const MethodInfo* inflateMethod)
	{
		VirtualInvokeData vid;
		il2cpp::vm::Runtime::GetGenericVirtualMethod(result, inflateMethod, &vid);
		return vid.method;
	}

	inline void InitNullableValueType(void* nullableValueTypeObj, void* data, Il2CppClass* klass)
	{
		IL2CPP_ASSERT(klass->fields[0].offset == sizeof(Il2CppObject));
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		std::memmove((uint8_t*)nullableValueTypeObj + klass->fields[1].offset - sizeof(Il2CppObject), data, size);
		*((uint8_t*)nullableValueTypeObj) = 1;
	}

	inline void NewNullableValueType(void* nullableValueTypeObj, void* data, Il2CppClass* klass)
	{
		InitNullableValueType(nullableValueTypeObj, data, klass);
	}

	inline bool IsNullableHasValue(void* nullableValueObj, Il2CppClass* klass)
	{
		IL2CPP_ASSERT(klass->fields[0].offset == sizeof(Il2CppObject));
		return *((uint8_t*)nullableValueObj);
	}

	inline void GetNullableValueOrDefault(void* dst, void* nullableValueObj, Il2CppClass* klass)
	{
		IL2CPP_ASSERT(klass->fields[0].offset == sizeof(Il2CppObject));
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		if (*((uint8_t*)nullableValueObj))
		{
			std::memmove(dst, (uint8_t*)nullableValueObj + klass->fields[1].offset - sizeof(Il2CppObject), size);
		}
		else
		{
			std::memset(dst, 0, size);
		}
	}

	inline void GetNullableValueOrDefault(void* dst, void* nullableValueObj, void* defaultData, Il2CppClass* klass)
	{
		IL2CPP_ASSERT(klass->fields[0].offset == sizeof(Il2CppObject));
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		std::memmove(dst, *((uint8_t*)nullableValueObj) ? (uint8_t*)nullableValueObj + klass->fields[1].offset - sizeof(Il2CppObject) : defaultData, size);
	}

	inline void GetNullableValue(void* dst, void* nullableValueObj, Il2CppClass* klass)
	{
		IL2CPP_ASSERT(klass->fields[0].offset == sizeof(Il2CppObject));
		uint32_t size = klass->castClass->instance_size - sizeof(Il2CppObject);
		if (*((uint8_t*)nullableValueObj))
		{
			std::memmove(dst, (uint8_t*)nullableValueObj + klass->fields[1].offset - sizeof(Il2CppObject), size);
		}
		else
		{
			il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Nullable object must have a value."));
		}
	}

	inline Il2CppString* GetKlassFullName(const Il2CppType* type)
	{
		Il2CppReflectionType* refType = il2cpp::icalls::mscorlib::System::Type::internal_from_handle((intptr_t)type);
		return il2cpp::icalls::mscorlib::System::RuntimeType::getFullName((Il2CppReflectionRuntimeType*)refType, false, false);
	}

}