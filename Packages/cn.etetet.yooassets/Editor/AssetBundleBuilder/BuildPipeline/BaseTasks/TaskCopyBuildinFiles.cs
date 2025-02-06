using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskCopyBuildinFiles
    {
        /// <summary>
        /// 拷贝首包资源文件
        /// </summary>
        internal void CopyBuildinFilesToStreaming(BuildParametersContext buildParametersContext, PackageManifest manifest)
        {
            EBuildinFileCopyOption copyOption = buildParametersContext.Parameters.BuildinFileCopyOption;
            string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            string buildinRootDirectory = buildParametersContext.GetBuildinRootDirectory();
            string buildPackageName = buildParametersContext.Parameters.PackageName;
            string buildPackageVersion = buildParametersContext.Parameters.PackageVersion;

            // 清空内置文件的目录
            if (copyOption == EBuildinFileCopyOption.ClearAndCopyAll || copyOption == EBuildinFileCopyOption.ClearAndCopyByTags)
            {
                EditorTools.ClearFolder(buildinRootDirectory);
            }

            // 拷贝补丁清单文件
            {
                string fileName = YooAssetSettingsData.GetManifestBinaryFileName(buildPackageName, buildPackageVersion);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{buildinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单哈希文件
            {
                string fileName = YooAssetSettingsData.GetPackageHashFileName(buildPackageName, buildPackageVersion);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{buildinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单版本文件
            {
                string fileName = YooAssetSettingsData.GetPackageVersionFileName(buildPackageName);
                string sourcePath = $"{packageOutputDirectory}/{fileName}";
                string destPath = $"{buildinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝文件列表（所有文件）
            if (copyOption == EBuildinFileCopyOption.ClearAndCopyAll || copyOption == EBuildinFileCopyOption.OnlyCopyAll)
            {
                foreach (var packageBundle in manifest.BundleList)
                {
                    string sourcePath = $"{packageOutputDirectory}/{packageBundle.FileName}";
                    string destPath = $"{buildinRootDirectory}/{packageBundle.FileName}";
                    EditorTools.CopyFile(sourcePath, destPath, true);
                }
            }

            // 拷贝文件列表（带标签的文件）
            if (copyOption == EBuildinFileCopyOption.ClearAndCopyByTags || copyOption == EBuildinFileCopyOption.OnlyCopyByTags)
            {
                string[] tags = buildParametersContext.Parameters.BuildinFileCopyParams.Split(';');
                foreach (var packageBundle in manifest.BundleList)
                {
                    if (packageBundle.HasTag(tags) == false)
                        continue;
                    string sourcePath = $"{packageOutputDirectory}/{packageBundle.FileName}";
                    string destPath = $"{buildinRootDirectory}/{packageBundle.FileName}";
                    EditorTools.CopyFile(sourcePath, destPath, true);
                }
            }

            // 刷新目录
            AssetDatabase.Refresh();
            BuildLogger.Log($"Buildin files copy complete: {buildinRootDirectory}");
        }
    }
}