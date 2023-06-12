namespace ET
{
    public interface ISingletonScheduler
    {
        void StartScheduler();
        
        void StopScheduler();

        void Add(Process process);
    }
}