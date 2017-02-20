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

		public ILInstanceMethod(Type type, string methodName)
		{
			this.Name = methodName;
			appDomain = Game.EntityEventManager.AppDomain;
			this.instance = this.appDomain.Instantiate(type.FullName);
			this.method = this.instance.Type.GetMethod(methodName);
		}

		public override void Run(params object[] param)
		{
			this.appDomain.Invoke(this.method, this.instance, param);
		}
	}

	public class ILStaticMethod : IStaticMethod
	{
		private readonly ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private readonly IMethod method;
		private readonly object[] param;

		public ILStaticMethod(IMethod method, int paramsCount)
		{
			this.param = new object[paramsCount + 1];
			this.Name = method.Name;
			appDomain = Game.EntityEventManager.AppDomain;
			this.method = method;
		}

		public override void Run(object instance, params object[] p)
		{
			if (this.method.IsStatic)
			{
				this.param[0] = instance;
				for (int i = 0; i < p.Length; ++i)
				{
					this.param[1 + i] = p[i];
				}
				this.appDomain.Invoke(this.method, null, this.param);
				return;
			}
			this.appDomain.Invoke(this.method, instance, p);
		}
	}
}
