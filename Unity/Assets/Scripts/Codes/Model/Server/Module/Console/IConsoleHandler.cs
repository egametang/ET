namespace ET.Server
{
    public interface IConsoleHandler
    {
        ETTask Run(ModeContex contex, string content);
    }
}