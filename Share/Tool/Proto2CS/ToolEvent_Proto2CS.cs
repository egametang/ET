using System;

namespace ET
{
    public static partial class SceneType
    {
        public const int Proto2CS = 2;
    }
    
    [Event(SceneType.Proto2CS)]
    public class ToolEvent_Proto2CS: AEvent<ToolScene, ToolEvent>
    {
        protected override async ETTask Run(ToolScene scene, ToolEvent a)
        {
            Options.Instance.Console = 1;
            Proto2CS.Export();
            await ETTask.CompletedTask;
        }
    }
}