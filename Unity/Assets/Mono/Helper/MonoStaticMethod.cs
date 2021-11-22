using System;
using System.Reflection;

namespace ET
{
    public class MonoStaticMethod : IStaticMethod
    {
        private readonly MethodInfo methodInfo;

        private readonly object[] param;

        public MonoStaticMethod(Assembly assembly, string typeName, string methodName)
        {
            this.methodInfo = assembly.GetType(typeName).GetMethod(methodName);
            this.param = new object[this.methodInfo.GetParameters().Length];
        }

        public override void Run()
        {
            this.methodInfo.Invoke(null, param);
        }

        public override void Run(object a)
        {
            this.param[0] = a;
            this.methodInfo.Invoke(null, param);
        }

        public override void Run(object a, object b)
        {
            this.param[0] = a;
            this.param[1] = b;
            this.methodInfo.Invoke(null, param);
        }

        public override void Run(object a, object b, object c)
        {
            this.param[0] = a;
            this.param[1] = b;
            this.param[2] = c;
            this.methodInfo.Invoke(null, param);
        }
    }
}

