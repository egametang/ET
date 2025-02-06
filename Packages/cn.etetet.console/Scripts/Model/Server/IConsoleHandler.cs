namespace ET.Server
{
    public interface IConsoleHandler
    {
        ETTask Run(Fiber fiber, ModeContex contex, string content);
    }
}