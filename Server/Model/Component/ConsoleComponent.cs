using System;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
{
    [ObjectSystem]
    public class ConsoleComponentAwakeSystem : StartSystem<ConsoleComponent>
    {
        public override void Start(ConsoleComponent self)
        {
            self.Start().NoAwait();
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

                    switch (line)
                    {
                        case "reload": 
                            try
                            {
                                Game.EventSystem.Add(DLLType.Hotfix, DllHelper.GetHotfixAssembly());
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
                        case "exit":
                            switch (this.Mode)
                            {
                                case ConsoleMode.Repl:
                                    this.RemoveComponent<ReplComponent>();
                                    break;
                            }

                            this.Mode = ConsoleMode.None;
                            break;
                        case "reset":
                            switch (this.Mode)
                            {
                                case ConsoleMode.Repl:
                                    this.RemoveComponent<ReplComponent>();
                                    this.AddComponent<ReplComponent>();
                                    break;
                            }
                            break;
                        default:
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
                                        await replComponent.Run(line, this.CancellationTokenSource.Token);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }

                                    break;
                                }
                            }

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