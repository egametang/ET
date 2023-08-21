# CHANGELOG

All notable changes to this package will be documented in this file.

## [1.5.1] - 2023-07-12

### Fixed

- 修复了太空战机DEMO在生成内置文件清单的时候，目录不存在引发的异常。
- 修复了在销毁Package时，如果存在正在加载的bundle，会导致后续加载该bundle报错的问题。

### Changed

- 真机上使用错误方法加载原生文件的时候给予正确的错误提示。

### Added

- 新增了HostPlayModeParameters.RemoteServices字段

  ```c#
  /// <summary>
  /// 远端资源地址查询服务类
  /// </summary>
  public IRemoteServices RemoteServices = null;
  ```

### Removed

- 移除了HostPlayModeParameters.DefaultHostServer字段
- 移除了HostPlayModeParameters.FallbackHostServer字段

## [1.5.0] - 2023-07-05

该版本重构了Persistent类，导致沙盒目录和内置目录的存储结构发生了变化。

该版本支持按照Package自定义沙盒存储目录和内置存储目录。

**注意：低版本升级用户，请使用Space Shooter目录下的StreamingAssetsHelper插件覆盖到本地工程！**

### Changed

- BuildParameters.OutputRoot重命名为BuildOutputRoot
- 变更了IQueryServices.QueryStreamingAssets(string packageName, string fileName)方法

### Added

- 新增了YooAssets.SetCacheSystemDisableCacheOnWebGL()方法

  ```c#
  /// <summary>
  /// 设置缓存系统参数，禁用缓存在WebGL平台
  /// </summary>
  public static void SetCacheSystemDisableCacheOnWebGL()
  ```

- 新增了YooAssets.SetDownloadSystemRedirectLimit()方法

  ```c#
  /// <summary>
  /// 设置下载系统参数，网络重定向次数（Unity引擎默认值32）
  /// 注意：不支持设置为负值
  /// </summary>
  public static void SetDownloadSystemRedirectLimit(int redirectLimit)
  ```

- 新增了构建流程可扩展的方法。

  ```c#
  public class AssetBundleBuilder
  {
      /// <summary>
      /// 构建资源包
      /// </summary>
      public BuildResult Run(BuildParameters buildParameters, List<IBuildTask> buildPipeline)
  }
  ```

- 新增了BuildParameters.StreamingAssetsRoot字段

  ```c#
  public class BuildParameters
  {
      /// <summary>
      /// 内置资源的根目录
      /// </summary>
      public string StreamingAssetsRoot;
  }
  ```

- 新增了InitializeParameters.BuildinRootDirectory字段

  ```c#
  /// <summary>
  /// 内置文件的根路径
  /// 注意：当参数为空的时候会使用默认的根目录。
  /// </summary>
  public string BuildinRootDirectory = string.Empty;
  ```

- 新增了InitializeParameters.SandboxRootDirectory字段

  ```c#
  /// <summary>
  /// 沙盒文件的根路径
  /// 注意：当参数为空的时候会使用默认的根目录。
  /// </summary>
  public string SandboxRootDirectory = string.Empty;
  ```

- 新增了ResourcePackage.GetPackageBuildinRootDirectory()方法

  ```c#
  /// <summary>
  /// 获取包裹的内置文件根路径
  /// </summary>
  public string GetPackageBuildinRootDirectory()
  ```

- 新增了ResourcePackage.GetPackageSandboxRootDirectory()方法

  ```c#
  /// <summary>
  /// 获取包裹的沙盒文件根路径
  /// </summary>
  public string GetPackageSandboxRootDirectory()
  ```

- 新增了ResourcePackage.ClearPackageSandbox()方法

  ```c#
  /// <summary>
  /// 清空包裹的沙盒目录
  /// </summary>
  public void ClearPackageSandbox()
  ```

### Removed

- 移除了资源包构建流程任务节点可扩展功能。
- 移除了YooAssets.SetCacheSystemSandboxPath()方法
- 移除了YooAssets.GetStreamingAssetBuildinFolderName()方法
- 移除了YooAssets.GetSandboxRoot()方法
- 移除了YooAssets.ClearSandbox()方法

## [1.4.17] - 2023-06-27

### Changed

- 优化了缓存的信息文件写入方式

- 离线模式支持内置资源解压到沙盒

- 资源包构建流程任务节点支持可扩展

  ```c#
  using YooAsset.Editor
  
  [TaskAttribute(ETaskPipeline.AllPipeline, 100, "自定义任务节点")]
  public class CustomTask : IBuildTask
  ```

- 资源收集界面增加了LocationToLower选项

- 资源收集界面增加了IncludeAssetGUID选项

- IShareAssetPackRule 重命名为 ISharedPackRule

### Added

- 新增了ResourcePackage.LoadAllAssetsAsync方法

  ```c#
  /// <summary>
  /// 异步加载资源包内所有资源对象
  /// </summary>
  /// <param name="assetInfo">资源信息</param>
  public AllAssetsOperationHandle LoadAllAssetsAsync(AssetInfo assetInfo)
  ```

