using System;
using System.Collections;
using System.Collections.Generic;
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

        private List<string> AOTDllList = new List<string>()
        {
            "Code_System.dll", "Code_Unity.Core.dll", "Code_Unity.ThirdParty.dll", "Code_mscorlib.dll",
            "Code_System.Core.dll"
        };

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
            List<ETTask<byte[]>> tasks = new List<ETTask<byte[]>>();

            foreach (var aotDll in AOTDllList)
            {
                Debug.Log($"添加{aotDll}");

                tasks.Add(YooAssetProxy.GetRawFileAsync(aotDll));
            }

            InternalLoadCode(tasks).Coroutine();
        }

        async ETTask InternalLoadCode(List<ETTask<byte[]>> tasks)
        {
            await ETTaskHelper.WaitAll(tasks);

            foreach (var task in tasks)
            {
                Debug.Log("准备加载AOT补充元数据");
                LoadMetadataForAOTAssembly(task.GetResult());
            }

            await CodeLoader.Instance.Start();
            Log.Info("Dll加载完毕，正式进入游戏流程");
        }

        public static unsafe void LoadMetadataForAOTAssembly(byte[] dllBytes)
        {
            fixed (byte* ptr = dllBytes)
            {
#if !UNITY_EDITOR
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