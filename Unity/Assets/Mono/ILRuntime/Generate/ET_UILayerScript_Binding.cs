using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ET_UILayerScript_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ET.UILayerScript);

            field = type.GetField("UILayer", flag);
            app.RegisterCLRFieldGetter(field, get_UILayer_0);
            app.RegisterCLRFieldSetter(field, set_UILayer_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_UILayer_0, AssignFromStack_UILayer_0);


        }



        static object get_UILayer_0(ref object o)
        {
            return ((ET.UILayerScript)o).UILayer;
        }

        static StackObject* CopyToStack_UILayer_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((ET.UILayerScript)o).UILayer;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_UILayer_0(ref object o, object v)
        {
            ((ET.UILayerScript)o).UILayer = (ET.UILayer)v;
        }

        static StackObject* AssignFromStack_UILayer_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            ET.UILayer @UILayer = (ET.UILayer)typeof(ET.UILayer).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)20);
            ((ET.UILayerScript)o).UILayer = @UILayer;
            return ptr_of_this_method;
        }



    }
}
