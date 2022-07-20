﻿using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;
using NLog;

namespace ET.Server
{
    public static class Entry
    {
        public static void Start()
        {
            StartAsync().Coroutine();
        }
        
        private static async ETTask StartAsync()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log.Error(e.ExceptionObject.ToString());
            };
			
            ETTask.ExceptionHandler += Log.Error;

            // 异步方法全部会回掉到主线程
            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
            
            CodeLoader.Instance.LoadHotfix();
				
            MongoHelper.Register(Game.EventSystem.GetTypes());

            string[] args = System.Environment.GetCommandLineArgs();
            
            // 命令行参数
            Options options = null;
            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(error => throw new Exception($"命令行格式错误!"))
                .WithParsed(o => { options = o; });

            Options.Instance = options;

            Game.ILog = new NLogger(Game.Options.AppType.ToString());
            LogManager.Configuration.Variables["appIdFormat"] = $"{Game.Options.Process:000000}";
				
            Log.Console($"app start: {Game.Scene.Id} options: {JsonHelper.ToJson(Game.Options)} ");
            
            await Game.EventSystem.Callback<ETTask>(CallbackType.InitShare);
            await Game.EventSystem.Callback<ETTask>(CallbackType.InitServer);
        }
    }
}