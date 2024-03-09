using System;

namespace ET
{
    public static partial class SceneType
    {
        public const int ExcelExporter = 1;
    }
    
    [Event(SceneType.ExcelExporter)]
    public class ToolEvent_ExcelExporter: AEvent<ToolScene, ToolEvent>
    {
        protected override async ETTask Run(ToolScene scene, ToolEvent a)
        {
            Options.Instance.Console = 1;
            ExcelExporter.Export();
            await ETTask.CompletedTask;
        }
    }
}