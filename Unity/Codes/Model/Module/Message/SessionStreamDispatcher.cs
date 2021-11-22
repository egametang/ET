namespace ET
{
    public class SessionStreamDispatcher: Entity
    {
        public static SessionStreamDispatcher Instance;
        public ISessionStreamDispatcher[] Dispatchers;
    }
}