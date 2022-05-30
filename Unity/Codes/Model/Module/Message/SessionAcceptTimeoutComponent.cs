namespace ET
{
    // 刚accept的session只持续5秒，必须通过验证，否则断开
    [ComponentOf(typeof(Session))]
    public class SessionAcceptTimeoutComponent: Entity, IAwake, IDestroy
    {
        public long Timer;
    }
}