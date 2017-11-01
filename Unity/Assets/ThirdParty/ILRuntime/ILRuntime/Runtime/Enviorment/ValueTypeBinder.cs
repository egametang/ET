using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Stack;

namespace ILRuntime.Runtime.Enviorment
{
    public unsafe abstract class ValueTypeBinder
    {
        CLRType clrType;
        Enviorment.AppDomain domain;

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
    }
}
