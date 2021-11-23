
#if !SERVER
using ILRuntime.Runtime;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ProtoBuf
{

    public class PBType 
    {
		static PBType m_Current;
		static PBType Current
		{
			get
			{
				if (m_Current == null)
				{
					m_Current = new PBType();
				}
				return m_Current;
			}
		}
		Dictionary<string, Type> m_Types = new Dictionary<string, Type>();

		private PBType() { }

		void RegisterTypeInternal(string metaName, Type type)
		{
			m_Types[metaName] = type;
			//if (!m_Types.ContainsKey(metaName))
			//     {
			//m_Types.Add(metaName,type);
			//     }
			//     else
			//throw new SystemException(string.Format("PropertyMeta : {0} is registered!",metaName));
		}

		Type FindTypeInternal(string metaName)
		{
			Type type = null;
			if (!m_Types.TryGetValue(metaName, out type))
			{
				throw new SystemException(string.Format("PropertyMeta : {0} is not registered!", metaName));
			}
			return type;
		}

		public static void RegisterType(string metaName, Type type)
		{
			Current.RegisterTypeInternal(metaName, type);
		}

		public delegate object DelegateFunctionCreateInstance(string typeName);
		static DelegateFunctionCreateInstance CreateInstanceFunc;
		private static void RegisterFunctionCreateInstance(DelegateFunctionCreateInstance func)
		{
			CreateInstanceFunc = func;
		}
		public delegate Type DelegateFunctionGetRealType(object o);
		static DelegateFunctionGetRealType GetRealTypeFunc;
		private static void RegisterFunctionGetRealType(DelegateFunctionGetRealType func)
		{
			GetRealTypeFunc = func;
		}

		public static Type FindType(string metaName)
		{
			return Current.FindTypeInternal(metaName);
		}

		public static object CreateInstance(Type type)
		{
			if (Type.GetType(type.FullName) == null)
			{
				if (CreateInstanceFunc != null)
				{
					return CreateInstanceFunc.Invoke(type.FullName);
				}
			}
			return Activator.CreateInstance(type
#if !(CF || SILVERLIGHT || WINRT || PORTABLE || NETSTANDARD1_3 || NETSTANDARD1_4)
				, nonPublic: true
#endif
			);
		}
		public static Type GetPType(object o)
		{
			if (GetRealTypeFunc != null)
			{
				return GetRealTypeFunc.Invoke(o);
			}
			return o.GetType();
		}
#if !SERVER
		public static unsafe void RegisterILRuntimeCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
		{
			Type[] args;
			BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static |
					BindingFlags.DeclaredOnly;
			// 注册pb反序列化
			Type pbSerializeType = typeof(ProtoBuf.Serializer);
			args = new[] { typeof(Type), typeof(Stream) };
			var pbDeserializeMethod = pbSerializeType.GetMethod("Deserialize", flag, null, args, null);

			appdomain.RegisterCLRMethodRedirection(pbDeserializeMethod, Deserialize_1);
			args = new[] { typeof(ILTypeInstance) };
			Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
			List<MethodInfo> lst = null;
			foreach (var m in pbSerializeType.GetMethods())
			{
				if (m.IsGenericMethodDefinition)
				{
					if (!genericMethods.TryGetValue(m.Name, out lst))
					{
						lst = new List<MethodInfo>();
						genericMethods[m.Name] = lst;
					}

					lst.Add(m);
				}
			}

			if (genericMethods.TryGetValue("Deserialize", out lst))
			{
				foreach (var m in lst)
				{
					if (m.MatchGenericParameters(args, typeof(ILTypeInstance), typeof(Stream)))
					{
						var method = m.MakeGenericMethod(args);
						appdomain.RegisterCLRMethodRedirection(method, Deserialize_2);
						break;
					}
				}
			}

			RegisterFunctionCreateInstance(typeName => appdomain.Instantiate(typeName));
			RegisterFunctionGetRealType(o =>
			{
				var type = o.GetType();
				if (type.FullName == "ILRuntime.Runtime.Intepreter.ILTypeInstance")
				{
					var ilo = o as ILRuntime.Runtime.Intepreter.ILTypeInstance;
					type = FindType(ilo.Type.FullName);
				}

				return type;
			});
		}

		/// <summary>
		/// pb net 反序列化重定向
		/// </summary>
		/// <param name="__intp"></param>
		/// <param name="__esp"></param>
		/// <param name="__mStack"></param>
		/// <param name="__method"></param>
		/// <param name="isNewObj"></param>
		/// <returns></returns>
		private static unsafe StackObject* Deserialize_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method,
		bool isNewObj)
		{
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
			StackObject* ptr_of_this_method;
			StackObject* __ret = ILIntepreter.Minus(__esp, 2);

			ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
			Stream source = (Stream)typeof(Stream).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
			__intp.Free(ptr_of_this_method);

			ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
			Type type = (Type)typeof(Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
			__intp.Free(ptr_of_this_method);

			var result_of_this_method = ProtoBuf.Serializer.Deserialize(type, source);

			object obj_result_of_this_method = result_of_this_method;
			if (obj_result_of_this_method is CrossBindingAdaptorType)
			{
				return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance, true);
			}

			return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method, true);
		}

		/// <summary>
		/// pb net 反序列化重定向
		/// </summary>
		/// <param name="__intp"></param>
		/// <param name="__esp"></param>
		/// <param name="__mStack"></param>
		/// <param name="__method"></param>
		/// <param name="isNewObj"></param>
		/// <returns></returns>
		private static unsafe StackObject* Deserialize_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method,
		bool isNewObj)
		{
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
			StackObject* ptr_of_this_method;
			StackObject* __ret = ILIntepreter.Minus(__esp, 1);

			ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
			Stream source = (Stream)typeof(Stream).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
			__intp.Free(ptr_of_this_method);

			var genericArgument = __method.GenericArguments;
			var type = genericArgument[0];
			var realType = type is CLRType ? type.TypeForCLR : type.ReflectionType;
			var result_of_this_method = ProtoBuf.Serializer.Deserialize(realType, source);
			return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
		}
#endif
	}
}

