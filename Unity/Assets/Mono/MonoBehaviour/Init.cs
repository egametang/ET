using System;
using System.Collections;
using System.Threading;
using Huatuo;
using UnityEditor;
using UnityEngine;
using YooAsset;

namespace ET
{
    // 1 mono模式 2 ILRuntime模式 3 mono热重载模式
    public enum CodeMode
    {
        Mono = 1,
        Reload = 3,
    }

    public class Init : MonoBehaviour
    {
        public CodeMode CodeMode = CodeMode.Mono;
        public YooAssets.EPlayMode PlayMode = YooAssets.EPlayMode.EditorSimulateMode;

        private void Awake()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { Log.Error(e.ExceptionObject.ToString()); };

            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);

            DontDestroyOnLoad(gameObject);

            ETTask.ExceptionHandler += Log.Error;

            Game.ILog = new UnityLogger();

            Options.Instance = new Options();

            CodeLoader.Instance.CodeMode = this.CodeMode;
            Options.Instance.Develop = 1;
            Options.Instance.LogLevel = 0;

            // 启动YooAsset引擎
            YooAssetProxy.StartYooAssetEngine(PlayMode, LoadCode);
        }

        private void LoadCode()
        {
            YooAssets.GetRawFileAsync("Code_Unity.Core.dll").Completed += abase =>
            {
                LoadMetadataForAOTAssembly((abase as RawFileOperation).LoadFileData());
            };

            InternalLoadCode().Coroutine();
            async ETTask InternalLoadCode()
            {
                await CodeLoader.Instance.Start();
                Log.Info("Dll加载完毕，正式进入游戏流程");
            }
        }
        
        public static unsafe void LoadMetadataForAOTAssembly(byte[] dllBytes)
        {
            fixed (byte* ptr = dllBytes)
            {
#if !UNITY
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                int err = Huatuo.HuatuoApi.LoadMetadataForAOTAssembly((IntPtr)ptr, dllBytes.Length);
                Debug.Log("LoadMetadataForAOTAssembly. ret:" + err);
#endif
            }
        }

        private void Update()
        {
            CodeLoader.Instance.Update?.Invoke();
        }

        private void LateUpdate()
        {
            CodeLoader.Instance.LateUpdate?.Invoke();
        }

        private void OnApplicationQuit()
        {
            CodeLoader.Instance.OnApplicationQuit();
            CodeLoader.Instance.Dispose();
        }
    }
}