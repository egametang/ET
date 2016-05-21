using System;
using System.Collections.Generic;
using System.Text;

namespace CLRSharp
{
    public class CLRSharp_Instance
    {
        public ICLRType_Sharp type
        {
            get;
            private set;
        }
        public CLRSharp_Instance(ICLRType_Sharp type)
        {
            this.type = type;
        }
        public object system_base;
        public Dictionary<string, object> Fields = new Dictionary<string, object>();

        //每个方法都可以产生一个代理
        public Dictionary<IMethod, Delegate> Delegates = new Dictionary<IMethod, Delegate>();

        public Delegate GetDelegate(ThreadContext context, Type deleType, IMethod method)
        {
            Delegate dele = null;
            if (!Delegates.TryGetValue(method, out dele))
            {
                dele = Delegate_Binder.MakeDelegate(deleType,  this, method);
                Delegates[method] = dele;
                //需要从Delegate转换成实际类型赋值的帮助类


            }
            return dele;
        }
    }

}
