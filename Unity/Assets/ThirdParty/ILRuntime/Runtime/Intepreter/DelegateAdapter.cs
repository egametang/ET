using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Stack;
using ILRuntime.Other;
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Runtime.Intepreter
{
    #region Functions
    class FunctionDelegateAdapter<TResult> : DelegateAdapter
    {
        Func<TResult> action;

        static InvocationTypes[] pTypes;
        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod()
        {
            using (var ctx = BeginInvoke())
            {
                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp); 
                return ctx.ReadResult<TResult>(pTypes[0]);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Func<TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<TResult>)dele;
        }
    }

    class FunctionDelegateAdapter<T1, TResult> : DelegateAdapter
    {
        Func<T1, TResult> action;

        static InvocationTypes[] pTypes;
        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod(T1 p1)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pTypes[0], p1);

                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp);
                return ctx.ReadResult<TResult>(pTypes[1]);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<T1, TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<T1, TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Func<T1, TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<T1, TResult>)dele;
        }
    }

    class FunctionDelegateAdapter<T1, T2, TResult> : DelegateAdapter
    {
        Func<T1, T2, TResult> action;

        static InvocationTypes[] pTypes;
        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod(T1 p1, T2 p2)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);

                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp);
                return ctx.ReadResult<TResult>(pTypes[2]);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<T1, T2, TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<T1, T2, TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Func<T1, T2, TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<T1, T2, TResult>)dele;
        }
    }

    class FunctionDelegateAdapter<T1, T2, T3, TResult> : DelegateAdapter
    {
        Func<T1, T2, T3, TResult> action;

        static InvocationTypes[] pTypes;

        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod(T1 p1, T2 p2, T3 p3)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);

                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp);
                return ctx.ReadResult<TResult>(pTypes[3]);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<T1, T2, T3, TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<T1, T2, T3, TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }
        public override void Combine(Delegate dele)
        {
            action += (Func<T1, T2, T3, TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<T1, T2, T3, TResult>)dele;
        }
    }

    class FunctionDelegateAdapter<T1, T2, T3, T4, TResult> : DelegateAdapter
    {
        Func<T1, T2, T3, T4, TResult> action;

        static InvocationTypes[] pTypes;

        static FunctionDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
                InvocationContext.GetInvocationType<T4>(),
                InvocationContext.GetInvocationType<TResult>(),
            };
        }
        public FunctionDelegateAdapter()
        {

        }

        private FunctionDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe TResult InvokeILMethod(T1 p1, T2 p2, T3 p3, T4 p4)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);
                ctx.PushParameter(pTypes[3], p4);

                var esp = ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
                ctx.SetInvoked(esp);
                return ctx.ReadResult<TResult>(pTypes[4]);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new FunctionDelegateAdapter<T1, T2, T3, T4, TResult>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new FunctionDelegateAdapter<T1, T2, T3, T4, TResult>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Func<T1, T2, T3, T4, TResult>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Func<T1, T2, T3, T4, TResult>)dele;
        }
    }
    #endregion

    #region Methods
    class MethodDelegateAdapter<T1> : DelegateAdapter
    {
        Action<T1> action;
        static InvocationTypes pType;

        static MethodDelegateAdapter()
        {
            pType = InvocationContext.GetInvocationType<T1>();
        }

        public MethodDelegateAdapter()
        {
            
        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pType, p1);
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1>)dele;
        }
    }

    class MethodDelegateAdapter<T1, T2> : DelegateAdapter
    {
        Action<T1, T2> action;

        static InvocationTypes[] pTypes;

        static MethodDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
            };
        }
        public MethodDelegateAdapter()
        {

        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1, T2 p2)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1, T2>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1, T2>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1, T2>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1, T2>)dele;
        }
    }

    class MethodDelegateAdapter<T1, T2, T3> : DelegateAdapter
    {
        Action<T1, T2, T3> action;

        static InvocationTypes[] pTypes;

        static MethodDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
            };
        }
        public MethodDelegateAdapter()
        {

        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1, T2 p2, T3 p3)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1, T2, T3>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1, T2, T3>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1, T2, T3>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1, T2, T3>)dele;
        }
    }

    class MethodDelegateAdapter<T1, T2, T3, T4> : DelegateAdapter
    {
        Action<T1, T2, T3, T4> action;

        static InvocationTypes[] pTypes;

        static MethodDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
                InvocationContext.GetInvocationType<T4>(),
            };
        }
        public MethodDelegateAdapter()
        {

        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1, T2 p2, T3 p3, T4 p4)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);
                ctx.PushParameter(pTypes[3], p4);
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1, T2, T3, T4>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1, T2, T3, T4>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1, T2, T3, T4>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1, T2, T3, T4>)dele;
        }
    }

