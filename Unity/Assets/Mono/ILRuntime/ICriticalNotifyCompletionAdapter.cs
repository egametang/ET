using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ET
{   
    public class ICriticalNotifyCompletionAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(System.Runtime.CompilerServices.ICriticalNotifyCompletion);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : System.Runtime.CompilerServices.ICriticalNotifyCompletion, CrossBindingAdaptorType
        {
            CrossBindingMethodInfo<System.Action> mUnsafeOnCompleted_0 = new CrossBindingMethodInfo<System.Action>("UnsafeOnCompleted");
            CrossBindingMethodInfo<System.Action> mOnCompleted_1 = new CrossBindingMethodInfo<System.Action>("OnCompleted");

            bool isInvokingToString;
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public void UnsafeOnCompleted(System.Action continuation)
            {
                mUnsafeOnCompleted_0.Invoke(this.instance, continuation);
            }

            public void OnCompleted(System.Action continuation)
            {
                mOnCompleted_1.Invoke(this.instance, continuation);
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    if (!isInvokingToString)
                    {
                        isInvokingToString = true;
                        string res = instance.ToString();
                        isInvokingToString = false;
                        return res;
                    }
                    else
                        return instance.Type.FullName;
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}

