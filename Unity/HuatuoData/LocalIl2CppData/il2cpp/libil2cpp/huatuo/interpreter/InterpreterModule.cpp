#include "InterpreterModule.h"

#include "Interpreter.h"

#include <unordered_map>

#include "vm/GlobalMetadata.h"
#include "vm/MetadataLock.h"
#include "vm/Class.h"

#include "../metadata/MetadataModule.h"
#include "../metadata/MetadataUtil.h"
#include "../transform/Transform.h"

#include "MethodBridge.h"

namespace huatuo
{
namespace interpreter
{
	il2cpp::os::ThreadLocalValue InterpreterModule::s_machineState;

	static std::unordered_map<const char*, NativeCallMethod, CStringHash, CStringEqualTo> s_calls;
	static std::unordered_map<const char*, NativeInvokeMethod, CStringHash, CStringEqualTo> s_invokes;

	MachineState& InterpreterModule::GetCurrentThreadMachineState()
	{
		MachineState* state = nullptr;
		s_machineState.GetValue((void**)&state);
		if (!state)
		{
			state = new MachineState();
			s_machineState.SetValue(state);
		}
		return *state;
	}

	void InterpreterModule::Initialize()
	{
		for (size_t i = 0; ; i++)
		{
			NativeCallMethod& method = g_callStub[i];
			if (!method.signature)
			{
				break;
			}
			s_calls.insert({ method.signature, method });
		}

		for (size_t i = 0; ; i++)
		{
			NativeInvokeMethod& method = g_invokeStub[i];
			if (!method.signature)
			{
				break;
			}
			s_invokes.insert({ method.signature, method });
		}
	}

	template<typename T>
	const NativeCallMethod* GetNativeCallMethod(const T* method, bool forceStatic)
	{
		char sigName[1000];
		ComputeSignature(method, !forceStatic, sigName, sizeof(sigName) - 1);
		auto it = s_calls.find(sigName);
		return (it != s_calls.end()) ? &it->second : nullptr;
	}

	template<typename T>
	const NativeInvokeMethod* GetNativeInvokeMethod(const T* method)
	{
		char sigName[1000];
		ComputeSignature(method, false, sigName, sizeof(sigName) - 1);
		auto it = s_invokes.find(sigName);
		return (it != s_invokes.end()) ? &it->second : nullptr;
	}

	static void RaiseMethodNotSupportException(const MethodInfo* method, const char* desc)
	{
		TEMP_FORMAT(errMsg, "%s. %s.%s::%s", desc, method->klass->namespaze, method->klass->name, method->name);
		il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetExecutionEngineException(errMsg));
	}

