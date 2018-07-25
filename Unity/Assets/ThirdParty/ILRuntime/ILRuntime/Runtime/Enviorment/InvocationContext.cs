using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Enviorment
{
    public unsafe struct InvocationContext : IDisposable
    {
        StackObject* esp;
        AppDomain domain;
        ILIntepreter intp;
        ILMethod method;
        IList<object> mStack;
        bool invocated;
        int paramCnt;
        bool hasReturn;

        internal InvocationContext(ILIntepreter intp, ILMethod method)
        {
            var stack = intp.Stack;
            mStack = stack.ManagedStack;
            esp = stack.StackBase;
            stack.ResetValueTypePointer();

            this.domain = intp.AppDomain;
            this.intp = intp;
            this.method = method;

            invocated = false;
            paramCnt = 0;
            hasReturn = method.ReturnType != domain.VoidType;
        }

        public void PushBool(bool val)
        {
            PushInteger(val ? 1 : 0);
        }

        public void PushInteger(int val)
        {
            esp->ObjectType = ObjectTypes.Integer;
            esp->Value = val;
            esp->ValueLow = 0;

            esp++;
            paramCnt++;
        }

        public void PushInteger(long val)
        {
            esp->ObjectType = ObjectTypes.Integer;
            *(long*)&esp->Value = val;

            esp++;
            paramCnt++;
        }

        public void PushFloat(float val)
        {
            esp->ObjectType = ObjectTypes.Integer;
            *(float*)&esp->Value = val;

            esp++;
            paramCnt++;
        }

        public void PushDouble(double val)
        {
            esp->ObjectType = ObjectTypes.Integer;
            *(double*)&esp->Value = val;

            esp++;
            paramCnt++;
        }

        public void PushObject(object obj)
        {
            if (obj is CrossBindingAdaptorType)
                obj = ((CrossBindingAdaptorType)obj).ILInstance;
            esp = ILIntepreter.PushObject(esp, mStack, obj, true);
            paramCnt++;
        }

        public void Invoke()
        {
            if (invocated)
                throw new NotSupportedException("A invocation context can only be used once");
            invocated = true;
            var cnt = method.HasThis ? method.ParameterCount + 1 : method.ParameterCount;
            if (cnt != paramCnt)
                throw new ArgumentException("Argument count mismatch");
            bool unhandledException;
            esp = intp.Execute(method, esp, out unhandledException);
            esp--;
        }

        void CheckReturnValue()
        {
            if (!invocated)
                throw new NotSupportedException("You have to invocate first before you try to read the return value");
            if(!hasReturn)
                throw new NotSupportedException("The target method does not have a return value");
        }
        public int ReadInteger()
        {
            CheckReturnValue();
            return esp->Value;
        }

        public long ReadLong()
        {
            CheckReturnValue();
            return *(long*)&esp->Value;
        }

        public float ReadFloat()
        {
            CheckReturnValue();
            return *(float*)&esp->Value;
        }

        public double ReadDouble()
        {
            CheckReturnValue();
            return *(double*)&esp->Value;
        }

        public bool ReadBool()
        {
            CheckReturnValue();
            return esp->Value == 1;
        }

        public T ReadObject<T>()
        {
            CheckReturnValue();
            return (T)typeof(T).CheckCLRTypes(StackObject.ToObject(esp, domain, mStack));
        }

        public void Dispose()
        {
            domain.FreeILIntepreter(intp);

            esp = null;
            intp = null;
            domain = null;
            method = null;
            mStack = null;
        }
    }
}
