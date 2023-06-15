namespace ET
{
    public interface IVProcessScheduler
    {
        void Start();
        void Stop();
        void Add(VProcess vProcess);
    }
}