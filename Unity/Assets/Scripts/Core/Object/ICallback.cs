namespace ET
{

    public interface IAction
    {
        public void Handle();
    }

    public interface IAction<in A>
    {
        public void Handle(A a);
    }

    public interface IAction<in A, in B>
    {
        public void Handle(A a, B b);
    }

    public interface IAction<in A, in B, in C>
    {
        public void Handle(A a, B b, C c);
    }

    public interface IAction<in A, in B, in C, in D>
    {
        public void Handle(A a, B b, C c, D d);
    }

    public interface IFunc<out T>
    {
        public T Handle();
    }

    public interface IFunc<in A, out T>
    {
        public T Handle(A a);
    }

    public interface IFunc<in A, in B, out T>
    {
        public T Handle(A a, B b);
    }

    public interface IFunc<in A, in B, in C, out T>
    {
        public T Handle(A a, B b, C c);
    }

    public interface IFunc<in A, in B, in C, in D, out T>
    {
        public T Handle(A a, B b, C c, D d);
    }
}