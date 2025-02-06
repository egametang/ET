using System.IO;
using UnityEngine;

namespace YooAsset
{
    internal class ResourceLoader
    {
        private IDecryptionServices _decryption;
        private IDeliveryLoadServices _delivery;

        public void Init(IDecryptionServices decryption, IDeliveryLoadServices delivery)
        {
            _decryption = decryption;
            _delivery = delivery;
        }

        /// <summary>
        /// 同步加载资源包对象
        /// </summary>
        public AssetBundle LoadAssetBundle(BundleInfo bundleInfo, string fileLoadPath, out Stream managedStream)
        {
            managedStream = null;
            if (bundleInfo.Bundle.Encrypted)
            {
                if (_decryption == null)
                {
                    YooLogger.Error($"{nameof(IDecryptionServices)} is null ! when load asset bundle {bundleInfo.Bundle.BundleName}!");
                    return null;
                }

                DecryptFileInfo fileInfo = new DecryptFileInfo();
                fileInfo.BundleName = bundleInfo.Bundle.BundleName;
                fileInfo.FileLoadPath = fileLoadPath;
                fileInfo.ConentCRC = bundleInfo.Bundle.UnityCRC;
                return _decryption.LoadAssetBundle(fileInfo, out managedStream);
            }
            else
            {
                return AssetBundle.LoadFromFile(fileLoadPath);
            }
        }

        /// <summary>
        /// 异步加载资源包对象
        /// </summary>
        public AssetBundleCreateRequest LoadAssetBundleAsync(BundleInfo bundleInfo, string fileLoadPath, out Stream managedStream)
        {
            managedStream = null;
            if (bundleInfo.Bundle.Encrypted)
            {
                if (_decryption == null)
                {
                    YooLogger.Error($"{nameof(IDecryptionServices)} is null ! when load asset bundle {bundleInfo.Bundle.BundleName}!");
                    return null;
                }

                DecryptFileInfo fileInfo = new DecryptFileInfo();
                fileInfo.BundleName = bundleInfo.Bundle.BundleName;
                fileInfo.FileLoadPath = fileLoadPath;
                fileInfo.ConentCRC = bundleInfo.Bundle.UnityCRC;
                return _decryption.LoadAssetBundleAsync(fileInfo, out managedStream);
            }
            else
            {
                return AssetBundle.LoadFromFileAsync(fileLoadPath);
            }
        }

        /// <summary>
        /// 同步加载分发的资源包对象
        /// </summary>
        public AssetBundle LoadDeliveryAssetBundle(BundleInfo bundleInfo, string fileLoadPath)
        {
            if (_delivery == null)
                throw new System.Exception("Should never get here !");

            // 注意：对于已经加密的资源包，需要开发者自行解密。
            DeliveryFileInfo fileInfo = new DeliveryFileInfo();
            fileInfo.BundleName = bundleInfo.Bundle.BundleName;
            fileInfo.FileLoadPath = fileLoadPath;
            fileInfo.ConentCRC = bundleInfo.Bundle.UnityCRC;
            fileInfo.Encrypted = bundleInfo.Bundle.Encrypted;
            return _delivery.LoadAssetBundle(fileInfo);
        }

        /// <summary>
        /// 异步加载分发的资源包对象
        /// </summary>
        public AssetBundleCreateRequest LoadDeliveryAssetBundleAsync(BundleInfo bundleInfo, string fileLoadPath)
        {
            if (_delivery == null)
                throw new System.Exception("Should never get here !");

            // 注意：对于已经加密的资源包，需要开发者自行解密。
            DeliveryFileInfo fileInfo = new DeliveryFileInfo();
            fileInfo.BundleName = bundleInfo.Bundle.BundleName;
            fileInfo.FileLoadPath = fileLoadPath;
            fileInfo.ConentCRC = bundleInfo.Bundle.UnityCRC;
            fileInfo.Encrypted = bundleInfo.Bundle.Encrypted;
            return _delivery.LoadAssetBundleAsync(fileInfo);
        }
    }
}