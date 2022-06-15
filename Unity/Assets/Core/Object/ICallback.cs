namespace ET
{

    public interface IAction
    {
        public void Handle();
    }

    public interface IAction<A>
    {
        public void Handle(A a);
    }

    public interface IAction<A, B>
    {
        public void Handle(A a, B b);
    }

    public interface IAction<A, B, C>
    {
        public void Handle(A a, B b, C c);
    }

    public interface IAction<A, B, C, D>
    {
        public void Handle(A a, B b, C c, D d);
    }

    public interface IFunc<T>
    {
        public T Handle();
    }

    public interface IFunc<T, A>
    {
        public T Handle(A a);
    }

    public interface IFunc<T, A, B>
    {
        public T Handle(A a, B b);
    }

    public interface IFunc<T, A, B, C>
    {
        public T Handle(A a, B b, C c);
    }

    public interface IFunc<T, A, B, C, D>
    {
        public T Handle(A a, B b, C c, D d);
    }
}