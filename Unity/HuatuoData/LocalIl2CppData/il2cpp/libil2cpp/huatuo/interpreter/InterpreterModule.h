#pragma once

#include "os/ThreadLocalValue.h"

#include "../CommonDef.h"
#include "MethodBridge.h"
#include "Engine.h"
#include "../metadata/Image.h"

namespace huatuo
{
namespace interpreter
{

	class InterpreterModule
	{
	public:
		static void Initialize();


		static MachineState& GetCurrentThreadMachineState();

		static InterpMethodInfo* GetInterpMethodInfo(const MethodInfo* methodInfo);

		static Il2CppMethodPointer GetMethodPointer(const Il2CppMethodDefinition* method);
		static Il2CppMethodPointer GetMethodPointer(const MethodInfo* method);
		static Il2CppMethodPointer GetAdjustThunkMethodPointer(const Il2CppMethodDefinition* method);
		static Il2CppMethodPointer GetAdjustThunkMethodPointer(const MethodInfo* method);
		static Managed2NativeCallMethod GetManaged2NativeMethodPointer(const MethodInfo* method, bool forceStatic);
		static Managed2NativeCallMethod GetManaged2NativeMethodPointer(const metadata::ResolveStandAloneMethodSig& methodSig);

		static InvokerMethod GetMethodInvoker(const Il2CppMethodDefinition* method);
		static InvokerMethod GetMethodInvoker(const MethodInfo* method);

	private:

		static il2cpp::os::ThreadLocalValue s_machineState;
	};
}
}
