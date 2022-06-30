#pragma once

#include "../CommonDef.h"
#include "InterpreterDefs.h"

namespace huatuo
{
namespace interpreter
{
	union StackObject;

	typedef void (*Managed2NativeCallMethod)(const MethodInfo* method, uint16_t* argVarIndexs, StackObject* localVarBase, void* ret);
	typedef void (*NativeClassCtor0)(Il2CppObject* obj, const MethodInfo* method);

	struct NativeCallMethod
	{
		const char* signature;
		Il2CppMethodPointer method;
		Il2CppMethodPointer adjustThunkMethod;
		Managed2NativeCallMethod managed2NativeMethod;
	};

	struct NativeInvokeMethod
	{
		const char* signature;
		InvokerMethod instanceMethod;
		InvokerMethod staticMethod;
	};

	extern NativeCallMethod g_callStub[];
	extern NativeInvokeMethod g_invokeStub[];


	template<int N>
	struct ValueTypeSize
	{
		uint8_t  __value[N];
	};

	struct ValueTypeSize16
	{
		uint64_t low;
		uint64_t high;
	};

	inline bool IsNeedExpandLocationType(LocationDataType type)
	{
		return type < LocationDataType::U8;
	}

	ArgDesc GetTypeArgDesc(const Il2CppType* type);

	inline LocationDataType GetLocationDataTypeByType(const Il2CppType* type)
	{
		return GetTypeArgDesc(type).type;
	}

	inline void ExpandLocationData2StackDataByType(void* retValue, LocationDataType type)
	{
		switch (type)
		{
		case huatuo::interpreter::LocationDataType::I1:
			*(int32_t*)retValue = *(int8_t*)retValue;
			break;
		case huatuo::interpreter::LocationDataType::U1:
			*(int32_t*)retValue = *(uint8_t*)retValue;
			break;
		case huatuo::interpreter::LocationDataType::I2:
			*(int32_t*)retValue = *(int16_t*)retValue;
			break;
		case huatuo::interpreter::LocationDataType::U2:
			*(int32_t*)retValue = *(uint16_t*)retValue;
			break;
		default:
			break;
		}
	}

	inline void ExpandLocationData2StackDataByType(void* retValue, Il2CppTypeEnum type)
	{
		switch (type)
		{
		case IL2CPP_TYPE_BOOLEAN:
		case IL2CPP_TYPE_I1:
			*(int32_t*)retValue = *(int8_t*)retValue;
			break;
		case IL2CPP_TYPE_U1:
			*(int32_t*)retValue = *(uint8_t*)retValue;
			break;
		case IL2CPP_TYPE_I2:
			*(int32_t*)retValue = *(int16_t*)retValue;
			break;
		case IL2CPP_TYPE_U2:
		case IL2CPP_TYPE_CHAR:
			*(int32_t*)retValue = *(uint16_t*)retValue;
			break;
		default:
			break;
		}
	}

	ArgDesc GetValueTypeArgDescBySize(uint32_t size);

	inline bool IsSimpleStackObjectCopyArg(LocationDataType type)
	{
		return type <= LocationDataType::U8;
	}

	void CopyArgs(StackObject* dstBase, StackObject* argBase, ArgDesc* args, uint32_t paramCount, uint32_t totalParamStackObjectSize);

	inline void* AdjustValueTypeSelfPointer(Il2CppObject* __this, const MethodInfo* method)
	{
		return __this + IS_CLASS_VALUE_TYPE(__this->klass);
	}

	bool IsPassArgAsValue(const Il2CppType* type, LocationDataType* locType = nullptr);
	Il2CppObject* TranslateNativeValueToBoxValue(const Il2CppType* type, void* value);
	void ConvertInvokeArgs(StackObject* resultArgs, const MethodInfo* method, void** __args);

	bool ComputeSignature(const MethodInfo* method, bool call, char* sigBuf, size_t bufferSize);
	bool ComputeSignature(const Il2CppMethodDefinition* method, bool call, char* sigBuf, size_t bufferSize);
	bool ComputeSignature(const Il2CppType* ret, const Il2CppType* params, uint32_t paramCount, bool instanceCall, char* sigBuf, size_t bufferSize);

}
}