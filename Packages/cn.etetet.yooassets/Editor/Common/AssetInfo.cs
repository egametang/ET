using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    [Serializable]
    public class AssetInfo
    {
        private string _fileExtension = null;

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 资源GUID
        /// </summary>
        public string AssetGUID;

        /// <summary>
        /// 资源类型
        /// </summary>
        public System.Type AssetType;

        /// <summary>
        /// 文件格式
        /// </summary>
        public string FileExtension
        {
            get
            {
                if (string.IsNullOrEmpty(_fileExtension))
                    _fileExtension = System.IO.Path.GetExtension(AssetPath);
                return _fileExtension;
            }
        }

        public AssetInfo(string assetPath)
        {
            AssetPath = assetPath;
            AssetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(AssetPath);
            AssetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(AssetPath);
        }

        /// <summary>
        /// 是否为着色器资源
        /// </summary>
        public bool IsShaderAsset()
        {
            if (AssetType == typeof(UnityEngine.Shader) || AssetType == typeof(UnityEngine.ShaderVariantCollection))
                return true;
            else
                return false;
        }
    }
}