- 新增了ResourcePackage.GetAssetInfoByGUID()方法

  ```c#
  /// <summary>
  /// 获取资源信息
  /// </summary>
  /// <param name="assetGUID">资源GUID</param>
  public AssetInfo GetAssetInfoByGUID(string assetGUID)
  ```

- 新增了场景加载参数suspendLoad

  ```c#
  /// <summary>
  /// 异步加载场景
  /// </summary>
  /// <param name="location">场景的定位地址</param>
  /// <param name="sceneMode">场景加载模式</param>
  /// <param name="suspendLoad">场景加载到90%自动挂起</param>
  /// <param name="priority">优先级</param>
  public SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool suspendLoad = false, int priority = 100)
  ```

- Extension Sample 增加了GameObjectAssetReference示例脚本

- 新增加了ZeroRedundancySharedPackRule类（零冗余的共享资源打包规则）

- 新增加了FullRedundancySharedPackRule类（全部冗余的共享资源打包规则）

### Removed

- 移除了InitializeParameters.LocationToLower成员字段
- 移除了LoadSceneAsync方法里的activateOnLoad形参参数
- 移除了BuildParameters.AutoAnalyzeRedundancy成员字段
- 移除了DefaultShareAssetPackRule编辑器类

## [1.4.16] - 2023-06-14

### Changed

- 增加了自动分析冗余资源的开关

  ```c#
  /// <summary>
  /// 构建参数
  /// </summary>
  public class BuildParameters
  {
      /// <summary>
      /// 自动分析冗余资源
      /// </summary>
      public bool AutoAnalyzeRedundancy = true;
  }
  ```

- 太空战机DEMO启用了新的内置资源查询机制。

## [1.4.15] - 2023-06-09

### Fixed

- 修复了安卓平台，解压内置文件到沙盒失败后不再重新尝试的问题。
- 修复了验证远端下载文件，极小概率失败的问题。
- 修复了太空战机DEMO在IOS平台流解密失败的问题。

## [1.4.14] - 2023-05-26

### Fixed

- 修复了收集器对着色器未过滤的问题。
- 修复了内置着色器Tag特殊情况下未正确传染给依赖资源包的问题。

### Changed

- Unity2021版本及以上推荐使用可编程构建管线（SBP）

## [1.4.13] - 2023-05-12

### Changed

- 可寻址地址冲突时，打印冲突地址的资源路径。
- 销毁Package的时候清空该Package的缓存记录。

### Added

- 新增方法ResoucePackage.ClearAllCacheFilesAsync()

  ```c#
  public class ResoucePackage
  {
      /// <summary>
      /// 清理包裹本地所有的缓存文件
      /// </summary>
      public ClearAllCacheFilesOperation ClearAllCacheFilesAsync();   
  }
  ```

- 新增方法YooAssets.SetCacheSystemSandboxPath()

  ```c#
  public class YooAssets
  {
      /// <summary>
      /// 设置缓存系统参数，沙盒目录的存储路径
      /// </summary>
      public static void SetCacheSystemSandboxPath(string sandboxPath);
  }
  ```

## [1.4.12] - 2023-04-22

### Changed

- 增加了对WEBGL平台加密选项的检测。

- 增加了YooAsset/Home Page菜单栏。

- 增加了鼠标右键创建配置的菜单。

- 增加了YooAssets.DestroyPackage()方法。

  ```c#
  class YooAssets
  {
      /// <summary>
      /// 销毁资源包
      /// </summary>
      /// <param name="package">资源包对象</param>
      public static void DestroyPackage(string packageName);
  }
  ```

- UpdatePackageManifestAsync方法增加了新参数autoSaveVersion

  ```c#
  class ResourcePackage
  {
      /// <summary>
      /// 向网络端请求并更新清单
      /// </summary>
      /// <param name="packageVersion">更新的包裹版本</param>
      /// <param name="autoSaveVersion">更新成功后自动保存版本号，作为下次初始化的版本。</param>
      /// <param name="timeout">超时时间（默认值：60秒）</param>
      public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion = true, int timeout = 60)   
  }
  ```

- BuildParameters类增加了新字段。

  可以自定义共享资源文件的打包规则。

  ```c#
  class BuildParameters
  {
      /// <summary>
      /// 共享资源的打包规则
      /// </summary>
      public IShareAssetPackRule ShareAssetPackRule = null;
  }
  ```

## [1.4.11] - 2023-04-14

### Fixed

