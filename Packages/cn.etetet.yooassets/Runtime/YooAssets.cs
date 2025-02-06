using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    public static partial class YooAssets
    {
        private static bool _isInitialize = false;
        private static GameObject _driver = null;
        private static readonly List<ResourcePackage> _packages = new List<ResourcePackage>();

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public static bool Initialized
        {
            get { return _isInitialize; }
        }

        /// <summary>
        /// 初始化资源系统
        /// </summary>
        /// <param name="logger">自定义日志处理</param>
        public static void Initialize(ILogger logger = null)
        {
            if (_isInitialize)
            {
                UnityEngine.Debug.LogWarning($"{nameof(YooAssets)} is initialized !");
                return;
            }

            if (_isInitialize == false)
            {
                YooLogger.Logger = logger;

                // 创建驱动器
                _isInitialize = true;
                _driver = new UnityEngine.GameObject($"[{nameof(YooAssets)}]");
                _driver.AddComponent<YooAssetsDriver>();
                UnityEngine.Object.DontDestroyOnLoad(_driver);
                YooLogger.Log($"{nameof(YooAssets)} initialize !");

#if DEBUG
                // 添加远程调试脚本
                _driver.AddComponent<RemoteDebuggerInRuntime>();
#endif

                OperationSystem.Initialize();
            }
        }

        /// <summary>
        /// 销毁资源系统
        /// </summary>
        public static void Destroy()
        {
            if (_isInitialize)
            {
                OperationSystem.DestroyAll();

                foreach (var package in _packages)
                {
                    package.DestroyPackage();
                }
                _packages.Clear();

                _isInitialize = false;
                if (_driver != null)
                    GameObject.Destroy(_driver);
                YooLogger.Log($"{nameof(YooAssets)} destroy all !");
            }
        }

        /// <summary>
        /// 更新资源系统
        /// </summary>
        internal static void Update()
        {
            if (_isInitialize)
            {
                OperationSystem.Update();

                for (int i = 0; i < _packages.Count; i++)
                {
                    _packages[i].UpdatePackage();
                }
            }
        }


        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static ResourcePackage CreatePackage(string packageName)
        {
            CheckException(packageName);
            if (ContainsPackage(packageName))
                throw new Exception($"Package {packageName} already existed !");

            YooLogger.Log($"Create resource package : {packageName}");
            ResourcePackage package = new ResourcePackage(packageName);
            _packages.Add(package);
            return package;
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static ResourcePackage GetPackage(string packageName)
        {
            CheckException(packageName);
            var package = GetPackageInternal(packageName);
            if (package == null)
                YooLogger.Error($"Not found resource package : {packageName}");
            return package;
        }

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static ResourcePackage TryGetPackage(string packageName)
        {
            CheckException(packageName);
            return GetPackageInternal(packageName);
        }

        /// <summary>
        /// 销毁资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static void DestroyPackage(string packageName)
        {
            CheckException(packageName);
            ResourcePackage package = GetPackageInternal(packageName);
            if (package == null)
                return;

            YooLogger.Log($"Destroy resource package : {packageName}");
            _packages.Remove(package);
            package.DestroyPackage();
        }

        /// <summary>
        /// 检测资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static bool ContainsPackage(string packageName)
        {
            CheckException(packageName);
            var package = GetPackageInternal(packageName);
            return package != null;
        }

        /// <summary>
        /// 开启一个异步操作
        /// </summary>
        /// <param name="operation">异步操作对象</param>
        public static void StartOperation(GameAsyncOperation operation)
        {
            // 注意：游戏业务逻辑的包裹填写为空
            OperationSystem.StartOperation(string.Empty, operation);
        }


        private static ResourcePackage GetPackageInternal(string packageName)
        {
            foreach (var package in _packages)
            {
                if (package.PackageName == packageName)
                    return package;
            }
            return null;
        }
        private static void CheckException(string packageName)
        {
            if (_isInitialize == false)
                throw new Exception($"{nameof(YooAssets)} not initialize !");

            if (string.IsNullOrEmpty(packageName))
                throw new Exception("Package name is null or empty !");
        }

        #region 系统参数
        /// <summary>
        /// 设置下载系统参数，下载失败后清理文件的HTTP错误码
        /// </summary>
        public static void SetDownloadSystemClearFileResponseCode(List<long> codes)
        {
            DownloadHelper.ClearFileResponseCodes = codes;
        }

        /// <summary>
        /// 设置下载系统参数，自定义下载请求
        /// </summary>
        public static void SetDownloadSystemUnityWebRequest(DownloadRequestDelegate requestDelegate)
        {
            DownloadHelper.RequestDelegate = requestDelegate;
        }

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        public static void SetOperationSystemMaxTimeSlice(long milliseconds)
        {
            if (milliseconds < 10)
            {
                milliseconds = 10;
                YooLogger.Warning($"MaxTimeSlice minimum value is 10 milliseconds.");
            }
            OperationSystem.MaxTimeSlice = milliseconds;
        }

        /// <summary>
        /// 设置缓存系统参数，禁用缓存在WebGL平台
        /// </summary>
        public static void SetCacheSystemDisableCacheOnWebGL()
        {
            CacheHelper.DisableUnityCacheOnWebGL = true;
        }
        #endregion

        #region 调试信息
        internal static DebugReport GetDebugReport()
        {
            DebugReport report = new DebugReport();
            report.FrameCount = Time.frameCount;

            foreach (var package in _packages)
            {
                var packageData = package.GetDebugPackageData();
                report.PackageDatas.Add(packageData);
            }
            return report;
        }
        #endregion
    }
}