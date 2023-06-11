using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;

namespace ET.Server
{
    internal static class Init
    {
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log.Error(e.ExceptionObject.ToString());
            };
            
            try
            {
                // 命令行参数
                Parser.Default.ParseArguments<Options>(args)
                    .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                    .WithParsed(Game.Instance.AddSingleton);
                Game.Instance.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
                
                Process process = Game.Instance.Create(false);
                // 异步方法全部会回掉到主线程
                process.AddSingleton<MainThreadSynchronizationContext>();
                process.AddSingleton<TimeInfo>();
                process.AddSingleton<ObjectPool>();
                process.AddSingleton<IdGenerater>();
                
                ETTask.ExceptionHandler += Log.Error;
                
                process.AddSingleton<EventSystem>();
                Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Process).Assembly);
                EventSystem.Instance.Add(types);

                process.AddSingleton<EntitySystemSingleton>();
                
                process.AddSingleton<Root>();

                MongoHelper.Register();
				
                Log.Info($"server start........................ {Root.Instance.Scene.Id}");
				
                switch (Options.Instance.AppType)
                {
                    case AppType.ExcelExporter:
                    {
                        Options.Instance.Console = 1;
                        ExcelExporter.Export();
                        return 0;
                    }
                    case AppType.Proto2CS:
                    {
                        Options.Instance.Console = 1;
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