using ET;

namespace YIUIFramework
{
    public interface IUITaskEventInvoke
    {
        ETTask Invoke();
    }

    public interface IUITaskEventInvoke<P1>
    {
        ETTask Invoke(P1 p1);
    }

    public interface IUITaskEventInvoke<P1, P2>
    {
        ETTask Invoke(P1 p1, P2 p2);
    }

    public interface IUITaskEventInvoke<P1, P2, P3>
    {
        ETTask Invoke(P1 p1, P2 p2, P3 p3);
    }

    public interface IUITaskEventInvoke<P1, P2, P3, P4>
    {
        ETTask Invoke(P1 p1, P2 p2, P3 p3, P4 p4);
    }

    public interface IUITaskEventInvoke<P1, P2, P3, P4, P5>
    {
        ETTask Invoke(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
    }
}