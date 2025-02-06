using System;

namespace ET.Server
{
    [ConsoleHandler(ConsoleMode.ReloadConfig)]
    public class ReloadConfigConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(Fiber fiber, ModeContex contex, string content)
        {
            switch (content)
            {
                case ConsoleMode.ReloadConfig:
                    contex.Parent.RemoveComponent<ModeContex>();
                    Log.Console("C must have config name, like: C UnitConfig");
                    break;
                default:
                    string[] ss = content.Split(" ");
                    string configName = ss[1];
                    string category = $"{configName}Category";
                    Type type = CodeTypes.Instance.GetType($"ET.{category}");
                    if (type == null)
                    {
                        Log.Console($"reload config but not find {category}");
                        return;
                    }
                    await ConfigLoader.Instance.Reload(type);
                    Log.Console($"reload config {configName} finish!");
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}