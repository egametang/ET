
using System;
using System.Text;
using UnityEngine;

namespace ETModel
{
    public static class PathHelper
    {     /// <summary>
          ///应用程序外部资源路径存放路径(热更新资源路径)
          /// </summary>
        public static string AppHotfixResPath
        {
            get
            {
                string game = Application.productName;
                string path = AppResPath;
                if (Application.isMobilePlatform)
                {
                    path = $"{Application.persistentDataPath}/{game}/";
                }
                return path;
            }

        }

        /// <summary>
        /// 应用程序内部资源路径存放路径
        /// </summary>
        public static string AppResPath
        {
            get
            {
                string path = string.Empty;
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        path = $"jar:file://{Application.dataPath}!!/assets/";
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        path = $"{Application.dataPath}/Raw/";
                        break;
                    default:
                        path = $"{Application.dataPath}/StreamingAssets/";
                        break;
                }
                return path;
            }
        }
    }
}
