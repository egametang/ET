﻿using System;
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
            
            // 异步方法全部会回掉到主线程
            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
            try
            {
                // 异步方法全部会回掉到主线程
                SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
				
                // 命令行参数
                Options options = null;
                Parser.Default.ParseArguments<Options>(args)
                        .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                        .WithParsed(o => { options = o; });
				
                Game.AddSingleton(options);
                Game.AddSingleton<TimeInfo>();
                Game.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
                Game.AddSingleton<ObjectPool>();
                Game.AddSingleton<IdGenerater>();
                Game.AddSingleton<EventSystem>();
                Game.AddSingleton<Root>();
                
                ETTask.ExceptionHandler += Log.Error;

                Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly);
                EventSystem.Instance.Add(types);

                MongoRegister.Init();
				
                Log.Info($"server start........................ {Game.Scene.Id}");
				
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