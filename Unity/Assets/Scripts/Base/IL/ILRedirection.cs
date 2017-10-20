using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace Model
{
	public static class ILRedirection
	{
		public static unsafe StackObject* LogDebug(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
		{
			//ILRuntime的调用约定为被调用者清理堆栈，因此执行这个函数后需要将参数从堆栈清理干净，并把返回值放在栈顶，具体请看ILRuntime实现原理文档
			ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
			StackObject* ptr_of_this_method;
			//这个是最后方法返回后esp栈指针的值，应该返回清理完参数并指向返回值，这里是只需要返回清理完参数的值即可
			StackObject* __ret = ILIntepreter.Minus(__esp, 1);
			//取Log方法的参数，如果有两个参数的话，第一个参数是esp - 2,第二个参数是esp -1, 因为Mono的bug，直接-2值会错误，所以要调用ILIntepreter.Minus
			ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

			//这里是将栈指针上的值转换成object，如果是基础类型可直接通过ptr->Value和ptr->ValueLow访问到值，具体请看ILRuntime实现原理文档
			object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
			//所有非基础类型都得调用Free来释放托管堆栈
			__intp.Free(ptr_of_this_method);

			//在真实调用Debug.Log前，我们先获取DLL内的堆栈
			var stacktrace = __domain.DebugService.GetStackTrance(__intp);

			//我们在输出信息后面加上DLL堆栈
			Log.Debug(message + "\n" + stacktrace);

			return __ret;
		}

		public static unsafe StackObject* LogInfo(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
		{
			//ILRuntime的调用约定为被调用者清理堆栈，因此执行这个函数后需要将参数从堆栈清理干净，并把返回值放在栈顶，具体请看ILRuntime实现原理文档
			ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
			StackObject* ptr_of_this_method;
			//这个是最后方法返回后esp栈指针的值，应该返回清理完参数并指向返回值，这里是只需要返回清理完参数的值即可
			StackObject* __ret = ILIntepreter.Minus(__esp, 1);
			//取Log方法的参数，如果有两个参数的话，第一个参数是esp - 2,第二个参数是esp -1, 因为Mono的bug，直接-2值会错误，所以要调用ILIntepreter.Minus
			ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

			//这里是将栈指针上的值转换成object，如果是基础类型可直接通过ptr->Value和ptr->ValueLow访问到值，具体请看ILRuntime实现原理文档
			object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
			//所有非基础类型都得调用Free来释放托管堆栈
			__intp.Free(ptr_of_this_method);

			//在真实调用Debug.Log前，我们先获取DLL内的堆栈
			var stacktrace = __domain.DebugService.GetStackTrance(__intp);

			//我们在输出信息后面加上DLL堆栈
			Log.Info(message + "\n" + stacktrace);

			return __ret;
		}

		public static unsafe StackObject* LogError(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
		{
			//ILRuntime的调用约定为被调用者清理堆栈，因此执行这个函数后需要将参数从堆栈清理干净，并把返回值放在栈顶，具体请看ILRuntime实现原理文档
			ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
			StackObject* ptr_of_this_method;
			//这个是最后方法返回后esp栈指针的值，应该返回清理完参数并指向返回值，这里是只需要返回清理完参数的值即可
			StackObject* __ret = ILIntepreter.Minus(__esp, 1);
			//取Log方法的参数，如果有两个参数的话，第一个参数是esp - 2,第二个参数是esp -1, 因为Mono的bug，直接-2值会错误，所以要调用ILIntepreter.Minus
			ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

			//这里是将栈指针上的值转换成object，如果是基础类型可直接通过ptr->Value和ptr->ValueLow访问到值，具体请看ILRuntime实现原理文档
			object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
			//所有非基础类型都得调用Free来释放托管堆栈
			__intp.Free(ptr_of_this_method);

			//在真实调用Debug.Log前，我们先获取DLL内的堆栈
			var stacktrace = __domain.DebugService.GetStackTrance(__intp);

			//我们在输出信息后面加上DLL堆栈
			Log.Error(message + "\n" + stacktrace);

			return __ret;
		}
	}
}
