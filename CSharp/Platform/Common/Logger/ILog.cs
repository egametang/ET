namespace Common.Logger
{
    public interface ILog
    {
        void Trace(string message);
        void Debug(string message);
    }
}