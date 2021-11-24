using System;
using System.Collections.Generic;
using ILRuntime.Runtime.Intepreter;

namespace ProtoBuf{
	public class PType
	{
		static PType m_Current;
	    static PType Current
	    {
	        get
	        { 
				if (m_Current == null) {
					m_Current = new PType ();
				}
	            return m_Current;
	        }
	    }
		Dictionary<string, Type> m_Types = new Dictionary<string, Type>();
		
	    private PType() { }

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
		private static void RegisterFunctionCreateInstance(DelegateFunctionCreateInstance func){
			CreateInstanceFunc = func;
		}
		public delegate Type DelegateFunctionGetRealType(object o);
		static DelegateFunctionGetRealType GetRealTypeFunc;
		private static void RegisterFunctionGetRealType(DelegateFunctionGetRealType func){
			GetRealTypeFunc = func;
		}

		public static Type FindType(string metaName)
		{
			return Current.FindTypeInternal(metaName);
		}

		public static object CreateInstance(Type type){
			if (Type.GetType (type.FullName) == null) {
				if (CreateInstanceFunc != null) {
					return CreateInstanceFunc.Invoke(type.FullName);
				}
			}
			return Activator.CreateInstance (type
				#if !(CF || SILVERLIGHT || WINRT || PORTABLE || NETSTANDARD1_3 || NETSTANDARD1_4)
				, nonPublic: true
				#endif
			);
		}
		public static Type GetPType(object o){
			if (GetRealTypeFunc != null) {
				return GetRealTypeFunc.Invoke (o);
			}
			return o.GetType ();
		}

		public static void RegisterILRuntimeCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appdomain)
		{
			RegisterFunctionCreateInstance(typeName => appdomain.Instantiate(typeName));
			RegisterFunctionGetRealType(o =>
			{
				Type type;
				if (o is ILTypeInstance ins)
				{
					type = ins.Type.ReflectionType;
					RegisterType(type.FullName, type); //自动注册一下
				}
				else
				{
					type = o.GetType();
				}

				return type;
			});
		}
	}
}
