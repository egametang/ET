using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Adapters
{
    public class AttributeAdapter : CrossBindingAdaptor
    {
        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override Type BaseCLRType
        {
            get
            {
                return typeof(Attribute);
            }
        }

        public override object CreateCLRInstance(Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : Attribute, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            bool isToStringGot;
            IMethod toString;

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }
            public ILTypeInstance ILInstance
            {
                get
                {
                    return instance;
                }
            }

            public override string ToString()
            {
                if (!isToStringGot)
                {
                    isToStringGot = true;
                    IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                    toString = instance.Type.GetVirtualMethod(m);
                }
                if (toString == null || toString is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}
