using System;
using System.Threading;
using CommandLine;
using NLog;

namespace ET
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log.Error(e.ExceptionObject.ToString());
            };

            ETTask.ExceptionHandler += Log.Error;
            
            // 异步方法全部会回掉到主线程
            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
            try
            {		
                Game.EventSystem.Add(typeof(Game).Assembly);
				
                ProtobufHelper.Init();
                MongoRegister.Init();
				
                // 命令行参数
                Options options = null;
                Parser.Default.ParseArguments<Options>(args)
                        .WithNotParsed(error => throw new Exception($"命令行格式错误!"))
                        .WithParsed(o => { options = o; });

                Options.Instance = options;

                Log.ILog = new NLogger(Game.Options.AppType.ToString());
                LogManager.Configuration.Variables["appIdFormat"] = $"{Game.Options.Process:000000}";
				
                Log.Info($"server start........................ {Game.Scene.Id}");
				
                switch (Game.Options.AppType)
                {
                    case AppType.ExcelExporter:
                    {
                        Game.Options.Console = 1;
                        ExcelExporter.Export();
                        return 0;
                    }
                    case AppType.Proto2CS:
                    {
                        Game.Options.Console = 1;
                        Proto2CS.Export();
                        return 0;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Console(e.ToString());
            }
            return 1;
        }
    }
}