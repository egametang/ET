using ILRuntime.CLR.Method;
using UnityEngine;

namespace ET
{
    public class ILStaticMethod : IStaticMethod
    {
        private readonly ILRuntime.Runtime.Enviorment.AppDomain appDomain;
        private readonly IMethod method;
        private readonly object[] param;

        public ILStaticMethod(ILRuntime.Runtime.Enviorment.AppDomain appDomain, string typeName, string methodName, int paramsCount)
        {
            this.appDomain = appDomain;
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


