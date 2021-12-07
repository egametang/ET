using System;
using System.Runtime.CompilerServices;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace ET
{
    /// <summary>
    /// 用于async await适配
    /// </summary>
    public class IAsyncStateMachineClassInheritanceAdaptor: CrossBindingAdaptor
    {
        public override Type BaseCLRType => typeof (IAsyncStateMachine);

        public override Type AdaptorType => typeof (IAsyncStateMachineAdaptor);

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new IAsyncStateMachineAdaptor(appdomain, instance);
        }

        public class IAsyncStateMachineAdaptor: IAsyncStateMachine, CrossBindingAdaptorType
        {
            private readonly ILTypeInstance instance;
            private readonly AppDomain appDomain;

            private readonly IMethod mMoveNext;
            private readonly IMethod mSetStateMachine;

            public IAsyncStateMachineAdaptor()
            {
            }

            public IAsyncStateMachineAdaptor(AppDomain appDomain, ILTypeInstance instance)
            {
                this.appDomain = appDomain;
                this.instance = instance;
                mMoveNext = instance.Type.GetMethod("MoveNext", 0);
                mSetStateMachine = instance.Type.GetMethod("SetStateMachine");
            }

            public ILTypeInstance ILInstance => instance;

            public void MoveNext()
            {
                this.appDomain.Invoke(mMoveNext, instance, null);
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.appDomain.Invoke(mSetStateMachine, instance, stateMachine);
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