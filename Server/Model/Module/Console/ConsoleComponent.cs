using System;
using System.Threading;
using System.Threading.Tasks;

namespace ET
{
    public class ConsoleComponentAwakeSystem : StartSystem<ConsoleComponent>
    {
        public override void Start(ConsoleComponent self)
        {
            self.Start().Coroutine();
        }
    }

    public static class ConsoleMode
    {
        public const string None = "";
        public const string Repl = "repl";
    }
    
    public class ConsoleComponent: Entity
    {
        public CancellationTokenSource CancellationTokenSource;
        public string Mode = "";

        public async ETVoid Start()
        {
            this.CancellationTokenSource = new CancellationTokenSource();
            
            while (true)
            {
                try
                {
                    string line = await Task.Factory.StartNew(() =>
                    {
                        Console.Write($"{this.Mode}> ");
                        return Console.In.ReadLine();
                    }, this.CancellationTokenSource.Token);
                    
                    line = line.Trim();

                    if (this.Mode != "")
                    {
                        bool isExited = true;
                        switch (this.Mode)
                        {
                            case ConsoleMode.Repl:
                            {
                                ReplComponent replComponent = this.GetComponent<ReplComponent>();
                                if (replComponent == null)
                                {
                                    Console.WriteLine($"no command: {line}!");
                                    break;
                                }
                            
                                try
                                {
                                    isExited = await replComponent.Run(line, this.CancellationTokenSource.Token);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }

                                break;
                            }
                        }

                        if (isExited)
                        {
                            this.Mode = "";
                        }

                        continue;
                    }

                    switch (line)
                    {
                        case "reload": 
                            try
                            {
                                Game.EventSystem.Add(DllHelper.GetHotfixAssembly());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                            break;
                        case "repl":
                            try
                            {
                                this.Mode = ConsoleMode.Repl;
                                this.AddComponent<ReplComponent>();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                            break;
                        default:
                            Console.WriteLine($"no such command: {line}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}