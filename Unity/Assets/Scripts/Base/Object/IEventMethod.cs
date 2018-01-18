using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace Model
{
	public interface IEventMethod
	{
		void Run();
		void Run<A>(A a);
		void Run<A, B>(A a, B b);
		void Run<A, B, C>(A a, B b, C c);
		void Run<A, B, C, D>(A a, B b, C c, D d);
	}

	public class IEventMonoMethod : IEventMethod
	{
		private readonly object obj;

		public IEventMonoMethod(object obj)
		{
			this.obj = obj;
		}

		public void Run()
		{
			((IEvent)obj).Run();
		}

		public void Run<A>(A a)
		{
			((IEvent<A>)obj).Run(a);
		}

		public void Run<A, B>(A a, B b)
		{
			((IEvent<A, B>)obj).Run(a, b);
		}

		public void Run<A, B, C>(A a, B b, C c)
		{
			((IEvent<A, B, C>)obj).Run(a, b, c);
		}

		public void Run<A, B, C, D>(A a, B b, C c, D d)
		{
			((IEvent<A, B, C, D>)obj).Run(a, b, c, d);
		}
	}

	public class IEventILMethod : IEventMethod
	{
		private readonly ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private readonly ILTypeInstance instance;
		private readonly IMethod method;
		private readonly object[] param;

		public IEventILMethod(Type type, string methodName)
		{
			appDomain = Init.Instance.AppDomain;
			this.instance = this.appDomain.Instantiate(type.FullName);
			this.method = this.instance.Type.GetMethod(methodName);
			int n = this.method.ParameterCount;
			this.param = new object[n];
		}

		public void Run()
		{
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public void Run<A>(A a)
		{
			this.param[0] = a;
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public void Run<A, B>(A a, B b)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public void Run<A, B, C>(A a, B b, C c)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.param[2] = c;
			this.appDomain.Invoke(this.method, this.instance, param);
		}

		public void Run<A, B, C, D>(A a, B b, C c, D d)
		{
			this.param[0] = a;
			this.param[1] = b;
			this.param[2] = c;
			this.param[3] = d;
			this.appDomain.Invoke(this.method, this.instance, param);
		}
	}
}
