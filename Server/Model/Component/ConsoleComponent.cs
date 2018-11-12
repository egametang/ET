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
    
    public class ConsoleComponent: Component
    {
        public CancellationTokenSource CancellationTokenSource;
        public string OutputPrefix = "";

        public async ETVoid Start()
        {
            this.CancellationTokenSource = new CancellationTokenSource();
            
            while (true)
            {
                try
                {
                    string line = await Task.Factory.StartNew(() =>
                    {
                        Console.Write($"{OutputPrefix}> ");
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
                                this.OutputPrefix = "repl";
                                Game.Scene.AddComponent<ReplComponent>();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                            break;
                        default:
                            ReplComponent replComponent = Game.Scene.GetComponent<ReplComponent>();
                            if (replComponent == null)
                            {
                                Console.WriteLine($"no command: {line}!");
                                break;
                            }
                            
                            try
                            {
                                if (line == "exit")
                                {
                                    this.OutputPrefix = "";
                                    Game.Scene.RemoveComponent<ReplComponent>();
                                    break;
                                }

                                switch (line)
                                {
                                    case "exit":
                                        this.OutputPrefix = "";
                                        Game.Scene.RemoveComponent<ReplComponent>();
                                        break;
                                    case "reset":
                                        Game.Scene.RemoveComponent<ReplComponent>();
                                        Game.Scene.AddComponent<ReplComponent>();
                                        break;
                                    default:
                                        await replComponent.Run(line, this.CancellationTokenSource.Token);
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
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