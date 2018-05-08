using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Enviorment
{
    public unsafe abstract class ValueTypeBinder
    {
        protected CLRType clrType;
        protected Enviorment.AppDomain domain;

        public CLRType CLRType
        {
            get { return clrType; }
            set
            {
                if (clrType == null)
                {
                    clrType = value;
                    domain = value.AppDomain;
                }
                else
                    throw new NotSupportedException();
            }
        }

        public abstract void CopyValueTypeToStack(object ins, StackObject* ptr, IList<object> mStack);

        public abstract object ToObject(StackObject* esp, IList<object> managedStack);

        public virtual void RegisterCLRRedirection(Enviorment.AppDomain appdomain)
        {

        }

        protected void CopyValueTypeToStack<K>(ref K ins, StackObject* esp, IList<object> mStack)
            where K : struct
        {
            switch (esp->ObjectType)
            {
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        var dst = *(StackObject**)&esp->Value;
                        var vb = ((CLRType)domain.GetType(dst->Value)).ValueTypeBinder as ValueTypeBinder<K>;
                        if (vb != null)
                        {
                            vb.CopyValueTypeToStack(ref ins, dst, mStack);
                        }
                        else
                            throw new NotSupportedException();
                    }
                    break;
                case ObjectTypes.Object:
                    mStack[esp->Value] = ins;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected void AssignFromStack<K>(ref K ins, StackObject* esp, IList<object> mStack)
            where K : struct
        {
            switch (esp->ObjectType)
            {
                case ObjectTypes.Null:
                    throw new NullReferenceException();
                case ObjectTypes.Object:
                    ins = (K)mStack[esp->Value];
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        var dst = *(StackObject**)&esp->Value;
                        var vb = ((CLRType)domain.GetType(dst->Value)).ValueTypeBinder as ValueTypeBinder<K>;
                        if (vb != null)
                        {
                            vb.AssignFromStack(ref ins, dst, mStack);
                        }
                        else
                            throw new NotSupportedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }        
    }

    public unsafe abstract class ValueTypeBinder<T> : ValueTypeBinder
        where T : struct
    {
        public override unsafe void CopyValueTypeToStack(object ins, StackObject* ptr, IList<object> mStack)
        {
            T obj = (T)ins;
            CopyValueTypeToStack(ref obj, ptr, mStack);
        }

        public abstract void CopyValueTypeToStack(ref T ins, StackObject* ptr, IList<object> mStack);

        public override unsafe object ToObject(StackObject* esp, IList<object> managedStack)
        {
            T obj = new T();
            AssignFromStack(ref obj, esp, managedStack);
            return obj;
        }

        public abstract void AssignFromStack(ref T ins, StackObject* ptr, IList<object> mStack);

        public T ParseValue(ILIntepreter intp, StackObject* ptr_of_this_method, IList<object> mStack)
        {
            T value = new T();

            var a = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            if (a->ObjectType == ObjectTypes.ValueTypeObjectReference)
            {
                var ptr = *(StackObject**)&a->Value;
                AssignFromStack(ref value, ptr, mStack);
                intp.FreeStackValueType(ptr_of_this_method);
            }
            else
            {
                value = (T)StackObject.ToObject(a, intp.AppDomain, mStack);
                intp.Free(ptr_of_this_method);
            }

            return value;
        }

        public void WriteBackValue(ILRuntime.Runtime.Enviorment.AppDomain domain, StackObject* ptr_of_this_method, IList<object> mStack, ref T instance_of_this_method)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch (ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        mStack[ptr_of_this_method->Value] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var obj = mStack[ptr_of_this_method->Value];
                        if (obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)obj)[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            var t = domain.GetType(obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref obj, instance_of_this_method);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = domain.GetType(ptr_of_this_method->Value);
                        if (t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method);
                        }
                    }
                    break;
                case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = mStack[ptr_of_this_method->Value] as T[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.ValueTypeObjectReference:
                    {
                        var dst = *((StackObject**)&ptr_of_this_method->Value);
                        CopyValueTypeToStack(ref instance_of_this_method, dst, mStack);
                    }
                    break;
            }
        }

        public void PushValue(ref T value, ILIntepreter intp, StackObject* ptr_of_this_method, IList<object> mStack)
        {
            intp.AllocValueType(ptr_of_this_method, clrType);
            var dst = *((StackObject**)&ptr_of_this_method->Value);
            CopyValueTypeToStack(ref value, dst, mStack);
        }
    }
}
