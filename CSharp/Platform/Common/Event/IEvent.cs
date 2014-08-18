namespace Common.Event
{
    public interface IEvent
    {
        void Trigger(Env env);
    }
}