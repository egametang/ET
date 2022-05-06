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
    unsafe class ET_ForeachHelper_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            MethodBase method;
            Type[] args;
            Type type = typeof(ET.ForeachHelper);
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }
            args = new Type[]{typeof(ET.AService)};
            if (genericMethods.TryGetValue("Foreach", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Collections.Generic.HashSet<ET.AService>), typeof(System.Action<ET.AService>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Foreach_0);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("Foreach", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Collections.Generic.HashSet<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Foreach_1);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int64), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("Foreach", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Collections.Generic.Dictionary<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Action<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Foreach_2);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Type), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("Foreach", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(System.Collections.Generic.Dictionary<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Action<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Foreach_3);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int64), typeof(System.Int64)};
            if (genericMethods.TryGetValue("ForEachFunc", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ET.MultiMap<System.Int64, System.Int64>), typeof(System.Func<System.Int64, System.Collections.Generic.List<System.Int64>, System.Boolean>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ForEachFunc_4);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int64), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("ForEachFunc", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(void), typeof(ET.MultiMap<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Func<System.Int64, System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Boolean>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ForEachFunc_5);

                        break;
                    }
                }
            }


        }


        static StackObject* Foreach_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<ET.AService> @action = (System.Action<ET.AService>)typeof(System.Action<ET.AService>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.HashSet<ET.AService> @hashSet = (System.Collections.Generic.HashSet<ET.AService>)typeof(System.Collections.Generic.HashSet<ET.AService>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ET.ForeachHelper.Foreach<ET.AService>(@hashSet, @action);

            return __ret;
        }

        static StackObject* Foreach_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance> @action = (System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.HashSet<ILRuntime.Runtime.Intepreter.ILTypeInstance> @hashSet = (System.Collections.Generic.HashSet<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.HashSet<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ET.ForeachHelper.Foreach<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@hashSet, @action);

            return __ret;
        }

        static StackObject* Foreach_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance> @action = (System.Action<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Action<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.Dictionary<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance> @dictionary = (System.Collections.Generic.Dictionary<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.Dictionary<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ET.ForeachHelper.Foreach<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>(@dictionary, @action);

            return __ret;
        }

        static StackObject* Foreach_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance> @action = (System.Action<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Action<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.Dictionary<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance> @dictionary = (System.Collections.Generic.Dictionary<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.Dictionary<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ET.ForeachHelper.Foreach<System.Type, ILRuntime.Runtime.Intepreter.ILTypeInstance>(@dictionary, @action);

            return __ret;
        }

        static StackObject* ForEachFunc_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Int64, System.Collections.Generic.List<System.Int64>, System.Boolean> @func = (System.Func<System.Int64, System.Collections.Generic.List<System.Int64>, System.Boolean>)typeof(System.Func<System.Int64, System.Collections.Generic.List<System.Int64>, System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ET.MultiMap<System.Int64, System.Int64> @multiMap = (ET.MultiMap<System.Int64, System.Int64>)typeof(ET.MultiMap<System.Int64, System.Int64>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ET.ForeachHelper.ForEachFunc<System.Int64, System.Int64>(@multiMap, @func);

            return __ret;
        }

        static StackObject* ForEachFunc_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Int64, System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Boolean> @func = (System.Func<System.Int64, System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Boolean>)typeof(System.Func<System.Int64, System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>, System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            ET.MultiMap<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance> @multiMap = (ET.MultiMap<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(ET.MultiMap<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            ET.ForeachHelper.ForEachFunc<System.Int64, ILRuntime.Runtime.Intepreter.ILTypeInstance>(@multiMap, @func);

            return __ret;
        }



    }
}
