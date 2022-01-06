namespace ET
{
    public class SessionIdleCheckerComponent: Entity, IAwake<int>, IDestroy
    {
        public long RepeatedTimer;
    }
}