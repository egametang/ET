using System;
using ETModel;
using FairyGUI.Utils;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace FairyGUI
{
    /// <summary>
    /// 用于FairyGui GComponentAdaptor适配
    /// </summary>
    [ILAdapter]
    public class GComponentAdaptor: CrossBindingAdaptor
    {
        public override Type BaseCLRType => typeof(GComponent);

        public override Type AdaptorType => typeof(Adaptor);

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            // 创建一个新的实例
            return new Adaptor(appdomain, instance);
        }

        // 实际的适配器类需要继承你想继承的那个类，并且实现CrossBindingAdaptorType接口
        public class Adaptor : GComponent, CrossBindingAdaptorType
        {
            private ILTypeInstance instance;
            private AppDomain appDomain;
            
            private IMethod iDisposable;
            private IMethod constructFromXML;
            private bool constructFromXMLGot;

            // 缓存这个数组来避免调用时的GC Alloc
            private readonly object[] param0 = new object[0];

            public Adaptor()
            {
            }

            public Adaptor(AppDomain appDomain, ILTypeInstance instance)
            {
                this.appDomain = appDomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance => instance;

            // 你需要重写所有你希望在热更脚本里面重写的方法，并且将控制权转到脚本里去
            public override void ConstructFromXML(XML xml)
            {
                if (!this.constructFromXMLGot)
                {
                    this.constructFromXML = instance.Type.GetMethod("ConstructFromXML", 0);
                    this.constructFromXMLGot = true;
                }
                if (this.constructFromXML != null)
                {
                    this.appDomain.Invoke(this.constructFromXML, instance, xml); //没有参数建议显式传递null为参数列表，否则会自动new object[0]导致GC Alloc
                }
            }

            public override void Dispose()
            {
                if (this.iDisposable == null)
                {
                    this.iDisposable = instance.Type.GetMethod("Dispose");
                }
                this.appDomain.Invoke(this.iDisposable, instance, this.param0);
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
