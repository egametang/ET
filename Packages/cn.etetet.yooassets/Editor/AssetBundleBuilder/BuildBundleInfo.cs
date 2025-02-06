using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class BuildBundleInfo
    {
        #region 补丁文件的关键信息
        /// <summary>
        /// Unity引擎生成的哈希值（构建内容的哈希值）
        /// </summary>
        public string PackageUnityHash { set; get; }

        /// <summary>
        /// Unity引擎生成的CRC
        /// </summary>
        public uint PackageUnityCRC { set; get; }

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string PackageFileHash { set; get; }

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string PackageFileCRC { set; get; }

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public long PackageFileSize { set; get; }

        /// <summary>
        /// 构建输出的文件路径
        /// </summary>
        public string BuildOutputFilePath { set; get; }

        /// <summary>
        /// 补丁包的源文件路径
        /// </summary>
        public string PackageSourceFilePath { set; get; }

        /// <summary>
        /// 补丁包的目标文件路径
        /// </summary>
        public string PackageDestFilePath { set; get; }

        /// <summary>
        /// 加密生成文件的路径
        /// 注意：如果未加密该路径为空
        /// </summary>
        public string EncryptedFilePath { set; get; }
        #endregion


        /// <summary>
        /// 参与构建的资源列表
        /// 注意：不包含零依赖资源和冗余资源
        /// </summary>
        public readonly List<BuildAssetInfo> MainAssets = new List<BuildAssetInfo>();

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName { private set; get; }

        /// <summary>
        /// 加密文件
        /// </summary>
        public bool Encrypted { set; get; }


        public BuildBundleInfo(string bundleName)
        {
            BundleName = bundleName;
        }

        /// <summary>
        /// 添加一个打包资源
        /// </summary>
        public void PackAsset(BuildAssetInfo buildAsset)
        {
            if (IsContainsAsset(buildAsset.AssetInfo.AssetPath))
                throw new System.Exception($"Should never get here ! Asset is existed : {buildAsset.AssetInfo.AssetPath}");

            MainAssets.Add(buildAsset);
        }

        /// <summary>
        /// 是否包含指定资源
        /// </summary>
        public bool IsContainsAsset(string assetPath)
        {
            foreach (var buildAsset in MainAssets)
            {
                if (buildAsset.AssetInfo.AssetPath == assetPath)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取构建的资源路径列表
        /// </summary>
        public string[] GetAllMainAssetPaths()
        {
            return MainAssets.Select(t => t.AssetInfo.AssetPath).ToArray();
        }

        /// <summary>
        /// 获取该资源包内的所有资源（包括零依赖资源和冗余资源）
        /// </summary>
        public List<string> GetAllBuiltinAssetPaths()
        {
            var packAssets = GetAllMainAssetPaths();
            List<string> result = new List<string>(packAssets);
            foreach (var buildAsset in MainAssets)
            {
                if (buildAsset.AllDependAssetInfos == null)
                    continue;
                foreach (var dependAssetInfo in buildAsset.AllDependAssetInfos)
                {
                    // 注意：依赖资源里只添加零依赖资源和冗余资源
                    if (dependAssetInfo.HasBundleName() == false)
                    {
                        if (result.Contains(dependAssetInfo.AssetInfo.AssetPath) == false)
                            result.Add(dependAssetInfo.AssetInfo.AssetPath);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 创建AssetBundleBuild类
        /// </summary>
        public UnityEditor.AssetBundleBuild CreatePipelineBuild()
        {
            // 注意：我们不再支持AssetBundle的变种机制
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = BundleName;
            build.assetBundleVariant = string.Empty;
            build.assetNames = GetAllMainAssetPaths();
            return build;
        }

        /// <summary>
        /// 获取所有写入补丁清单的资源
        /// </summary>
        public BuildAssetInfo[] GetAllManifestAssetInfos()
        {
            return MainAssets.Where(t => t.CollectorType == ECollectorType.MainAssetCollector).ToArray();
        }

        /// <summary>
        /// 创建PackageBundle类
        /// </summary>
        internal PackageBundle CreatePackageBundle()
        {
            PackageBundle packageBundle = new PackageBundle();
            packageBundle.BundleName = BundleName;
            packageBundle.UnityCRC = PackageUnityCRC;
            packageBundle.FileHash = PackageFileHash;
            packageBundle.FileCRC = PackageFileCRC;
            packageBundle.FileSize = PackageFileSize;
            packageBundle.Encrypted = Encrypted;
            return packageBundle;
        }
    }
}