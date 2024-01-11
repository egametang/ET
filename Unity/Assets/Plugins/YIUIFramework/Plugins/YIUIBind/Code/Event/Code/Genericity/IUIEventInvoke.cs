namespace YIUIFramework
{
    public interface IUIEventInvoke
    {
        void Invoke();
    }

    public interface IUIEventInvoke<P1>
    {
        void Invoke(P1 p1);
    }

    public interface IUIEventInvoke<P1, P2>
    {
        void Invoke(P1 p1, P2 p2);
    }

    public interface IUIEventInvoke<P1, P2, P3>
    {
        void Invoke(P1 p1, P2 p2, P3 p3);
    }

    public interface IUIEventInvoke<P1, P2, P3, P4>
    {
        void Invoke(P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public interface IUIEventInvoke<P1, P2, P3, P4, P5>
    {
        void Invoke(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}