using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal static class ManifestTools
    {
#if UNITY_EDITOR
        /// <summary>
        /// 序列化（JSON文件）
        /// </summary>
        public static void SerializeToJson(string savePath, PackageManifest manifest)
        {
            string json = JsonUtility.ToJson(manifest, true);
            FileUtility.WriteAllText(savePath, json);
        }

        /// <summary>
        /// 序列化（二进制文件）
        /// </summary>
        public static void SerializeToBinary(string savePath, PackageManifest manifest)
        {
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                // 创建缓存器
                BufferWriter buffer = new BufferWriter(YooAssetSettings.ManifestFileMaxSize);

                // 写入文件标记
                buffer.WriteUInt32(YooAssetSettings.ManifestFileSign);

                // 写入文件版本
                buffer.WriteUTF8(manifest.FileVersion);

                // 写入文件头信息
                buffer.WriteBool(manifest.EnableAddressable);
                buffer.WriteBool(manifest.LocationToLower);
                buffer.WriteBool(manifest.IncludeAssetGUID);
                buffer.WriteInt32(manifest.OutputNameStyle);
                buffer.WriteUTF8(manifest.BuildPipeline);
                buffer.WriteUTF8(manifest.PackageName);
                buffer.WriteUTF8(manifest.PackageVersion);

                // 写入资源列表
                buffer.WriteInt32(manifest.AssetList.Count);
                for (int i = 0; i < manifest.AssetList.Count; i++)
                {
                    var packageAsset = manifest.AssetList[i];
                    buffer.WriteUTF8(packageAsset.Address);
                    buffer.WriteUTF8(packageAsset.AssetPath);
                    buffer.WriteUTF8(packageAsset.AssetGUID);
                    buffer.WriteUTF8Array(packageAsset.AssetTags);
                    buffer.WriteInt32(packageAsset.BundleID);
                }

                // 写入资源包列表
                buffer.WriteInt32(manifest.BundleList.Count);
                for (int i = 0; i < manifest.BundleList.Count; i++)
                {
                    var packageBundle = manifest.BundleList[i];
                    buffer.WriteUTF8(packageBundle.BundleName);
                    buffer.WriteUInt32(packageBundle.UnityCRC);
                    buffer.WriteUTF8(packageBundle.FileHash);
                    buffer.WriteUTF8(packageBundle.FileCRC);
                    buffer.WriteInt64(packageBundle.FileSize);
                    buffer.WriteBool(packageBundle.Encrypted);
                    buffer.WriteUTF8Array(packageBundle.Tags);
                    buffer.WriteInt32Array(packageBundle.DependIDs);
                }

                // 写入文件流
                buffer.WriteToStream(fs);
                fs.Flush();
            }
        }

        /// <summary>
        /// 反序列化（JSON文件）
        /// </summary>
        public static PackageManifest DeserializeFromJson(string jsonContent)
        {
            return JsonUtility.FromJson<PackageManifest>(jsonContent);
        }

        /// <summary>
        /// 反序列化（二进制文件）
        /// </summary>
        public static PackageManifest DeserializeFromBinary(byte[] binaryData)
        {
            // 创建缓存器
            BufferReader buffer = new BufferReader(binaryData);

            // 读取文件标记
            uint fileSign = buffer.ReadUInt32();
            if (fileSign != YooAssetSettings.ManifestFileSign)
                throw new Exception("Invalid manifest file !");

            // 读取文件版本
            string fileVersion = buffer.ReadUTF8();
            if (fileVersion != YooAssetSettings.ManifestFileVersion)
                throw new Exception($"The manifest file version are not compatible : {fileVersion} != {YooAssetSettings.ManifestFileVersion}");

            PackageManifest manifest = new PackageManifest();
            {
                // 读取文件头信息
                manifest.FileVersion = fileVersion;
                manifest.EnableAddressable = buffer.ReadBool();
                manifest.LocationToLower = buffer.ReadBool();
                manifest.IncludeAssetGUID = buffer.ReadBool();
                manifest.OutputNameStyle = buffer.ReadInt32();
                manifest.BuildPipeline = buffer.ReadUTF8();
                manifest.PackageName = buffer.ReadUTF8();
                manifest.PackageVersion = buffer.ReadUTF8();

                // 检测配置
                if (manifest.EnableAddressable && manifest.LocationToLower)
                    throw new Exception("Addressable not support location to lower !");

                // 读取资源列表
                int packageAssetCount = buffer.ReadInt32();
                manifest.AssetList = new List<PackageAsset>(packageAssetCount);
                for (int i = 0; i < packageAssetCount; i++)
                {
                    var packageAsset = new PackageAsset();
                    packageAsset.Address = buffer.ReadUTF8();
                    packageAsset.AssetPath = buffer.ReadUTF8();
                    packageAsset.AssetGUID = buffer.ReadUTF8();
                    packageAsset.AssetTags = buffer.ReadUTF8Array();
                    packageAsset.BundleID = buffer.ReadInt32();
                    manifest.AssetList.Add(packageAsset);
                }

                // 读取资源包列表
                int packageBundleCount = buffer.ReadInt32();
                manifest.BundleList = new List<PackageBundle>(packageBundleCount);
                for (int i = 0; i < packageBundleCount; i++)
                {
                    var packageBundle = new PackageBundle();
                    packageBundle.BundleName = buffer.ReadUTF8();
                    packageBundle.UnityCRC = buffer.ReadUInt32();
                    packageBundle.FileHash = buffer.ReadUTF8();
                    packageBundle.FileCRC = buffer.ReadUTF8();
                    packageBundle.FileSize = buffer.ReadInt64();
                    packageBundle.Encrypted = buffer.ReadBool();
                    packageBundle.Tags = buffer.ReadUTF8Array();
                    packageBundle.DependIDs = buffer.ReadInt32Array();
                    manifest.BundleList.Add(packageBundle);
                }
            }

            // 填充BundleDic
            manifest.BundleDic1 = new Dictionary<string, PackageBundle>(manifest.BundleList.Count);
            manifest.BundleDic2 = new Dictionary<string, PackageBundle>(manifest.BundleList.Count);
            foreach (var packageBundle in manifest.BundleList)
            {
                packageBundle.ParseBundle(manifest);
                manifest.BundleDic1.Add(packageBundle.BundleName, packageBundle);
                manifest.BundleDic2.Add(packageBundle.FileName, packageBundle);
            }

            // 填充AssetDic
            manifest.AssetDic = new Dictionary<string, PackageAsset>(manifest.AssetList.Count);
            foreach (var packageAsset in manifest.AssetList)
            {
                // 注意：我们不允许原始路径存在重名
                string assetPath = packageAsset.AssetPath;
                if (manifest.AssetDic.ContainsKey(assetPath))
                    throw new Exception($"AssetPath have existed : {assetPath}");
                else
                    manifest.AssetDic.Add(assetPath, packageAsset);
            }

            return manifest;
        }
