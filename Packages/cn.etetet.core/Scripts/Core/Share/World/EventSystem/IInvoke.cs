using System;

namespace ET
{
    public interface IInvoke
    {
        Type Type { get; }
    }
    
    public abstract class AInvokeHandler<A>: HandlerObject, IInvoke where A: struct
    {
        public Type Type
        {
            get
            {
                return typeof (A);
            }
        }

        public abstract void Handle(A args);
    }
    
    public abstract class AInvokeHandler<A, T>: HandlerObject, IInvoke where A: struct
    {
        public Type Type
        {
            get
            {
                return typeof (A);
            }
        }

        public abstract T Handle(A args);
    }
}