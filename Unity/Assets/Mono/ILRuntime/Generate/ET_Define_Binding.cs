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
    unsafe class ET_Define_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(ET.Define);
            args = new Type[]{typeof(System.String), typeof(System.Boolean)};
            method = type.GetMethod("GetAssetBundleDependencies", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetAssetBundleDependencies_0);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("GetAssetPathsFromAssetBundle", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetAssetPathsFromAssetBundle_1);
            args = new Type[]{typeof(System.String)};
            method = type.GetMethod("LoadAssetAtPath", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LoadAssetAtPath_2);

            field = type.GetField("IsAsync", flag);
            app.RegisterCLRFieldGetter(field, get_IsAsync_0);
            app.RegisterCLRFieldSetter(field, set_IsAsync_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_IsAsync_0, AssignFromStack_IsAsync_0);
            field = type.GetField("IsEditor", flag);
            app.RegisterCLRFieldGetter(field, get_IsEditor_1);
            app.RegisterCLRFieldSetter(field, set_IsEditor_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_IsEditor_1, AssignFromStack_IsEditor_1);


        }


        static StackObject* GetAssetBundleDependencies_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @v = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @assetBundleName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = ET.Define.GetAssetBundleDependencies(@assetBundleName, @v);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GetAssetPathsFromAssetBundle_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @assetBundleName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = ET.Define.GetAssetPathsFromAssetBundle(@assetBundleName);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* LoadAssetAtPath_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @s = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = ET.Define.LoadAssetAtPath(@s);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_IsAsync_0(ref object o)
        {
            return ET.Define.IsAsync;
        }

        static StackObject* CopyToStack_IsAsync_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ET.Define.IsAsync;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_IsAsync_0(ref object o, object v)
        {
            ET.Define.IsAsync = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_IsAsync_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @IsAsync = ptr_of_this_method->Value == 1;
            ET.Define.IsAsync = @IsAsync;
            return ptr_of_this_method;
        }

        static object get_IsEditor_1(ref object o)
        {
            return ET.Define.IsEditor;
        }

        static StackObject* CopyToStack_IsEditor_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ET.Define.IsEditor;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_IsEditor_1(ref object o, object v)
        {
            ET.Define.IsEditor = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_IsEditor_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @IsEditor = ptr_of_this_method->Value == 1;
            ET.Define.IsEditor = @IsEditor;
            return ptr_of_this_method;
        }



    }
}