#endif

        /// <summary>
        /// 注意：该类拷贝自编辑器
        /// </summary>
        private enum EFileNameStyle
        {
            /// <summary>
            /// 哈希值名称
            /// </summary>
            HashName = 0,

            /// <summary>
            /// 资源包名称（不推荐）
            /// </summary>
            BundleName = 1,

            /// <summary>
            /// 资源包名称 + 哈希值名称
            /// </summary>
            BundleName_HashName = 2,
        }

        /// <summary>
        /// 获取资源文件的后缀名
        /// </summary>
        public static string GetRemoteBundleFileExtension(string bundleName)
        {
            string fileExtension = Path.GetExtension(bundleName);
            return fileExtension;
        }

        /// <summary>
        /// 获取远端的资源文件名
        /// </summary>
        public static string GetRemoteBundleFileName(int nameStyle, string bundleName, string fileExtension, string fileHash)
        {
            if (nameStyle == (int)EFileNameStyle.HashName)
            {
                return StringUtility.Format("{0}{1}", fileHash, fileExtension);
            }
            else if (nameStyle == (int)EFileNameStyle.BundleName)
            {
                return bundleName;
            }
            else if (nameStyle == (int)EFileNameStyle.BundleName_HashName)
            {
                string fileName = bundleName.Remove(bundleName.LastIndexOf('.'));
                return StringUtility.Format("{0}_{1}{2}", fileName, fileHash, fileExtension);
            }
            else
            {
                throw new NotImplementedException($"Invalid name style : {nameStyle}");
            }
        }
    }
}