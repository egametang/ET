namespace ET
{
    public interface IVProcessScheduler: IVProcessSingletonAwake
    {
        int Create(int vProcessId = 0);
    }
}