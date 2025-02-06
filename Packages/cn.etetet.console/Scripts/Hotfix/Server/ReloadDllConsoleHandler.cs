namespace ET.Server
{
    [ConsoleHandler(ConsoleMode.ReloadDll)]
    public class ReloadDllConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(Fiber fiber, ModeContex contex, string content)
        {
            await ETTask.CompletedTask;
            CodeLoader.Instance.Reload();
        }
    }
}