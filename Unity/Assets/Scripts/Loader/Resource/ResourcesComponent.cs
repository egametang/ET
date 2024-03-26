using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET
{
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    public class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    public class ResourcesComponent : Singleton<ResourcesComponent>, ISingletonAwake
    {
        private ResourcePackage _package;
        
        public void Awake()
        {
            YooAssets.Initialize();
        }

        protected override void Destroy()
        {
            YooAssets.Destroy();
        }

        public async ETTask CreatePackageAsync(string packageName, bool isDefault = false)
        {
            //加载YooAsset配置好的包
            _package = YooAssets.CreatePackage(packageName);
            if (isDefault)
            {
                YooAssets.SetDefaultPackage(_package);
            }

            //读取全局配置文件，包括代码执行类型（客户端/服务端/双端）、打包类型（Develop/Release）、App类型（状态同步/帧同步）、运行模式
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            //运行模式
            EPlayMode ePlayMode = globalConfig.EPlayMode;

            //资源初始化
            switch (ePlayMode)
            {
                //编辑器下的模拟模式
                case EPlayMode.EditorSimulateMode:
                {
                    EditorSimulateModeParameters createParameters = new();
                    createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild("ScriptableBuildPipeline", packageName);
                    await _package.InitializeAsync(createParameters).Task;
                    break;
                }
                //离线运行模式
                case EPlayMode.OfflinePlayMode:
                {
                    OfflinePlayModeParameters createParameters = new();
                    await _package.InitializeAsync(createParameters).Task;
                    break;
                }
                //联网运行模式
                case EPlayMode.HostPlayMode:
                {
                    string defaultHostServer = GetHostServerURL();
                    string fallbackHostServer = GetHostServerURL();
                    HostPlayModeParameters createParameters = new();
                    createParameters.BuildinQueryServices = new GameQueryServices();
                    createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    Debug.Log(defaultHostServer);
                    await _package.InitializeAsync(createParameters).Task;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static string GetHostServerURL()
        {
#if UNITY_EDITOR
            string hostServerIP = Application.dataPath.Substring(0,Application.dataPath.Length - 7);
            string appVersion = "v1.0";
            
            if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            {
                return $"{hostServerIP}/Bundles/Android/DefaultPackage/{appVersion}";
            }
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            {
                return $"{hostServerIP}/Bundles/IPhone/DefaultPackage/{appVersion}";
            }
            else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            {
                return $"{hostServerIP}/Bundles/WebGL/DefaultPackage/{appVersion}";
            }

            return $"{hostServerIP}/Bundles/StandaloneWindows64/DefaultPackage/{appVersion}";
#else
            string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
            string appVersion = "v1.0";

            if (Application.platform == RuntimePlatform.Android)
            {
                return $"{hostServerIP}/CDN/Android/{appVersion}";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return $"{hostServerIP}/CDN/IPhone/{appVersion}";
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return $"{hostServerIP}/CDN/WebGL/{appVersion}";
            }

            return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
        }

        #region 检测并下载更新
        public ResourceDownloaderOperation Downloader{ private set;get; }
        /// <summary>
        /// 下载状态 0=未检测 1=版本检测完毕 2=下载清单检测完毕 3=下载中 4=下载成功 5=下载失败
        /// </summary>
        public int DownloadStatus { private set;get; }
        
        /// <summary>
        /// 检测并更新版本号
        /// </summary>
        public async ETTask CheckPackageVersionAndUpdate()
        {
            var operation = _package.UpdatePackageVersionAsync(false);

            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //检测成功
                string packageVersion = operation.PackageVersion;
                Debug.Log($"Updated package Version : {packageVersion}");
                this.DownloadStatus = 1;
                await CheckPackageManifest(packageVersion);
            }
            else
            {
                //检测失败
                this.DownloadStatus = 5; 
                Debug.LogError(operation.Error);
            }
        }

        /// <summary>
        /// 检测并更新资源清单
        /// </summary>
        private async ETTask CheckPackageManifest(string packageVersion)
        {
            // 更新成功后自动保存版本号，作为下次初始化的版本。
            // 也可以通过operation.SavePackageVersion()方法保存。
            bool savePackageVersion = true;
            var operation = _package.UpdatePackageManifestAsync(packageVersion, savePackageVersion);
            
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //检测成功
                this.DownloadStatus = 2;
                await DownloadPackageAsset();
            }
            else
            {
                //检测失败
                this.DownloadStatus = 5;
                Debug.LogError(operation.Error);
            }
        }
        
        /// <summary>
        /// 真正下载资源的地方
        /// </summary>
        public async ETTask DownloadPackageAsset()
        {
            if (Downloader == null)
            {
                //同时下载数量
                int downloadingMaxNum = 5;
                //重试次数
                int failedTryAgain = 3;
                //创建下载器
                Downloader = _package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            }
            
            //没有需要下载的资源
            if (Downloader.TotalDownloadCount == 0)
            {
                this.DownloadStatus = 4;
                return;
            }

            //需要下载的文件总数和总大小
            //int totalDownloadCount = Downloader.TotalDownloadCount;
            //long totalDownloadBytes = Downloader.TotalDownloadBytes;    

            //注册回调方法，不注册也可
            Downloader.OnDownloadErrorCallback = (fileName, error) => { };
            Downloader.OnDownloadProgressCallback = (totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes) => { };
            Downloader.OnDownloadOverCallback = (fileName) => { };
            Downloader.OnStartDownloadFileCallback = (fileName, sizeBytes) => { };

            //开启下载
            Downloader.BeginDownload();
            
            this.DownloadStatus = 3; 
            
            await Downloader.Task;
                   
            //检测下载结果
            if (Downloader.Status == EOperationStatus.Succeed)
            {
                //下载成功
                this.DownloadStatus = 4;
            }
            else
            {
                //下载失败
                this.DownloadStatus = 5;
            }
        }
        #endregion

        public void DestroyPackage(string packageName)
        {
            ResourcePackage package = YooAssets.GetPackage(packageName);
            package.UnloadUnusedAssets();
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            AssetHandle handle = YooAssets.LoadAssetAsync<T>(location);
            await handle.Task;
            T t = (T)handle.AssetObject;
            handle.Release();
            return t;
        }

        /// <summary>
        /// 主要用来加载dll config aotdll，因为这时候纤程还没创建，无法使用ResourcesLoaderComponent。
        /// 游戏中的资源应该使用ResourcesLoaderComponent来加载
        /// </summary>
        public async ETTask<Dictionary<string, T>> LoadAllAssetsAsync<T>(string location) where T : UnityEngine.Object
        {
            AllAssetsHandle allAssetsOperationHandle = YooAssets.LoadAllAssetsAsync<T>(location);
            await allAssetsOperationHandle.Task;
            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in allAssetsOperationHandle.AllAssetObjects)
            {
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }

            allAssetsOperationHandle.Release();
            return dictionary;
        }
    }
}