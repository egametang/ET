using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class TaskEncryption
    {
        /// <summary>
        /// 加密文件
        /// </summary>
        public void EncryptingBundleFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
        {
            var encryptionServices = buildParametersContext.Parameters.EncryptionServices;
            if (encryptionServices == null)
                return;

            if (encryptionServices.GetType() == typeof(EncryptionNone))
                return;

            int progressValue = 0;
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                EncryptFileInfo fileInfo = new EncryptFileInfo();
                fileInfo.BundleName = bundleInfo.BundleName;
                fileInfo.FilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                var encryptResult = encryptionServices.Encrypt(fileInfo);
                if (encryptResult.Encrypted)
                {
                    string filePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}.encrypt";
                    FileUtility.WriteAllBytes(filePath, encryptResult.EncryptedData);
                    bundleInfo.EncryptedFilePath = filePath;
                    bundleInfo.Encrypted = true;
                    BuildLogger.Log($"Bundle file encryption complete: {filePath}");
                }
                else
                {
                    bundleInfo.Encrypted = false;
                }

                // 进度条
                EditorTools.DisplayProgressBar("Encrypting bundle", ++progressValue, buildMapContext.Collection.Count);
            }
            EditorTools.ClearProgressBar();
        }
    }
}