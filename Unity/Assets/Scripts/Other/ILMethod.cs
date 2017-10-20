using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace Model
{
	public class ILInstanceMethod : IInstanceMethod
	{
		private readonly ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private readonly ILTypeInstance instance;
		private readonly IMethod method;
		private readonly object[] param;

		public ILInstanceMethod(Type type, string methodName)
		{
			appDomain = Init.Instance.AppDomain;
			this.instance = this.appDomain.Instantiate(type.FullName);
			this.method = this.instance.Type.GetMethod(methodName);
			int n = this.method.ParameterCount;
			this.param = new object[n];
		}

		public override void Run()
		{
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public override void Run(object a)
		{
			this.param[0] = a;
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public override void Run(object a, object b)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public override void Run(object a, object b, object c)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.param[2] = c;
			this.appDomain.Invoke(this.method, this.instance, param);
		}
	}

	public class ILStaticMethod : IStaticMethod
	{
		private readonly ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private readonly IMethod method;
		private readonly object[] param;

		public ILStaticMethod(string typeName, string methodName, int paramsCount)
		{
			appDomain = Init.Instance.AppDomain;
			this.method = appDomain.GetType(typeName).GetMethod(methodName, paramsCount);
			this.param = new object[paramsCount];
		}

		public override void Run()
		{
			this.appDomain.Invoke(this.method, null, this.param);
		}

		public override void Run(object a)
		{
			this.param[0] = a;
			this.appDomain.Invoke(this.method, null, param);
		}

		public override void Run(object a, object b)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.appDomain.Invoke(this.method, null, param);
		}

		public override void Run(object a, object b, object c)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.param[2] = c;
			this.appDomain.Invoke(this.method, null, param);
		}
	}
}
