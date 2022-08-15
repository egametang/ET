namespace ET
{
    public interface ICallback
    {
        public int Id { get; set; }
    }
    
    public interface ICallbackType
    {
        public System.Type Type { get; }
    }
    
    public abstract class ACallbackHandler<A>: ICallbackType where A: struct, ICallback
    {
        public System.Type Type
        {
            get
            {
                return typeof (A);
            }
        }

        public abstract void Handle(A a);
    }
    
    public abstract class ACallbackHandler<A, T>: ICallbackType where A: struct, ICallback
    {
        public System.Type Type
        {
            get
            {
                return typeof (A);
            }
        }

        public abstract T Handle(A a);
    }
}