- (#97)修复了着色器变种收集配置无法保存的问题。
- (#83)修复了资源收集界面Package列表没有实时刷新的问题。
- (#48)优化了场景卸载机制，在切换场景的时候不在主动卸载资源。

### Changed

- 增加了扩展属性

  ```c#
  [assembly: InternalsVisibleTo("YooAsset.EditorExtension")]
  [assembly: InternalsVisibleTo("YooAsset.RuntimeExtension")]
  ```

## [1.4.10] - 2023-04-08

### Fixed

- 修复了资源文件路径无效导致异常的问题。
- 修复了原生文件不支持ini格式文件的问题。
- 修复了通过代码途径导入XML配置的报错问题。

## [1.4.9] - 2023-03-29

### Fixed

- 修复了资源配置界面的GroupActiveRule保存无效的问题。

### Changed

- 优化了资源配置导入逻辑，增加了对XML配置文件的合法性检测。

- 优化了UniTask的说明文档。

- 调整构建的输出目录结构。

- 调试窗口增加分屏功能。（Unity2020.3+起效）

- 报告窗口增加分屏功能。（Unity2020.3+起效）

- 编辑器模拟模式支持了虚拟资源包。

- 扩展了Instantiate方法。

  ```c#
  public sealed class AssetOperationHandle
  {
      public GameObject InstantiateSync();
      public GameObject InstantiateSync(Transform parent);
      public GameObject InstantiateSync(Transform parent, bool worldPositionStays);
      public GameObject InstantiateSync(Vector3 position, Quaternion rotation);
      public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent);
  }
  ```

### Added

- 优化了报告文件内容，增加了资源包内嵌的资源列表。

- 可寻址规则增加了AddressByFilePath类。

- 新增了新方法。

  ```c#
  /// <summary>
  /// 向远端请求并更新清单
  /// </summary>
  public class UpdatePackageManifestOperation : AsyncOperationBase
  {
  	/// <summary>
  	/// 保存当前清单的版本，用于下次启动时自动加载的版本。
  	/// </summary>
  	public void SavePackageVersion();
  }
  ```

- 新增了初始化参数。

  ```c#
  /// <summary>
  /// 下载失败尝试次数
  /// 注意：默认值为MaxValue
  /// </summary>
  public int DownloadFailedTryAgain = int.MaxValue;
  ```

- 新增了初始化参数。

  ```c#
  /// <summary>
  /// 资源加载每帧处理的最大时间片段
  /// 注意：默认值为MaxValue
  /// </summary>
  public long LoadingMaxTimeSlice = long.MaxValue;
  ```

### Removed

- 移除了代码里的Patch敏感字。

  ```c#
  //PatchManifest.cs重命名为PackageManifest.cs
  //AssetsPackage.cs重命名为ResourcePackage.cs
  //YooAssets.CreateAssetsPackage()重命名为YooAssets.CreatePackage()
  //YooAssets.GetAssetsPackage()重命名为YooAssets.GetPackage()
  //YooAssets.TryGetAssetsPackage()重命名为YooAssets.TryGetPackage()
  //YooAssets.HasAssetsPackage()重命名为YooAssets.HasPackage()
  ```

- 移除了初始化参数：AssetLoadingMaxNumber

## [1.4.8] - 2023-03-10

### Fixed

- 修复了同步加载原生文件，程序卡死的问题。
- 修复了可编程构建管线，当项目里没有着色器，如果有引用内置着色器会导致打包失败的问题。
- 修复了在Unity2021.3版本下着色器收集界面错乱的问题。

### Changed

- 优化了打包逻辑，提高构建速度。

- 支持自定义日志处理，方便收集线上问题。

  ```c#
  public class YooAssets
  {
      /// <summary>
      /// 初始化资源系统
      /// </summary>
      /// <param name="logger">自定义日志处理</param>
      public static void Initialize(ILogger logger = null)
  }
  ```

## [1.4.7] - 2023-03-03

### Fixed

- 修复了在运行时资源引用链无效的问题。
- 修复了在构建过程中发生异常后进度条未消失的问题。
- 修复了使用SBP构建管线，如果有原生文件会导致打包失败的问题。

### Changed

- 支持自定义下载请求

  ```c#
  /// <summary>
  /// 设置下载系统参数，自定义下载请求
  /// </summary>
  public static void SetDownloadSystemUnityWebRequest(DownloadRequestDelegate requestDelegate)
  ```

- 优化了打包时资源包引用关系计算的逻辑。

- 优化了缓存系统初始化逻辑，支持分帧获取所有缓存文件。

- 优化了缓存系统的存储目录结构，提高了文件夹查询速度。

- 优化了在资源收集界面，点击查看Collector主资源列表卡顿问题。

- 优化了资源对象加载耗时统计的逻辑，现在更加准确了。

- 优化了资源加载器查询逻辑。

- 优化了资源下载系统，下载文件的验证支持了多线程。

- 着色器变种收集界面增加单次照射数量的控制。

## [1.4.6-preview] - 2023-02-22

### Changed

- EVerifyLevel新增Middle级别。

  ```c#
  public enum EVerifyLevel
  {
      /// <summary>
      /// 验证文件存在
      /// </summary>
      Low,
      
      /// <summary>
      /// 验证文件大小
      /// </summary>
      Middle,
  
      /// <summary>
      /// 验证文件大小和CRC
      /// </summary>
      High,
  }
  ```

- 补丁清单的资源包列表新增引用链。

  （解决复杂依赖关系下，错误卸载资源包的问题）

- 缓存系统支持后缀格式存储。

  （解决原生文件没有后缀格式的问题）

- 收集界面增加用户自定义数据栏。

## [1.4.5-preview] - 2023-02-17

### Fixed

- (#67)修复了报告查看界面在Unity2021.3上的兼容性问题。
- (#66)修复了在Unity2021.3上编辑器模拟模式运行报错的问题。

### Changed

- 接口变更：IPackRule

  ````c#
  /// <summary>
  /// 资源打包规则接口
  /// </summary>
  public interface IPackRule
  {
      /// <summary>
      /// 获取打包规则结果
      /// </summary>
      PackRuleResult GetPackRuleResult(PackRuleData data);
  
      /// <summary>
      /// 是否为原生文件打包规则
      /// </summary>
      bool IsRawFilePackRule();
  }
  ````

## [1.4.4-preview] - 2023-02-14

### Fixed

- (#65)修复了AssetBundle构建宏逻辑错误。
- 修复了AssetBundle加载宏逻辑错误。

## [1.4.3-preview] - 2023-02-10

全新的缓存系统！

### Fixed

- 修复了WebGL平台本地文件验证报错。
- 修复了WEBGL平台加载原生文件失败的问题。
- 修复了通过Handle句柄查询资源包下载进度为零的问题。

### Changed

- 着色器变种收集增加分批次处理功能。
- Unity2021版本开始不再支持内置构建管线。

### Removed

- 太空战机DEMO移除了BetterStreamingAssets插件。

## [1.4.2-preview] - 2023-01-03

### Fixed

- 修复了清单解析异步操作的进度条变化错误。
- 修复了更新资源清单错误计算超时时间的问题。

## [1.4.1-preview] - 2022-12-26

### Fixed

- 修复了开启UniqueBundleName选项后，SBP构建报错的问题。

### Added

- 新增了AssetsPackage.PreDownloadPackageAsync()方法

  ````c#
  /// <summary>
  /// 预下载指定版本的包裹资源
  /// </summary>
  /// <param name="packageVersion">下载的包裹版本</param>
  /// <param name="timeout">超时时间（默认值：60秒）</param>
  public PreDownloadPackageOperation PreDownloadPackageAsync(string packageVersion, int timeout = 60)
  ````

- 新增了OperationHandleBase.GetDownloadReport()方法

  ````c#
  /// <summary>
  /// 获取下载报告
  /// </summary>
  public DownloadReport GetDownloadReport();
  ````

### Changed

- 优化了资源清单更新流程，支持缓存下载的清单。
- 优化了清单文件的解析流程，支持分帧解析避免卡顿。
- 优化了缓存文件的验证流程，支持分帧处理。
- 初始化的时候支持覆盖安装检测，然后清理所有的缓存清单文件。
- ClearPackageUnusedCacheFilesAsync重名为ClearUnusedCacheFilesAsync

## [1.4.0-preview] - 2022-12-04

### Fixed

- (#46)修复了资源包初始化失败之后，再次初始化发生异常的问题。
- 修复了在初始化失败的之后，销毁YooAssets会报异常的问题。

### Changed

- 优化了资源收集界面，可以选择显示中文别名。
- **优化了补丁清单序列化方式，由文本数据修改为二进制数据。**
- 资源操作句柄增加using支持。

## [1.3.7] - 2022-11-26

全新的太空战机Demo !

### Fixed

- (#45)修复了package列表更新触发的异常。

### Added

- 新增了YooAssets.Destroy()资源系统销毁方法。

  ```C#
  /// <summary>
  /// 销毁资源系统
  /// </summary>
  public static void Destroy();
  ```

### Changed

- 优化了资源收集规则，原生文件打包名称现在已经包含文件后缀名。
- 优化了资源收集规则，非原生文件收集器自动移除Unity无法识别的文件。
- 优化了调试信息窗口，列表元素的加载状态显示为文本。

## [1.3.5] - 2022-11-19

### Fixed

- 修复了同步接口加载加密文件失败的问题。

### Added

- 新增了方法AssetsPackage.ClearPackageUnusedCacheFilesAsync()

  ```c#
  /// <summary>
  /// 清理本地包裹未使用的缓存文件
  /// </summary>
  public ClearPackageUnusedCacheFilesOperation ClearPackageUnusedCacheFilesAsync()
  ```

- 新增了方法AssetsPackage.LoadRawFileAsync()

  ```c#
  /// <summary>
  /// 异步加载原生文件
  /// </summary>
  /// <param name="location">资源的定位地址</param>
  public RawFileOperationHandle LoadRawFileAsync(string location)
  ```

- 新增了方法AssetsPackage.LoadRawFileSync()

  ```c#
  /// <summary>
  /// 同步加载原生文件
  /// </summary>
  /// <param name="location">资源的定位地址</param>
  public RawFileOperationHandle LoadRawFileSync(string location)
  ```

### Changed

- 重命名AssetsPackage.UpdateStaticVersionAsync()为AssetsPackage.UpdatePackageVersionAsync();
- 重命名AssetsPackage.UpdateManifestAsync()为AssetsPackage.UpdatePackageManifestAsync();

### Removed

- 移除了方法YooAssets.ClearUnusedCacheFiles()
- 移除了方法AssetsPackage.GetRawFileAsync()

## [1.3.4] - 2022-11-04

### Fixed

- (#29)修复了EditorHelper中根据guid找uxml有时候会出错的问题。
- (#37)修复了在修改GroupName和GroupDesc时，左侧Group栏显示没刷新的问题。
- (#38)修复了工程里没有shader的话，SBP构建会报异常的问题。

### Added

- 新增了AssetsPackage.CheckPackageContentsAsync()方法

  ```c#
  /// <summary>
  /// 检查本地包裹内容的完整性
  /// </summary>
  public CheckPackageContentsOperation CheckPackageContentsAsync()
  ```

### Changed

- 优化了HostPlayMode的初始化逻辑，优先读取沙盒内的清单，如果不存在则读取内置清单。

- 重写了文件的加密和解密逻辑。

  ```c#
  public interface IDecryptionServices
  {
      /// <summary>
      /// 文件偏移解密方法
      /// </summary>
      ulong LoadFromFileOffset(DecryptFileInfo fileInfo);
  
      /// <summary>
      /// 文件内存解密方法
      /// </summary>
      byte[] LoadFromMemory(DecryptFileInfo fileInfo);
  
      /// <summary>
      /// 文件流解密方法
      /// </summary>
      System.IO.FileStream LoadFromStream(DecryptFileInfo fileInfo);
  
      /// <summary>
      /// 文件流解密的托管缓存大小
      /// </summary>
      uint GetManagedReadBufferSize();
  }
  ```

- AssetBundleBuilder界面增加了构建版本选项。

### Removed

- 移除了AssetsPackage.WeaklyUpdateManifestAsync()方法。

## [1.3.3] - 2022-10-27

### Fixed

- 修复了资源回收方法无效的问题。

### Added

- 新增了PackageVersion构建参数。

  ````c#
  public class BuildParameters
  {
      /// <summary>
      /// 构建的包裹版本
      /// </summary>
      public string PackageVersion;  
  }
  ````

### Changed

- AssetBundleDebugger窗口增加了包裹名称显示列。
- AssetBundleDebugger窗口增加资源对象的加载耗时统计和显示。
- AssetBundleDebugger窗口增加帧调试数据导出功能。
- AssetBundleBuilder构建流程增加输出目录文件路径过长的检测。
- 下载器返回的错误提示增加HTTP Response Code。
- UpdateStaticVersionOperation.PackageCRC重名为UpdateStaticVersionOperation.PackageVersion。
- AssetPackage.GetHumanReadableVersion()重名为AssetPackage.GetPackageVersion()

## [1.3.2] - 2022-10-22

### Fixed

- 修复了AssetBundleCollector界面点击修复按钮界面没有刷新的问题。

### Added

- 新增了自定义证书认证方法。

  ````c#
  public static class YooAssets
  {
      /// <summary>
      /// 设置下载系统参数，自定义的证书认证实例
      /// </summary>
      public static void SetDownloadSystemCertificateHandler(UnityEngine.Networking.CertificateHandler instance)
  }
  ````

- 新增了下载失败后清理文件的方法。

  ````c#
  public static class YooAssets
  {
      /// <summary>
      /// 设置下载系统参数，下载失败后清理文件的HTTP错误码
      /// </summary>
      public static void SetDownloadSystemClearFileResponseCode(List<long> codes)
  }
  ````

- 新增了检查资源定位地址是否有效的方法。

  ```c#
  public class AssetsPackage
  {
      /// <summary>
      /// 检查资源定位地址是否有效
      /// </summary>
      /// <param name="location">资源的定位地址</param>
      public bool CheckLocationValid(string location)
  }
  ```

### Removed

- 移除了ILocationServices接口类和初始化字段。
- 移除了AssetPackage.GetAssetPath(string location)方法。
- 移除了BuildParameters.EnableAddressable字段。

### Changed

- AssetBundleCollector配置增加了UniqueBundleName设置，用于解决不同包裹之间Bundle名称冲突的问题。

## [1.3.1] - 2022-10-18

### Fixed

- 修复了原生文件每次获取都重复拷贝的问题。
- 修复了断点续传下载字节数统计不准确的问题。

### Added

- 所有下载相关方法增加超时判断参数。

- 新增首包资源文件拷贝选项。

  ```c#
  public class BuildParameters
  {
      /// <summary>
      /// 拷贝内置资源选项
      /// </summary>
      public ECopyBuildinFileOption CopyBuildinFileOption = ECopyBuildinFileOption.None;
  
      /// <summary>
      /// 拷贝内置资源的标签
      /// </summary>
      public string CopyBuildinFileTags = string.Empty;  
  }
  ```

- 新增资源包初始化查询字段。

  ```c#
  public class AssetsPackage
  {
      /// <summary>
      /// 初始化状态
      /// </summary>
      public EOperationStatus InitializeStatus
  }
  ```

- 增加获取人类可读的版本信息。

  ````c#
  public class AssetsPackage
  {
      /// <summary>
      /// 获取人类可读的版本信息
      /// </summary>
      public string GetHumanReadableVersion()
  }
  ````

- 新增资源缓存清理方法。

  ```c#
  public static class YooAssets
  {
      /// <summary>
      /// 清理未使用的缓存文件
      /// </summary>
      public static ClearUnusedCacheFilesOperation ClearUnusedCacheFiles()  
  }
  ```

- 异步操作类新增繁忙查询方法。

  ````c#
  public abstract class GameAsyncOperation
  {
      /// <summary>
      /// 异步操作系统是否繁忙
      /// </summary>
      protected bool IsBusy() 
  }
  ````

### Removed

- 移除了AssetsPackage.IsInitialized()方法。
- 移除了YooAssets.ClearAllCacheFiles()方法。

### Changed

- YooAssetsPackage类重名为AssetsPackage

## [1.3.0-preview] - 2022-10-08

该预览版本提供了分布式构建的功能，用于解决分工程或分内容构建的问题。

### Added

- 新增方法设置异步系统的每帧允许运行的最大时间切片。

  ```c#
  /// <summary>
  /// 设置异步系统的每帧允许运行的最大时间切片（单位：毫秒）
  /// </summary>
  public static void SetOperationSystemMaxTimeSlice(long milliseconds)
  ```

- 新增方法设置缓存系统的已经缓存文件的校验等级。

  ```c#
  /// <summary>
  /// 设置缓存系统的已经缓存文件的校验等级
  /// </summary>
  public static void SetCacheSystemCachedFileVerifyLevel(EVerifyLevel verifyLevel)
  ```

- 新增方法设置下载系统的断点续传功能的文件大小。

  ````C#
  /// <summary>
  /// 启用下载系统的断点续传功能的文件大小
  /// </summary>
  public static void SetDownloadSystemBreakpointResumeFileSize(int fileBytes)
  ````

### Removed

- 移除了资源版本号相关概念的代码。
- 移除了TaskCopyBuildinFiles节点在构建流程里。
- 移除了YooAssets.ClearUnusedCacheFiles()方法。
- 移除了初始化参数 InitializeParameters.ClearCacheOnDirty
- 移除了初始化参数 InitializeParameters.OperationSystemMaxTimeSlice
- 移除了初始化参数 InitializeParameters.BreakpointResumeFileSize
- 移除了初始化参数 InitializeParameters.VerifyLevel

## [1.2.4] - 2022-09-22

### Fixed

- 修复了加密文件下载验证失败的问题。
- 修复了可编程构建管线下模拟构建模式报错的问题。

### Changed

- 可编程构建管线强制使用增量构建模式。
- 移除了对Gizmos资源的打包限制。
- AssetBundleCollector窗口增加配置表修复功能。

## [1.2.3] - 2022-09-09

### Fixed

- 修复了资源收集器无法识别.bank音频文件格式。

### Changed

- **HostPlayMode正式支持WebGL平台。**
- AssetBundleCollector里的着色器收集选项已经移除，现在必定收集。
- AssetBundleCollector修改了默认的打包规则类。
- AssetBundleBuilder现在构建结果增加补丁包目录。
- 更新了UniTask的Sample。
- 优化了缓存系统的代码结构。
- 使用了新的断点续传下载器。

### Added

- 增加清理缓存资源的异步操作类。

````c#
/// <summary>
/// 清空未被使用的缓存文件
/// </summary>
public static ClearUnusedCacheFilesOperation ClearUnusedCacheFiles();
````

## [1.2.2] - 2022-07-31

### Fixed

- 修复了加载多个相同的子场景而无法全部卸载的问题。

### Changed

- ShaderVariantCollecor支持在CI上调用运行。

- 资源补丁清单增加文件版本校验功能。

- AssetBundleBuilder现在构建结果可以查询构建失败信息。

- AssetBundleBuilder现在资源包文件名称样式提供选择功能。

  ````c#
  class BuildParameters
  {
      /// <summary>
      /// 补丁文件名称的样式
      /// </summary>
      public EOutputNameStyle OutputNameStyle;
  }
  ````

### Added

- 增加获取资源信息新方法。

  ````c#
  /// <summary>
  /// 获取资源信息
  /// </summary>
  /// <param name="location">资源的定位地址</param>
  public static AssetInfo GetAssetInfo(string location);
  ````

## [1.2.1] - 2022-07-23

### Fixed

- (#25)修复了资源文件不存在返回的handle无法完成的问题。
- (#26)修复多个场景打进一个AB包时，卸载子场景时抛出异常。

### Changed

- 构建报告里增加主资源总数的统计。
- 资源构建系统里修改了内置构建管线的构建结果验证逻辑，移除了对中文路径的检测。
- 资源构建系统里移除了对增量更新初次无法构建的限制。
- 优化了缓存验证逻辑，不期望删除断点续传的资源文件。
- 资源构建系统里SBP构建参数增加了缓存服务器的地址和端口。

## [1.2.0] - 2022-07-18

### Fixed

- 修复了ShaderVariantCollection刷新不及时问题。

### Changed

- 资源收集忽略了Gizmos资源文件。
- 解密服务接口增加解密文件信息参数。
- 资源收集窗体增加配置保存按钮。
- 资源构建窗体增加配置保存按钮。

### Added

- 资源构建模块增加了可编程构建管线(SBP)的支持，开发者可以在内置构建管线和可编程构建管线之间自由选择，零修改成本。

## [1.1.1] - 2022-07-07

### Fixed

- 修复了AssetBundleDebugger窗口，View下拉页签切换无效的问题。
- 修复了在Unity2020.3版本下UniTask在真机上的一个IL2CPP相关的错误。

### Changed

- 优化了AssetBundleDebugger窗口，增加了帧数显示以及回放功能。
- 优化了AssetBundleBuilder的代码结构。
- 增强了YooAssets.GetRawFileAsync()方法的容错。

### Added

- 新增了OperationHandleBase.GetAssetInfo()方法。

  ````c#
  /// <summary>
  /// 获取资源信息
  /// </summary>
  public AssetInfo GetAssetInfo();
  ````

- 新增了AssetOperationHandle.GetAssetObjet<TAsset>()方法。

  ````c#
  /// <summary>
  /// 获取资源对象
  /// </summary>
  /// <typeparam name="TAsset">资源类型</typeparam>
  public TAsset GetAssetObjet<TAsset>()；
  ````

- 新增了弱联网情况下加载补丁清单方法。

  ````c#
  /// <summary>
  /// 弱联网情况下加载补丁清单
  /// 注意：当指定版本内容验证失败后会返回失败。
  /// </summary>
  /// <param name="resourceVersion">指定的资源版本</param>
  public static UpdateManifestOperation WeaklyUpdateManifestAsync(int resourceVersion)；
  ````

### Removed

- 离线运行模式（OfflinePlayMode）下移除了资内置资源解压相关逻辑。
- 移除了初始化参数：AutoReleaseGameObjectHandle及相关代码逻辑。

## [1.1.0] - 2022-06-23

### Fixed

- 修复了AssetBundleCollector窗口，在切换EnableAddressable时未及时刷新界面的问题。
- 修复了AssetBundleCollector窗口，资源过滤器CollectSprite无效的问题。
- 修复了AssetBundleCollector窗口，无法正常预览StaticAssetCollector的资源列表的问题。
- 修复了在离线模式下原生文件每次都从包内加载的问题。

### Changed

- 变更了共享资源打包机制。
- AssetBundleCollector窗口增加了分组禁用功能。
- AssetBundleDebugger窗口增加了真机远程调试功能。
- AssetBundleBuilder窗口在构建成功后自动显示构建文件夹。
- DownloaderOperation.OnDownloadFileFailedCallback委托变更为OnDownloadErrorCallback委托。

### Added

- 新增UpdateManifestOperation.FoundNewManifest字段。
- 新增DownloaderOperation.OnStartDownloadFileCallback委托。
- 新增AssetInfo.Address字段。
- 新增YooAssets.IsInitialized字段。
- 新增YooAssets初始化参数。

  ````c#
  /// <summary>
  /// 下载文件校验等级
  /// </summary>
  public EVerifyLevel VerifyLevel = EVerifyLevel.High;
  ````

- 新增YooAssets获取资源完成路径的方法。

  ````c#
  /// <summary>
  /// 获取资源路径
  /// </summary>
  /// <param name="location">资源的定位地址</param>
  /// <returns>如果location地址无效，则返回空字符串</returns>
  public static string GetAssetPath(string location);
  ````

- 新增YooAssets初始化参数。

  ```c#
  /// <summary>
  /// 自动释放游戏对象所属资源句柄
  /// 说明：通过资源句柄实例化的游戏对象在销毁之后，会自动释放所属资源句柄。
  /// </summary>
  public bool AutoReleaseGameObjectHandle = false;
  ```

## [1.0.10] - 2022-05-22

### Fixed

- 修复了资源收集配置存在多个的时候，导致后续无法打开窗口的问题。
- 修复了在编辑器模拟模式下加载精灵图片失败的问题。
- 修复了在Unity2019版本无法识别配置文件的问题。

### Changed

- 资源构建增加内置资源文件（首包资源文件）拷贝的选项。
- 补丁下载器增加暂停方法和恢复方法。
- 在资源收集界面，对Collector的增加和删除支持撤销和恢复操作。

## [1.0.9] - 2022-05-14

### Fixed

- 修复了YooAssets.GetAssetInfos(string Tag)方法返回了无关的资源信息的问题。

### Changed

- 编辑器下的模拟运行模式，不再依赖配置里的构建版本。
- 更新资源清单结构，资源对象类增加分类标签。
- 优化了资源工具相关配置文件的加载方式和途径，这些配置文件可以放置在任何目录下。
- 优化了Location无效后的错误报告方式。
- 优化了资源包的构建参数，现在始终开启DisableLoadAssetByFileName，帮助减小运行时的内存。
- YooAssets.ProcessOperation()重命名为YooAssets.StartOperation()

### Added

- 新增YooAssets.IsNeedDownloadFromRemote()方法。

  ````c#
  public static bool IsNeedDownloadFromRemote(string location);
  ````

- 新增获取所有子资源对象的方法。

  ````c#
  class SubAssetsOperationHandle
  {
      public TObject[] GetSubAssetObjects<TObject>();
  }
  ````

### Removed

- YooAssets.GetBundleInfo()方法已经移除。

## [1.0.8] - 2022-05-08

### Fixed

- 修复了资源收集器导出配置文件时没有导出公共设置。
- 修复了不兼容Unity2018版本的错误。

### Changed

- AssetBundleGrouper窗口变更为AssetBundleCollector窗口。
- **优化了编辑器下模拟运行的初始化速度**。
- **优化了资源收集窗口打开时卡顿的问题**。
- 资源收集XML配表支持版本兼容。
- 资源报告查看窗口支持预览AssetBundle文件内容的功能。
- 完善了对UniTask的支持。
- YooAssets所有接口支持初始化容错检测。

### Added

- 异步操作类增加进度查询字段。

  ```c#
  class AsyncOperationBase
  {
      /// <summary>
      /// 处理进度
      /// </summary>
      public float Progress { get; protected set; } 
  }
  ```

- 增加开启异步操作的方法。

  ```c#
  /// <summary>
  /// 开启一个异步操作
  /// </summary>
  /// <param name="operation">异步操作对象</param>
  public static void ProcessOperaiton(GameAsyncOperation operation)
  ```

- 新增编辑器下模拟模式的初始化参数。

  ````c#
  /// <summary>
  /// 用于模拟运行的资源清单路径
  /// 注意：如果路径为空，会自动重新构建补丁清单。
  /// </summary>
  public string SimulatePatchManifestPath;
  ````

- 新增通用的初始化参数。

  ```c#
  /// <summary>
  /// 资源定位地址大小写不敏感
  /// </summary>
  public bool LocationToLower = false;
  ```

## [1.0.7] - 2022-05-04

### Fixed

- 修复了异步操作系统的Task再次等待无效的问题。

### Changed

- YooAssets.LoadRawFileAsync()方法重新命名为YooAssets.GetRawFileAsync()
- YooAssetSetting文件夹支持了全路径搜索定位。
- 优化了打包的核心逻辑，对依赖资源进行自动划分，以及支持设置依赖资源收集器。
- 初始化的时候，删除验证失败的资源文件。
- 构建报告浏览窗口支持排序功能。
- 着色器变种收集工具支持了配置缓存。

### Added

- 支持可寻址资源定位系统，包括编辑器和运行时环境。
- 增加快速构建模式，用于EditorPlayMode完美模拟线上环境。
- 增加了Window Dock功能，已打开的界面会自动停靠在一个窗体下。
- 增加一个新的打包规则：PackTopDirectory。
- 增加获取资源信息的方法。
  ```c#
  public static AssetInfo[] GetAssetInfos(string tag)
  ```
- 增加补丁下载器下载全部资源的方法。
  ```c#
  public static PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain)
  ```
- 增加指定资源版本的资源更新下载方法。
  ```c#
  public static UpdatePackageOperation UpdatePackageAsync(int resourceVersion, int timeout = 60)
  ```

### Removed

- 移除了自动释放资源的初始化参数。

## [1.0.6] - 2022-04-26

### Fixed

- 修复工具界面显示异常在Unity2021版本下。

### Changed

- 操作句柄支持错误信息查询。
- 支持UniTask异步操作库。
- 优化类型搜索方式，改为全域搜索类型。
- AssetBundleGrouper窗口添加和移除Grouper支持操作回退。

## [1.0.5] - 2022-04-22

### Fixed

- 修复了非主动收集的着色器没有打进统一的着色器资源包的问题。
- 修复了单个收集的资源对象没有设置依赖资源列表的问题。
- 修复Task异步加载一直等待的问题。

### Changed

- 资源打包的过滤文件列表增加cginc格式。
- 增加编辑器扩展的支持，第三方实现YooAsset插件。
- 优化原生文件加载逻辑，支持离线运行模式和编辑器运行模式。
- 优化场景卸载逻辑，在加载新的主场景的时候自动卸载已经加载的所有场景。
- 支持演练构建模式，在不生成资源包的情况下快速构建查看结果。
- 新增调试信息，出生场景和出生时间。

## [1.0.4] - 2022-04-18

### Fixed

- 修复资源清单附加版本之后引发的一个流程错误。
- 修复原生文件拷贝目录不存导致的加载失败。

### Changed

- 在编辑器下检测资源路径是否合法并警告。
- 完善原生文件异步加载接口。

## [1.0.3] - 2022-04-14

### Fixed

- 修复了AssetBundleDebugger窗口的BundleView视口下，Using列表显示不完整的问题。
- 修复了AssetBundleDebugger窗口的BundleView视口下，Bundle列表内元素重复的问题。
- 修复了特殊情况下依赖的资源包列表里包含主资源包的问题。

### Changed

- 实例化GameObject的时候，如果没有传递坐标和角度则使用默认值。
- 优化了资源分组配置保存策略，修改为窗口关闭时保存。
- 简化了资源版本概念，降低学习成本，统一了CDN上的目录结构。
- 资源定位接口扩展，方便开发可寻址资产定位功能。

### Added

- 离线运行模式支持WEBGL平台。
- 保留构建窗口界面的配置数据。

## [1.0.2] - 2022-04-07

### Fixed

- 修复在资源加载完成回调内释放自身资源句柄时的异常报错。
- 修复了资源分组在特殊情况下打包报错的问题。

### Changed

- StreamingAssets目录下增加了用于存放打包资源的总文件夹。

## [1.0.1] - 2022-04-07

### Fixed

- 修复Assets目录下存在多个YooAsset同名文件夹时，工具窗口无法显示的问题。
- 修复通过Packages导入YooAsset，工具窗口无法显示的问题。

## [1.0.0] - 2022-04-05
*Compatible with Unity 2019.4*

