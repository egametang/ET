using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Other;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Enviorment
{
    public class DelegateManager
    {
        List<DelegateMapNode> methods = new List<DelegateMapNode>();
        List<DelegateMapNode> functions = new List<DelegateMapNode>();
        IDelegateAdapter zeroParamMethodAdapter = new MethodDelegateAdapter();
        IDelegateAdapter dummyAdapter = new DummyDelegateAdapter();
        Dictionary<Type, Func<Delegate, Delegate>> clrDelegates = new Dictionary<Type, Func<Delegate, Delegate>>(new ByReferenceKeyComparer<Type>());
        Func<Delegate, Delegate> defaultConverter;
        Enviorment.AppDomain appdomain;
        public DelegateManager(Enviorment.AppDomain appdomain)
        {
            this.appdomain = appdomain;
            defaultConverter = DefaultConverterStub;
        }

        static Delegate DefaultConverterStub(Delegate dele)
        {
            return dele;
        }

        public void RegisterDelegateConvertor<T>(Func<Delegate, Delegate> action)
        {
            var type = typeof(T);
            if (type.IsSubclassOf(typeof(Delegate)))
            {
                clrDelegates[type] = action;
            }
            else
                throw new NotSupportedException();
        }

        public void RegisterMethodDelegate<T1>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1>();
            node.ParameterTypes = new Type[] { typeof(T1) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1>>(defaultConverter);
        }

        public void RegisterMethodDelegate<T1, T2>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1, T2>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1, T2>>(defaultConverter);
        }

        public void RegisterMethodDelegate<T1, T2, T3>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1, T2, T3>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1, T2, T3>>(defaultConverter);
        }

        public void RegisterMethodDelegate<T1, T2, T3, T4>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1, T2, T3, T4>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1, T2, T3, T4>>(defaultConverter);
        }

#if NET_4_6 || NET_STANDARD_2_0
        public void RegisterMethodDelegate<T1, T2, T3, T4, T5>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1, T2, T3, T4, T5>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1, T2, T3, T4, T5>>(defaultConverter);
        }
