using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using CommandLine;

namespace ET
{
    public class ToolScene: IScene
    {
        public Fiber Fiber { get; set; }

        public int SceneType
        {
            get;
            set;
        }

        public ToolScene()
        {
        }

        public ToolScene(int sceneType)
        {
            this.SceneType = sceneType;
        }
    }
    
    public struct ToolEvent
    {
    }
    
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
                    .WithParsed((o)=>World.Instance.AddSingleton(o));
                
                World.Instance.AddSingleton<Logger>().Log = new NLogger(Options.Instance.SceneName, Options.Instance.Process, 0);
                
                World.Instance.AddSingleton<CodeTypes, Assembly[]>([typeof (Init).Assembly]);
                World.Instance.AddSingleton<EventSystem>();
                World.Instance.AddSingleton<SceneTypeSingleton, Type>(typeof(SceneType));
                
                // 强制调用一下mongo，避免mongo库被裁剪
                MongoHelper.ToJson(1);
                
                ETTask.ExceptionHandler += Log.Error;

                int sceneType = SceneTypeSingleton.Instance.GetSceneType(Options.Instance.SceneName);

                ToolScene scene = new(sceneType);
                EventSystem.Instance.Publish(scene, new ToolEvent());
                
                Log.Console($"{Options.Instance.SceneName} run finish!");
            }
            catch (Exception e)
            {
                Log.Console(e.ToString());
            }
            return 1;
        }
    }
}