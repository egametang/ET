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
                    
                    Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, typeof(Unit).Assembly, DllHelper.GetHotfixAssembly());
                    
                    Game.EventSystem.Add(types);
                    
                    Game.EventSystem.Load();
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}