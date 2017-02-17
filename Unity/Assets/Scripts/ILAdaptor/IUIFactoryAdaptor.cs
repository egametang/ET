using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace Model
{
	[ILAdapter]
	public class IUIFactoryAdaptorClassInheritanceAdaptor : CrossBindingAdaptor
	{
		public override Type BaseCLRType
		{
			get
			{
				return typeof (IUIFactory);
			}
		}

		public override Type AdaptorType
		{
			get
			{
				return typeof (IUIFactoryAdaptor);
			}
		}

		public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
		{
			return new IUIFactoryAdaptor(appdomain, instance);
		}
		
		public class IUIFactoryAdaptor : IUIFactory, CrossBindingAdaptorType
		{
			private ILTypeInstance instance;
			private ILRuntime.Runtime.Enviorment.AppDomain appDomain;

			private IMethod mCreate;
			private readonly object[] param3 = new object[3];

			public IUIFactoryAdaptor()
			{
			}

			public IUIFactoryAdaptor(ILRuntime.Runtime.Enviorment.AppDomain appDomain, ILTypeInstance instance)
			{
				this.appDomain = appDomain;
				this.instance = instance;
			}

			public ILTypeInstance ILInstance
			{
				get
				{
					return instance;
				}
			}

			public UI Create(Scene scene, int type, UI parent)
			{
				if (this.mCreate == null)
				{
					this.mCreate = instance.Type.GetMethod("Create", 3);
				}
				this.param3[0] = scene;
				this.param3[1] = type;
				this.param3[2] = parent;
				UI ui = (UI)this.appDomain.Invoke(this.mCreate, instance, this.param3);
				return ui;
			}

			public override string ToString()
			{
				IMethod m = this.appDomain.ObjectType.GetMethod("ToString", 0);
				m = instance.Type.GetVirtualMethod(m);
				if (m == null || m is ILMethod)
				{
					return instance.ToString();
				}

				return instance.Type.FullName;
			}
		}
	}
}
