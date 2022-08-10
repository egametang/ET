using System;
using System.Collections.Generic;

namespace ET.Server
{
    [ConsoleHandler(ConsoleMode.ReloadDll)]
    public class ReloadDllConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContex contex, string content)
        {
            switch (content)
            {
                case ConsoleMode.ReloadDll:
                    contex.Parent.RemoveComponent<ModeContex>();
                    
                    CodeLoader.Instance.LoadHotfix();
                    
                    Game.EventSystem.Load();
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}