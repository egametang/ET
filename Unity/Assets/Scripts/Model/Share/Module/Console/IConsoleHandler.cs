namespace ET
{
    public interface IConsoleHandler
    {
        ETTask Run(ModeContex contex, string content);
    }
}