#if NET_4_6 || NET_STANDARD_2_0
    class MethodDelegateAdapter<T1, T2, T3, T4, T5> : DelegateAdapter
    {
        Action<T1, T2, T3, T4, T5> action;

        static InvocationTypes[] pTypes;

        static MethodDelegateAdapter()
        {
            pTypes = new InvocationTypes[]
            {
                InvocationContext.GetInvocationType<T1>(),
                InvocationContext.GetInvocationType<T2>(),
                InvocationContext.GetInvocationType<T3>(),
                InvocationContext.GetInvocationType<T4>(),
                InvocationContext.GetInvocationType<T5>(),
            };
        }
        public MethodDelegateAdapter()
        {

        }

        private MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            using (var ctx = BeginInvoke())
            {
                ctx.PushParameter(pTypes[0], p1);
                ctx.PushParameter(pTypes[1], p2);
                ctx.PushParameter(pTypes[2], p3);
                ctx.PushParameter(pTypes[3], p4);
                ctx.PushParameter(pTypes[4], p5);
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter<T1, T2, T3, T4, T5>(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter<T1, T2, T3, T4, T5>(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action<T1, T2, T3, T4, T5>)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action<T1, T2, T3, T4, T5>)dele;
        }
    }
#endif

    class MethodDelegateAdapter : DelegateAdapter
    {
        Action action;
        
        public MethodDelegateAdapter()
        {

        }

        protected MethodDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            action = InvokeILMethod;
        }

        public override Delegate Delegate
        {
            get
            {
                return action;
            }
        }

        unsafe void InvokeILMethod()
        {
            using(var ctx = BeginInvoke())
            {
                ILInvoke(ctx.Intepreter, ctx.ESP, ctx.ManagedStack);
            }
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new MethodDelegateAdapter(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new MethodDelegateAdapter(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            action += (Action)dele;
        }

        public override void Remove(Delegate dele)
        {
            action -= (Action)dele;
        }
    }

    class DummyDelegateAdapter : DelegateAdapter
    {
        public DummyDelegateAdapter()
        {

        }

        protected DummyDelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
            : base(appdomain, instance, method)
        {
            
        }

        public override Delegate Delegate
        {
            get
            {
                ThrowAdapterNotFound(method);
                return null;
            }
        }

        void InvokeILMethod()
        {
            if (method.HasThis)
                appdomain.Invoke(method, instance, null);
            else
                appdomain.Invoke(method, null, null);
        }

        public override IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            return new DummyDelegateAdapter(appdomain, instance, method);
        }

        public override IDelegateAdapter Clone()
        {
            var res = new DummyDelegateAdapter(appdomain, instance, method);
            res.isClone = true;
            return res;
        }

        public override void Combine(Delegate dele)
        {
            ThrowAdapterNotFound(method);
        }

        public override void Remove(Delegate dele)
        {
            ThrowAdapterNotFound(method);
        }
    }
    #endregion

    abstract class DelegateAdapter : ILTypeInstance, IDelegateAdapter
    {
        protected ILMethod method;
        protected ILTypeInstance instance;
        protected Enviorment.AppDomain appdomain;
        Dictionary<Type, Delegate> converters;
        IDelegateAdapter next;
        protected bool isClone;

        public abstract Delegate Delegate { get; }

        public IDelegateAdapter Next { get { return next; } }

        public ILTypeInstance Instance { get { return instance; } }

        public ILMethod Method { get { return method; } }

        protected DelegateAdapter() { }

        protected DelegateAdapter(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method)
        {
            this.appdomain = appdomain;
            this.instance = instance;
            this.method = method;
            CLRInstance = this;
        }

        public override bool IsValueType
        {
            get
            {
                return false;
            }
        }

        unsafe protected InvocationContext BeginInvoke()
        {
            var ctx = appdomain.BeginInvoke(method);
            *ctx.ESP = default(StackObject);
            ctx.ESP++;//required to simulate delegate invocation
            return ctx;
        }

        public unsafe StackObject* ILInvoke(ILIntepreter intp, StackObject* esp, IList<object> mStack)
        {
            var ebp = esp;
            esp = ILInvokeSub(intp, esp, mStack);
            return ClearStack(intp, esp, ebp, mStack);
        }

        unsafe StackObject* ILInvokeSub(ILIntepreter intp, StackObject* esp, IList<object> mStack)
        {
            var ebp = esp;
            bool unhandled;
            if (method.HasThis)
                esp = ILIntepreter.PushObject(esp, mStack, instance);
            int paramCnt = method.ParameterCount;
            bool useRegister = method.ShouldUseRegisterVM;
            for (int i = paramCnt; i > 0; i--)
            {
                intp.CopyToStack(esp, Minus(ebp, i), mStack);
                if (esp->ObjectType < ObjectTypes.Object && useRegister)
                    mStack.Add(null);
                esp++;
            }
            StackObject* ret;
            if (useRegister)
                ret = intp.ExecuteR(method, esp, out unhandled);
            else
                ret = intp.Execute(method, esp, out unhandled);
            if (next != null)
            {
                if (method.ReturnType != appdomain.VoidType)
                {
                    intp.Free(ret - 1);//Return value for multicast delegate doesn't make sense, only return the last one's value
                }
                DelegateAdapter n = (DelegateAdapter)next;
                ret = n.ILInvokeSub(intp, ebp, mStack);

            }
            return ret;
        }

        unsafe StackObject* ClearStack(ILIntepreter intp, StackObject* esp, StackObject* ebp, IList<object> mStack)
        {
            int paramCnt = method.ParameterCount;
            object retObj = null;
            StackObject retSObj = StackObject.Null;
            bool hasReturn = method.ReturnType != appdomain.VoidType;
            if (hasReturn)
            {
                var ret = esp - 1;
                retSObj = *ret;
                if(ret->ObjectType>= ObjectTypes.Object)
                {
                    retObj = mStack[ret->Value];
                    if(retObj == null)
                    {
                        retSObj.ObjectType = ObjectTypes.Null;
                        retSObj.Value = -1;
                        retSObj.ValueLow = 0;
                    }

                    intp.Free(ret);
                }
            }
            for (int i = 1; i <= paramCnt; i++)
            {
                intp.Free(ebp - i);
            }
            var returnVal = Minus(ebp, paramCnt + 1);
            intp.Free(returnVal);//Free delegateInstance
            if (hasReturn)
            {
                *returnVal = retSObj;
                if(retObj != null)
                {
                    returnVal->Value = mStack.Count;
                    mStack.Add(retObj);
                }
                returnVal++;
            }
            return returnVal;
        }

        public abstract IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method);

        public new abstract IDelegateAdapter Clone();

        public bool IsClone
        {
            get
            {
                return isClone;
            }
        }

        public virtual void Combine(IDelegateAdapter adapter)
        {
            if (next != null)
                next.Combine(adapter);
            else
                next = adapter;
        }

        public abstract void Combine(Delegate dele);

        public virtual void Remove(IDelegateAdapter adapter)
        {
            if (next != null)
            {
                if (next.Equals(adapter))
                {
                    next = ((DelegateAdapter)next).next;
                }
                else
                    next.Remove(adapter);
            }
        }

        public abstract void Remove(Delegate dele);

        public virtual bool Equals(IDelegateAdapter adapter)
        {
            if (adapter is DelegateAdapter)
            {
                DelegateAdapter b = (DelegateAdapter)adapter;
                return instance == b.instance && method == b.method;
            }
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is DelegateAdapter)
            {
                DelegateAdapter b = (DelegateAdapter)obj;
                return instance == b.instance && method == b.method;
            }
            return false;
        }

        public virtual bool Equals(Delegate dele)
        {
            return Delegate == dele;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return method.ToString();
        }

        public override bool CanAssignTo(IType type)
        {
            if (type.IsDelegate)
            {
                var im = type.GetMethod("Invoke", method.ParameterCount);
                if (im.IsDelegateInvoke)
                {
                    if (im.ParameterCount == method.ParameterCount && im.ReturnType == method.ReturnType)
                    {
                        for (int i = 0; i < im.ParameterCount; i++)
                        {
                            if (im.Parameters[i] != method.Parameters[i])
                                return false;
                        }
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public Delegate GetConvertor(Type type)
        {
            if (converters == null)
                converters = new Dictionary<System.Type, Delegate>(new ByReferenceKeyComparer<Type>());
            Delegate res;
            if (converters.TryGetValue(type, out res))
                return res;
            else
            {
                res = appdomain.DelegateManager.ConvertToDelegate(type, this);
                converters[type] = res;
                return res;
            }
        }

        unsafe StackObject* Minus(StackObject* a, int b)
        {
            return (StackObject*)((long)a - sizeof(StackObject) * b);
        }

        public static void ThrowAdapterNotFound(IMethod method)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Cannot find Delegate Adapter for:");
            sb.Append(method.ToString());
            string clsName, rName;
            bool isByRef;
            if (method.ReturnType.Name != "Void" || method.ParameterCount > 0)
            {
                sb.AppendLine(", Please add following code:");
                if (method.ReturnType.Name == "Void")
                {
                    sb.Append("appdomain.DelegateManager.RegisterMethodDelegate<");
                    bool first = true;
                    foreach(var i in method.Parameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            sb.Append(", ");
                        }
                        i.TypeForCLR.GetClassName(out clsName, out rName, out isByRef);
                        sb.Append(rName);                        
                    }
                    sb.AppendLine(">();");
                }
                else
                {
                    sb.Append("appdomain.DelegateManager.RegisterFunctionDelegate<");
                    bool first = true;
                    foreach (var i in method.Parameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            sb.Append(", ");
                        }
                        i.TypeForCLR.GetClassName(out clsName, out rName, out isByRef);
                        sb.Append(rName);
                    }
                    if (!first)
                        sb.Append(", ");
                    method.ReturnType.TypeForCLR.GetClassName(out clsName, out rName, out isByRef);
                    sb.Append(rName);
                    sb.AppendLine(">();");
                }
            }
            throw new KeyNotFoundException(sb.ToString());
        }
    }

    unsafe interface IDelegateAdapter
    {
        Delegate Delegate { get; }
        IDelegateAdapter Next { get; }
        ILTypeInstance Instance { get; }
        ILMethod Method { get; }
        StackObject* ILInvoke(ILIntepreter intp, StackObject* esp, IList<object> mStack);
        IDelegateAdapter Instantiate(Enviorment.AppDomain appdomain, ILTypeInstance instance, ILMethod method);
        bool IsClone { get; }
        IDelegateAdapter Clone();
        Delegate GetConvertor(Type type);
        void Combine(IDelegateAdapter adapter);
        void Combine(Delegate dele);
        void Remove(IDelegateAdapter adapter);
        void Remove(Delegate dele);
        bool Equals(IDelegateAdapter adapter);
        bool Equals(Delegate dele);
    }
}
