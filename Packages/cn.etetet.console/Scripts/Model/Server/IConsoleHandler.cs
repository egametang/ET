namespace ET
{
    public interface IConsoleHandler
    {
        ETTask Run(Fiber fiber, ModeContex contex, string content);
    }
}