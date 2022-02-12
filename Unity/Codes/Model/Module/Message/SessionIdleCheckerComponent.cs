namespace ET
{
    /// <summary>
    /// 空闲检查员
    /// </summary>
    public class SessionIdleCheckerComponent: Entity, IAwake<int>, IDestroy
    {
        //重复时间
        public long RepeatedTimer;
    }
}