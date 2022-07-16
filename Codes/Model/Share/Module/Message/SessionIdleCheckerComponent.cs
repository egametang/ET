namespace ET
{
    [ComponentOf(typeof(Session))]
    public class SessionIdleCheckerComponent: Entity, IAwake<int>, IDestroy
    {
        public long RepeatedTimer;
    }
}