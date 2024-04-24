namespace ET
{
    public interface ISingletonAwake
    {
        void Awake();
    }
    
    public interface ISingletonAwake<A>
    {
        void Awake(A a);
    }
    
    public interface ISingletonAwake<A, B>
    {
        void Awake(A a, B b);
    }
    
    public interface ISingletonAwake<A, B, C>
    {
        void Awake(A a, B b, C c);
    }
}