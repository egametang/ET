namespace ET
{
    /// <summary>
    /// 会议 流调查员
    /// </summary>
    public class SessionStreamDispatcher: Entity, IAwake, IDestroy, ILoad
    {
        public static SessionStreamDispatcher Instance;
        public ISessionStreamDispatcher[] Dispatchers;
    }
}