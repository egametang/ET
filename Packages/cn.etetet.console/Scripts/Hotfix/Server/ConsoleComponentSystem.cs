using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ET.Server
{
    [EntitySystemOf(typeof(ConsoleComponent))]
    [FriendOf(typeof(ModeContex))]
    public static partial class ConsoleComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ConsoleComponent self)
        {
            self.Start().NoContext();
        }

        
        private static async ETTask Start(this ConsoleComponent self)
        {
            self.CancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                try
                {
                    ModeContex modeContex = self.GetComponent<ModeContex>();
                    string line = await Task.Factory.StartNew(() =>
                    {
                        Console.Write($"{modeContex?.Mode ?? ""}> ");
                        return Console.In.ReadLine();
                    }, self.CancellationTokenSource.Token);
                    
                    line = line.Trim();

                    switch (line)
                    {
                        case "":
                            break;
                        case "exit":
                            self.RemoveComponent<ModeContex>();
                            break;
                        default:
                        {
                            string[] lines = line.Split(" ");
                            string mode = modeContex == null? lines[0] : modeContex.Mode;

                            IConsoleHandler iConsoleHandler = ConsoleDispatcher.Instance.Get(mode);
                            if (modeContex == null)
                            {
                                modeContex = self.AddComponent<ModeContex>();
                                modeContex.Mode = mode;
                            }
                            await iConsoleHandler.Run(self.Fiber(), modeContex, line);
                            break;
                        }
                    }


                }
                catch (Exception e)
                {
                    Log.Console(e.ToString());
                }
            }
        }
    }
}