using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YooAsset.Editor
{
    [CreateAssetMenu(fileName = "AssetBundleCollectorSetting", menuName = "YooAsset/Create AssetBundle Collector Settings")]
    public class AssetBundleCollectorSetting : ScriptableObject
    {
        /// <summary>
        /// 显示包裹列表视图
        /// </summary>
        public bool ShowPackageView = false;

        /// <summary>
        /// 是否显示编辑器别名
        /// </summary>
        public bool ShowEditorAlias = false;

        /// <summary>
        /// 资源包名唯一化
        /// </summary>
        public bool UniqueBundleName = false;


        /// <summary>
        /// 包裹列表
        /// </summary>
        public List<AssetBundleCollectorPackage> Packages = new List<AssetBundleCollectorPackage>();


        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void ClearAll()
        {
            ShowPackageView = false;
            UniqueBundleName = false;
            ShowEditorAlias = false;
            Packages.Clear();
        }

        /// <summary>
        /// 检测包裹配置错误
        /// </summary>
        public void CheckPackageConfigError(string packageName)
        {
            var package = GetPackage(packageName);
            package.CheckConfigError();
        }

        /// <summary>
        /// 检测所有配置错误
        /// </summary>
        public void CheckAllPackageConfigError()
        {
            foreach (var package in Packages)
            {
                package.CheckConfigError();
            }
        }

        /// <summary>
        /// 修复所有配置错误
        /// </summary>
        public bool FixAllPackageConfigError()
        {
            bool isFixed = false;
            foreach (var package in Packages)
            {
                if (package.FixConfigError())
                {
                    isFixed = true;
                }
            }
            return isFixed;
        }

        /// <summary>
        /// 获取所有的资源标签
        /// </summary>
        public List<string> GetPackageAllTags(string packageName)
        {
            var package = GetPackage(packageName);
            return package.GetAllTags();
        }

        /// <summary>
        /// 获取包裹收集的资源文件
        /// </summary>
        public CollectResult GetPackageAssets(EBuildMode buildMode, string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                throw new Exception("Build package name is null or empty !");

            // 检测配置合法性
            var package = GetPackage(packageName);
            package.CheckConfigError();

            // 创建资源收集命令
            CollectCommand command = new CollectCommand(buildMode, packageName,
                 package.EnableAddressable,
                 package.LocationToLower,
                 package.IncludeAssetGUID,
                 package.IgnoreDefaultType,
                 package.AutoCollectShaders,
                 UniqueBundleName);

            // 获取收集的资源集合
            CollectResult collectResult = new CollectResult(command);
            collectResult.SetCollectAssets(package.GetAllCollectAssets(command));
            return collectResult;
        }

        /// <summary>
        /// 获取包裹类
        /// </summary>
        public AssetBundleCollectorPackage GetPackage(string packageName)
        {
            foreach (var package in Packages)
            {
                if (package.PackageName == packageName)
                    return package;
            }
            throw new Exception($"Not found package : {packageName}");
        }
    }
}