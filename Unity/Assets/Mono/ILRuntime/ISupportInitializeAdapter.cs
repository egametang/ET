using System;
using System.ComponentModel;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace ET
{
    public class ISupportInitializeAdapter: CrossBindingAdaptor
    {
        private static CrossBindingMethodInfo mBeginInit_0 = new CrossBindingMethodInfo("BeginInit");
        private static CrossBindingMethodInfo mEndInit_1 = new CrossBindingMethodInfo("EndInit");
        public override Type BaseCLRType => typeof (ISupportInitialize);

        public override Type AdaptorType => typeof (Adapter);

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter: ISupportInitialize, CrossBindingAdaptorType
        {
            private ILTypeInstance instance;
            private AppDomain appdomain;

            public Adapter()
            {
            }

            public Adapter(AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance => instance;

            public void BeginInit()
            {
                mBeginInit_0.Invoke(this.instance);
            }

            public void EndInit()
            {
                mEndInit_1.Invoke(this.instance);
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }

                return this.instance.Type.FullName;
            }
        }
    }
}