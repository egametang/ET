using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Runtime.Enviorment
{
    #region Functions
    public class CrossBindingFunctionInfo<T1, T2, T3, T4, T5, TResult> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        {
            InvocationContext.GetInvocationType<T1>(),
            InvocationContext.GetInvocationType<T2>(),
            InvocationContext.GetInvocationType<T3>(),
            InvocationContext.GetInvocationType<T4>(),
            InvocationContext.GetInvocationType<T5>(),
            InvocationContext.GetInvocationType<TResult>(),
        };
        static Type[] pTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };

        public delegate TResult InvocationDelegate(ILTypeInstance instance, T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public CrossBindingFunctionInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return typeof(TResult); } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public TResult Invoke(ILTypeInstance instance, T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                TResult res = default(TResult);
                try
                {
                    if (DoInvoke != null)
                        res = DoInvoke(instance, arg, arg2, arg3, arg4, arg5);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.PushParameter(piTypes[1], arg2);
                            ctx.PushParameter(piTypes[2], arg3);
                            ctx.PushParameter(piTypes[3], arg4);
                            ctx.PushParameter(piTypes[4], arg5);
                            ctx.Invoke();
                            res = ctx.ReadResult<TResult>(piTypes[5]);
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
                return res;
            }
            else
                return default(TResult);
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingFunctionInfo<T1, T2, T3, T4, TResult> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        {
            InvocationContext.GetInvocationType<T1>(),
            InvocationContext.GetInvocationType<T2>(),
            InvocationContext.GetInvocationType<T3>(),
            InvocationContext.GetInvocationType<T4>(),
            InvocationContext.GetInvocationType<TResult>(),
        };
        static Type[] pTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };

        public delegate TResult InvocationDelegate(ILTypeInstance instance, T1 arg, T2 arg2, T3 arg3, T4 arg4);

        public CrossBindingFunctionInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return typeof(TResult); } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public TResult Invoke(ILTypeInstance instance, T1 arg, T2 arg2, T3 arg3, T4 arg4)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                TResult res = default(TResult);
                try
                {
                    if (DoInvoke != null)
                        res = DoInvoke(instance, arg, arg2, arg3, arg4);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.PushParameter(piTypes[1], arg2);
                            ctx.PushParameter(piTypes[2], arg3);
                            ctx.PushParameter(piTypes[3], arg4);
                            ctx.Invoke();
                            res = ctx.ReadResult<TResult>(piTypes[4]);
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
                return res;
            }
            else
                return default(TResult);
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingFunctionInfo<T1, T2, T3, TResult> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        {
            InvocationContext.GetInvocationType<T1>(),
            InvocationContext.GetInvocationType<T2>(),
            InvocationContext.GetInvocationType<T3>(),
            InvocationContext.GetInvocationType<TResult>(),
        };
        static Type[] pTypes = new Type[] { typeof(T1), typeof(T2), typeof(T3) };

        public delegate TResult InvocationDelegate(ILTypeInstance instance, T1 arg, T2 arg2, T3 arg3);

        public CrossBindingFunctionInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return typeof(TResult); } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public TResult Invoke(ILTypeInstance instance, T1 arg, T2 arg2, T3 arg3)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                TResult res = default(TResult);
                try
                {
                    if (DoInvoke != null)
                        res = DoInvoke(instance, arg, arg2, arg3);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.PushParameter(piTypes[1], arg2);
                            ctx.PushParameter(piTypes[2], arg3);
                            ctx.Invoke();
                            res = ctx.ReadResult<TResult>(piTypes[3]);
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
                return res;
            }
            else
                return default(TResult);
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingFunctionInfo<T1, T2, TResult> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        {
            InvocationContext.GetInvocationType<T1>(),
            InvocationContext.GetInvocationType<T2>(),
            InvocationContext.GetInvocationType<TResult>(),
        };
        static Type[] pTypes = new Type[] { typeof(T1), typeof(T2) };

        public delegate TResult InvocationDelegate(ILTypeInstance instance, T1 arg, T2 arg2);

        public CrossBindingFunctionInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return typeof(TResult); } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public TResult Invoke(ILTypeInstance instance, T1 arg, T2 arg2)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                TResult res = default(TResult);
                try
                {
                    if (DoInvoke != null)
                        res = DoInvoke(instance, arg, arg2);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.PushParameter(piTypes[1], arg2);
                            ctx.Invoke();
                            res = ctx.ReadResult<TResult>(piTypes[2]);
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
                return res;
            }
            else
                return default(TResult);
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingFunctionInfo<T1, TResult> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        {
            InvocationContext.GetInvocationType<T1>(),
            InvocationContext.GetInvocationType<TResult>(),
        };
        static Type[] pTypes = new Type[] { typeof(T1) };

        public delegate TResult InvocationDelegate(ILTypeInstance instance, T1 arg);

        public CrossBindingFunctionInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return typeof(TResult); } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public TResult Invoke(ILTypeInstance instance, T1 arg)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                TResult res = default(TResult);
                try
                {
                    if (DoInvoke != null)
                        res = DoInvoke(instance, arg);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.Invoke();
                            res = ctx.ReadResult<TResult>(piTypes[1]);
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
                return res;
            }
            else
                return default(TResult);
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingFunctionInfo<TResult> : CrossBindingMethodInfo
    {
        static InvocationTypes rType = InvocationContext.GetInvocationType<TResult>();

        public delegate TResult InvocationDelegate(ILTypeInstance instance);

        public CrossBindingFunctionInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return typeof(TResult); } }

        public InvocationDelegate DoInvoke { get; set; }

        public new TResult Invoke(ILTypeInstance instance)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;                
                TResult res = default(TResult);
                try
                {
                    if (DoInvoke != null)
                        res = DoInvoke(instance);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.Invoke();
                            res = ctx.ReadResult<TResult>(rType);
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
                return res;
            }
            else
                return default(TResult);
        }
    }
    #endregion

    #region Methods
    public class CrossBindingMethodInfo<T, T2, T3, T4, T5> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        {
            InvocationContext.GetInvocationType<T>(),
            InvocationContext.GetInvocationType<T2>(),
            InvocationContext.GetInvocationType<T3>(),
            InvocationContext.GetInvocationType<T4>(),
            InvocationContext.GetInvocationType<T5>(),
        };
        static Type[] pTypes = new Type[] { typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };

        public delegate void InvocationDelegate(ILTypeInstance instance, T arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public CrossBindingMethodInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return null; } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public void Invoke(ILTypeInstance instance, T arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                try
                {
                    if (DoInvoke != null)
                        DoInvoke(instance, arg, arg2, arg3, arg4, arg5);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.PushParameter(piTypes[1], arg2);
                            ctx.PushParameter(piTypes[2], arg3);
                            ctx.PushParameter(piTypes[3], arg4);
                            ctx.PushParameter(piTypes[4], arg5);
                            ctx.Invoke();
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
            }
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingMethodInfo<T, T2, T3, T4> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        {
            InvocationContext.GetInvocationType<T>(),
            InvocationContext.GetInvocationType<T2>(),
            InvocationContext.GetInvocationType<T3>(),
            InvocationContext.GetInvocationType<T4>(),
        };
        static Type[] pTypes = new Type[] { typeof(T), typeof(T2), typeof(T3), typeof(T4) };

        public delegate void InvocationDelegate(ILTypeInstance instance, T arg, T2 arg2, T3 arg3, T4 arg4);

        public CrossBindingMethodInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return null; } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public void Invoke(ILTypeInstance instance, T arg, T2 arg2, T3 arg3, T4 arg4)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                try
                {
                    if (DoInvoke != null)
                        DoInvoke(instance, arg, arg2, arg3, arg4);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.PushParameter(piTypes[1], arg2);
                            ctx.PushParameter(piTypes[2], arg3);
                            ctx.PushParameter(piTypes[3], arg4);
                            ctx.Invoke();
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
            }
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingMethodInfo<T, T2, T3> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        {
            InvocationContext.GetInvocationType<T>(),
            InvocationContext.GetInvocationType<T2>(),
            InvocationContext.GetInvocationType<T3>(),
        };
        static Type[] pTypes = new Type[] { typeof(T), typeof(T2), typeof(T3) };

        public delegate void InvocationDelegate(ILTypeInstance instance, T arg, T2 arg2, T3 arg3);

        public CrossBindingMethodInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return null; } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public void Invoke(ILTypeInstance instance, T arg, T2 arg2, T3 arg3)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                try
                {
                    if (DoInvoke != null)
                        DoInvoke(instance, arg, arg2, arg3);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.PushParameter(piTypes[1], arg2);
                            ctx.PushParameter(piTypes[2], arg3);
                            ctx.Invoke();
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
            }
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingMethodInfo<T, T2> : CrossBindingMethodInfo
    {
        static InvocationTypes[] piTypes = new InvocationTypes[]
        { 
            InvocationContext.GetInvocationType<T>(),
            InvocationContext.GetInvocationType<T2>(),
        };
        static Type[] pTypes = new Type[] { typeof(T), typeof(T2) };


        public delegate void InvocationDelegate(ILTypeInstance instance, T arg, T2 arg2);
        
        public CrossBindingMethodInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return null; } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public void Invoke(ILTypeInstance instance, T arg, T2 arg2)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                try
                {
                    if (DoInvoke != null)
                        DoInvoke(instance, arg, arg2);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes[0], arg);
                            ctx.PushParameter(piTypes[1], arg2);
                            ctx.Invoke();
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
            }
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }

    public class CrossBindingMethodInfo<T> : CrossBindingMethodInfo
    {
        static InvocationTypes piTypes = InvocationContext.GetInvocationType<T>();
        static Type[] pTypes = new Type[] { typeof(T) };

        public delegate void InvocationDelegate(ILTypeInstance instance, T arg);
        public CrossBindingMethodInfo(string name)
            : base(name)
        {

        }

        protected override Type ReturnType { get { return null; } }

        protected override Type[] Parameters { get { return pTypes; } }

        public InvocationDelegate DoInvoke { get; set; }

        public void Invoke(ILTypeInstance instance, T arg)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                try
                {
                    if (DoInvoke != null)
                        DoInvoke(instance, arg);
                    else
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(instance);
                            ctx.PushParameter(piTypes, arg);
                            ctx.Invoke();
                        }
                    }
                }
                finally
                {
                    invoking = false;
                }
            }
        }

        public override void Invoke(ILTypeInstance instance)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    public class CrossBindingMethodInfo
    {
        public string Name { get; private set; }
        protected AppDomain domain;
        protected IMethod method;
        private IMethod baseMethod;
        private bool methodGot;
        protected bool invoking;
        static List<IType> emptyParam = new List<IType>();

        public CrossBindingMethodInfo(string name)
        {
            Name = name;
        }

        protected virtual Type[] Parameters { get { return null; } }
        protected virtual Type ReturnType { get { return null; } }

        public bool CheckShouldInvokeBase(ILTypeInstance ins)
        {
            EnsureMethod(ins);
            return method == null || invoking;
        }

        protected void EnsureMethod(ILTypeInstance ins)
        {
            if (!methodGot)
            {
                var ilType = ins.Type;
                domain = ilType.AppDomain;
                methodGot = true;
                List<IType> param = null;
                IType rt = null;
                if (Parameters != null)
                {
                    param = new List<IType>();
                    foreach (var i in Parameters)
                    {
                        if (i.IsByRef)
                        {
                            var type = domain.GetType(i.GetElementType());
                            param.Add(type.MakeByRefType());
                        }
                        else
                            param.Add(domain.GetType(i));
                    }
                }
                else
                    param = emptyParam;
                if (ReturnType != null)
                    rt = domain.GetType(ReturnType);
                if (ilType.FirstCLRBaseType != null)
                    baseMethod = ilType.FirstCLRBaseType.GetMethod(Name, param, null, rt);
                if (ilType.FirstCLRInterface != null)
                    baseMethod = ilType.FirstCLRInterface.GetMethod(Name, param, null, rt);
                if (baseMethod == null)
                    method = ilType.GetMethod(Name, param, null, rt);
            }
            if (baseMethod != null)
            {
                method = ins.Type.GetVirtualMethod(baseMethod);
                if (method is CLRMethod)
                    method = null;
            }
        }

        public virtual void Invoke(ILTypeInstance instance)
        {
            EnsureMethod(instance);
            if (method != null)
            {
                invoking = true;
                domain.Invoke(method, instance, null);
                invoking = false;
            }
        }
    }
}
