using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Enviorment
{
    public class DelegateManager
    {
        List<DelegateMapNode> methods = new List<DelegateMapNode>();
        List<DelegateMapNode> functions = new List<DelegateMapNode>();
        IDelegateAdapter zeroParamMethodAdapter = new MethodDelegateAdapter();
        IDelegateAdapter dummyAdapter = new DummyDelegateAdapter();
        Dictionary<Type, Func<Delegate, Delegate>> clrDelegates = new Dictionary<Type, Func<Delegate, Delegate>>();
        Enviorment.AppDomain appdomain;
        public DelegateManager(Enviorment.AppDomain appdomain)
        {
            this.appdomain = appdomain;
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
            RegisterDelegateConvertor<Action<T1>>((dele) => dele);
        }

        public void RegisterMethodDelegate<T1, T2>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1, T2>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1, T2>>((dele) => dele);
        }

        public void RegisterMethodDelegate<T1, T2, T3>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1, T2, T3>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1, T2, T3>>((dele) => dele);
        }

        public void RegisterMethodDelegate<T1, T2, T3, T4>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new MethodDelegateAdapter<T1, T2, T3, T4>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
            methods.Add(node);
            RegisterDelegateConvertor<Action<T1, T2, T3, T4>>((dele) => dele);
        }

        public void RegisterFunctionDelegate<TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<TResult>();
            node.ParameterTypes = new Type[] { typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<TResult>>((dele) => dele);
        }

        public void RegisterFunctionDelegate<T1, TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<T1, TResult>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<T1, TResult>>((dele) => dele);
        }

        public void RegisterFunctionDelegate<T1, T2, TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<T1, T2, TResult>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<T1, T2, TResult>>((dele) => dele);
        }

        public void RegisterFunctionDelegate<T1, T2, T3, TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<T1, T2, T3, TResult>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<T1, T2, T3, TResult>>((dele) => dele);
        }

        public void RegisterFunctionDelegate<T1, T2, T3, T4, TResult>()
        {
            DelegateMapNode node = new Enviorment.DelegateManager.DelegateMapNode();
            node.Adapter = new FunctionDelegateAdapter<T1, T2, T3, T4, TResult>();
            node.ParameterTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(TResult) };
            functions.Add(node);
            RegisterDelegateConvertor<Func<T1, T2, T3, T4, TResult>>((dele) => dele);
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
                throw new KeyNotFoundException("Cannot find convertor for " + clrDelegateType);
        }

        internal IDelegateAdapter FindDelegateAdapter(ILTypeInstance instance, ILMethod method)
        {
            IDelegateAdapter res;
            if (method.ReturnType == appdomain.VoidType)
            {
                if (method.ParameterCount == 0)
                {
                    res = zeroParamMethodAdapter.Instantiate(appdomain, instance, method);
                    if (instance != null)
                        instance.SetDelegateAdapter(method, res);
                    return res;
                }
                foreach (var i in methods)
                {
                    if (i.ParameterTypes.Length == method.ParameterCount)
                    {
                        bool match = true;
                        for (int j = 0; j < method.ParameterCount; j++)
                        {
                            if (i.ParameterTypes[j] != method.Parameters[j].TypeForCLR)
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            res = i.Adapter.Instantiate(appdomain, instance, method);
                            if (instance != null)
                                instance.SetDelegateAdapter(method, res);
                            return res;
                        }
                    }
                }
            }
            else
            {
                foreach (var i in functions)
                {
                    if (i.ParameterTypes.Length == method.ParameterCount + 1)
                    {
                        bool match = true;
                        for (int j = 0; j < method.ParameterCount; j++)
                        {
                            if (i.ParameterTypes[j] != method.Parameters[j].TypeForCLR)
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            if (method.ReturnType.TypeForCLR == i.ParameterTypes[method.ParameterCount])
                            {
                                res = i.Adapter.Instantiate(appdomain, instance, method);
                                if (instance != null)
                                    instance.SetDelegateAdapter(method, res);
                                return res;
                            }
                        }
                    }
                }
            }

            res = dummyAdapter.Instantiate(appdomain, instance, method);
            if (instance != null)
                instance.SetDelegateAdapter(method, res);
            return res;
        }

        class DelegateMapNode
        {
            public IDelegateAdapter Adapter { get; set; }
            public Type[] ParameterTypes { get; set; }
        }
    }
}
