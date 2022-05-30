namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class SessionStreamDispatcher: Entity, IAwake, IDestroy, ILoad
    {
        public static SessionStreamDispatcher Instance;
        public ISessionStreamDispatcher[] Dispatchers;
    }
}