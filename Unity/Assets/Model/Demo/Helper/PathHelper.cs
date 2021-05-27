using UnityEngine;

namespace ET
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
                return Application.streamingAssetsPath;
            }
        }

        /// <summary>
        /// 应用程序内部资源路径存放路径(www/webrequest专用)
        /// </summary>
        public static string AppResPath4Web
        {
            get
            {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                return $"file://{Application.streamingAssetsPath}";
#else
                return Application.streamingAssetsPath;
#endif

            }
        }
    }
}
