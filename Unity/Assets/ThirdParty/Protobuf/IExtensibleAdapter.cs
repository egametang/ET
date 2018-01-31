using UnityEngine;
using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Collections;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using ProtoBuf;

namespace ProtoBuf
{
    public sealed class IExtensibleAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(IExtensible);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adaptor);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        internal class Adaptor : IExtensible, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adaptor():base()
            {

            }

            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
                Init();
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public IExtension GetExtensionObject(bool createIfMissing)
            {
                return appdomain.Invoke(mMethoGetExObject, instance, createIfMissing) as IExtension;
            }

            IMethod mMethoGetExObject;

            void Init()
            {
                mMethoGetExObject = instance.Type.GetMethod("GetExtensionObject",0);
            }
        }
    }
}