#endif

        public void RegisterFunctionDelegate<TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<TResult>();
            node.ParameterTypes = new Type[] { typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<TResult>>(defaultConverter);
        }

        public void RegisterFunctionDelegate<T1, TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<T1, TResult>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<T1, TResult>>(defaultConverter);
        }

        public void RegisterFunctionDelegate<T1, T2, TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<T1, T2, TResult>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<T1, T2, TResult>>(defaultConverter);
        }

        public void RegisterFunctionDelegate<T1, T2, T3, TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<T1, T2, T3, TResult>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<T1, T2, T3, TResult>>(defaultConverter);
        }

        public void RegisterFunctionDelegate<T1, T2, T3, T4, TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<T1, T2, T3, T4, TResult>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<T1, T2, T3, T4, TResult>>(defaultConverter);
        }

        internal Delegate ConvertToDelegate(Type clrDelegateType, IDelegateAdapter adapter)
        {
            Func<Delegate, Delegate> func;
            if(adapter is DummyDelegateAdapter)
            {
                DelegateAdapter.ThrowAdapterNotFound(adapter.Method);
                return null;
            }
            if (clrDelegates.TryGetValue(clrDelegateType, out func))
            {
                return func(adapter.Delegate);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                string clsName, rName;
                bool isByRef;
                clrDelegateType.GetClassName(out clsName, out rName, out isByRef);
                sb.AppendLine("Cannot find convertor for " + rName);
                sb.AppendLine("Please add following code:");
                sb.Append("appdomain.DelegateManager.RegisterDelegateConvertor<");
                sb.Append(rName);
                sb.AppendLine(">((act) =>");
                sb.AppendLine("{");
                sb.Append("    return new ");
                sb.Append(rName);
                sb.Append("((");
                var mi = clrDelegateType.GetMethod("Invoke");
                bool first = true;
                foreach(var i in mi.GetParameters())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                        sb.Append(", ");
                    sb.Append(i.Name);
                }
                sb.AppendLine(") =>");
                sb.AppendLine("    {");
                if(mi.ReturnType != appdomain.VoidType.TypeForCLR)
                {
                    sb.Append("        return ((Func<");
                    first = true;
                    foreach (var i in mi.GetParameters())
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                            sb.Append(", ");
                        i.ParameterType.GetClassName(out clsName, out rName, out isByRef);
                        sb.Append(rName);
                    }
                    if (!first)
                        sb.Append(", ");
                    mi.ReturnType.GetClassName(out clsName, out rName, out isByRef);
                    sb.Append(rName);
                }
                else
                {
                    sb.Append("        ((Action<");
                    first = true;
                    foreach (var i in mi.GetParameters())
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                            sb.Append(", ");
                        i.ParameterType.GetClassName(out clsName, out rName, out isByRef);
                        sb.Append(rName);
                    }
                }
                sb.Append(">)act)(");
                first = true;
                foreach (var i in mi.GetParameters())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                        sb.Append(", ");
                    sb.Append(i.Name);
                }
                sb.AppendLine(");");
                sb.AppendLine("    });");
                sb.AppendLine("});");
                throw new KeyNotFoundException(sb.ToString());
            }
        }

        internal IDelegateAdapter FindDelegateAdapter(CLRType type, ILTypeInstance ins, ILMethod ilMethod)
        {
            IDelegateAdapter dele;
            if (ins != null)
            {
                dele = (ins).GetDelegateAdapter(ilMethod);
                if (dele == null)
                {
                    var invokeMethod =
                        type.GetMethod("Invoke",
                            ilMethod.ParameterCount);
                    if (invokeMethod == null && ilMethod.IsExtend)
                    {
                        invokeMethod = type.GetMethod("Invoke", ilMethod.ParameterCount - 1);
                    }
                    dele = appdomain.DelegateManager.FindDelegateAdapter(
                        ins, ilMethod, invokeMethod);
                }
            }
            else
            {
                if (ilMethod.DelegateAdapter == null)
                {
                    var invokeMethod = type.GetMethod("Invoke", ilMethod.ParameterCount);
                    ilMethod.DelegateAdapter = appdomain.DelegateManager.FindDelegateAdapter(null, ilMethod, invokeMethod);
                }
                dele = ilMethod.DelegateAdapter;
            }
            return dele;
        }

        /// <summary>
        /// ilMethod代表的delegate会赋值给method对应的delegate，一般两者参数类型都一致，
        /// 但新版本的支持泛型协变之后，有些时候会不一致，所以此处判断是用method判断，而不是用ilMethod判断
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ilMethod"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        internal IDelegateAdapter FindDelegateAdapter(ILTypeInstance instance, ILMethod ilMethod, IMethod method)
        {
            IDelegateAdapter res;
            var parameterCount = method.ParameterCount;
            var returnTypeForCLR = method.ReturnType.TypeForCLR;
            if (method.ReturnType == appdomain.VoidType)
            {
                if (parameterCount == 0)
                {
                    res = zeroParamMethodAdapter.Instantiate(appdomain, instance, ilMethod);
                    if (instance != null)
                        instance.SetDelegateAdapter(ilMethod, res);
                    return res;
                }
                foreach (var i in methods)
                {
                    var parameterTypes = i.ParameterTypes;
                    if (parameterTypes.Length == parameterCount)
                    {
                        bool match = true;
                        for (int j = 0; j < parameterCount; j++)
                        {
                            if (parameterTypes[j] != method.Parameters[j].TypeForCLR)
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            res = i.Adapter.Instantiate(appdomain, instance, ilMethod);
                            if (instance != null)
                                instance.SetDelegateAdapter(ilMethod, res);
                            return res;
                        }
                    }
                }
            }
            else
            {

                foreach (var i in functions)
                {
                    var parameterTypes = i.ParameterTypes;
                    if (parameterTypes.Length == parameterCount + 1)
                    {
                        bool match = true;
                        for (int j = 0; j < parameterCount; j++)
                        {
                            if (parameterTypes[j] != method.Parameters[j].TypeForCLR)
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            if (returnTypeForCLR == parameterTypes[parameterCount])
                            {
                                res = i.Adapter.Instantiate(appdomain, instance, ilMethod);
                                if (instance != null)
                                    instance.SetDelegateAdapter(ilMethod, res);
                                return res;
                            }
                        }
                    }
                }
            }

            res = dummyAdapter.Instantiate(appdomain, instance, ilMethod);
            if (instance != null)
                instance.SetDelegateAdapter(ilMethod, res);
            return res;
        }

        class DelegateMapNode
        {
            public IDelegateAdapter Adapter { get; set; }
            public Type[] ParameterTypes { get; set; }
        }
    }
}
