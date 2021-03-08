namespace ET
{
    // 刚accept的session只持续5秒，必须通过验证，否则断开
    public class SessionAcceptTimeoutComponent: Entity
    {
        public long Timer;
    }
}