	static void RaiseMethodNotSupportException(const Il2CppMethodDefinition* method, const char* desc)
	{
		Il2CppClass* klass = il2cpp::vm::GlobalMetadata::GetTypeInfoFromTypeDefinitionIndex(method->declaringType);
		TEMP_FORMAT(errMsg, "%s. %s.%s::%s", desc, klass->namespaze, klass->name, il2cpp::vm::GlobalMetadata::GetStringFromIndex(method->nameIndex));
		il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetExecutionEngineException(errMsg));
	}

	static void NotSupportNative2Managed()
	{
		il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetExecutionEngineException("NotSupportNative2Managed"));
	}

	static void* NotSupportInvoke(Il2CppMethodPointer, const MethodInfo* method, void*, void**)
	{
		TEMP_FORMAT(errMsg, "Invoke method missing. %s.%s::%s", method->klass->namespaze, method->klass->name, method->name);
		il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetExecutionEngineException(errMsg));
		return nullptr;
	}

	Il2CppMethodPointer InterpreterModule::GetMethodPointer(const Il2CppMethodDefinition* method)
	{
		const NativeCallMethod* ncm = GetNativeCallMethod(method, false);
		if (ncm)
		{
			return ncm->method;
		}
		//RaiseMethodNotSupportException(method, "GetMethodPointer");
		return (Il2CppMethodPointer)NotSupportNative2Managed;
	}

	Il2CppMethodPointer InterpreterModule::GetMethodPointer(const MethodInfo* method)
	{
		const NativeCallMethod* ncm = GetNativeCallMethod(method, false);
		if (ncm)
		{
			return ncm->method;
		}
		//RaiseMethodNotSupportException(method, "GetMethodPointer");
		return (Il2CppMethodPointer)NotSupportNative2Managed;
	}

	Il2CppMethodPointer InterpreterModule::GetAdjustThunkMethodPointer(const Il2CppMethodDefinition* method)
	{
		if (!huatuo::metadata::IsInstanceMethod(method))
		{
			return nullptr;
		}
		const NativeCallMethod* ncm = GetNativeCallMethod(method, false);
		if (ncm)
		{
			return ncm->adjustThunkMethod;
		}
		//RaiseMethodNotSupportException(method, "GetAdjustThunkMethodPointer");
		return (Il2CppMethodPointer)NotSupportNative2Managed;
	}

	Il2CppMethodPointer InterpreterModule::GetAdjustThunkMethodPointer(const MethodInfo* method)
	{
		if (!huatuo::metadata::IsInstanceMethod(method))
		{
			return nullptr;
		}
		const NativeCallMethod* ncm = GetNativeCallMethod(method, false);
		if (ncm)
		{
			return ncm->adjustThunkMethod;
		}
		//RaiseMethodNotSupportException(method, "GetAdjustThunkMethodPointer");
		return (Il2CppMethodPointer)NotSupportNative2Managed;
	}

	Managed2NativeCallMethod InterpreterModule::GetManaged2NativeMethodPointer(const MethodInfo* method, bool forceStatic)
	{
		const NativeCallMethod* ncm = GetNativeCallMethod(method, forceStatic);
		if (ncm)
		{
			return ncm->managed2NativeMethod;
		}
		char sigName[1000];
		ComputeSignature(method, !forceStatic, sigName, sizeof(sigName) - 1);

		TEMP_FORMAT(errMsg, "GetManaged2NativeMethodPointer. sinature:%s not support.", sigName);
		RaiseMethodNotSupportException(method, errMsg);
		return nullptr;
	}

	Managed2NativeCallMethod InterpreterModule::GetManaged2NativeMethodPointer(const metadata::ResolveStandAloneMethodSig& method)
	{
		char sigName[1000];
		ComputeSignature(&method.returnType, method.params, method.paramCount, false, sigName, sizeof(sigName) - 1);
		auto it = s_calls.find(sigName);
		if (it != s_calls.end())
		{
			return it->second.managed2NativeMethod;
		}
		TEMP_FORMAT(errMsg, "GetManaged2NativeMethodPointer. sinature:%s not support.", sigName);
		il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetExecutionEngineException(errMsg));
		return nullptr;
	}

	InvokerMethod InterpreterModule::GetMethodInvoker(const Il2CppMethodDefinition* method)
	{
		const NativeInvokeMethod* nim = GetNativeInvokeMethod(method);
		if (nim)
		{
			return huatuo::metadata::IsInstanceMethod(method) ? nim->instanceMethod : nim->staticMethod;
		}
		//RaiseMethodNotSupportException(method, "GetMethodInvoker");
		return (InvokerMethod)NotSupportInvoke;
	}

	InvokerMethod InterpreterModule::GetMethodInvoker(const MethodInfo* method)
	{
		const NativeInvokeMethod* nim = GetNativeInvokeMethod(method);
		if (nim)
		{
			return huatuo::metadata::IsInstanceMethod(method) ? nim->instanceMethod : nim->staticMethod;
		}
		//RaiseMethodNotSupportException(method, "GetMethodInvoker");
		return (InvokerMethod)NotSupportInvoke;
	}

	InterpMethodInfo* InterpreterModule::GetInterpMethodInfo(const MethodInfo* methodInfo)
	{
		il2cpp::os::FastAutoLock lock(&il2cpp::vm::g_MetadataLock);

		if (methodInfo->huatuoData)
		{
			return (InterpMethodInfo*)methodInfo->huatuoData;
		}

		metadata::Image* image = metadata::IsInterpreterMethod(methodInfo) ? huatuo::metadata::MetadataModule::GetImage(methodInfo->klass)
			: (metadata::Image*)huatuo::metadata::AOTHomologousImage::FindImageByAssembly(methodInfo->klass->image->assembly);
		IL2CPP_ASSERT(image);

		metadata::MethodBody* methodBody = image->GetMethodBody(methodInfo);
		if (methodBody == nullptr || methodBody->ilcodes == nullptr)
		{
			TEMP_FORMAT(errMsg, "%s.%s::%s method body is null. not support external method currently.", methodInfo->klass->namespaze, methodInfo->klass->name, methodInfo->name);
			il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetExecutionEngineException(errMsg));
		}
		InterpMethodInfo* imi = new (IL2CPP_MALLOC_ZERO(sizeof(InterpMethodInfo))) InterpMethodInfo;
		transform::HiTransform::Transform(image, methodInfo, *methodBody, *imi);
		il2cpp::os::Atomic::FullMemoryBarrier();
		const_cast<MethodInfo*>(methodInfo)->huatuoData = imi;
		return imi;
	}
}
}

