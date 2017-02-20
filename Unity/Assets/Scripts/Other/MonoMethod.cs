using System;
using System.Reflection;
using Base;

namespace Model
{
	public class MonoInstanceMethod : IInstanceMethod
	{
		private readonly object instance;
		private readonly MethodInfo methodInfo;
		public MonoInstanceMethod(Type type, string methodName)
		{
			this.Name = methodName;
			this.instance = Activator.CreateInstance(type);
			this.methodInfo = type.GetMethod(methodName);
		}

		public override void Run(params object[] param)
		{
			this.methodInfo.Invoke(this.instance, param);
		}
	}

	public class MonoStaticMethod : IStaticMethod
	{
		private readonly MethodInfo methodInfo;

		public MonoStaticMethod(MethodInfo methodInfo)
		{
			this.methodInfo = methodInfo;
			this.Name = methodInfo.Name;
		}

		public override void Run(object instance, params object[] param)
		{
			this.methodInfo.Run(instance, param);
		}
	}
}
