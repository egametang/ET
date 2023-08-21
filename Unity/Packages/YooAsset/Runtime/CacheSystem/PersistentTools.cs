using System.IO;
using System.Collections.Generic;

namespace YooAsset
{
	internal class PersistentTools
	{
		private static readonly Dictionary<string, Persistent> _persitentDic = new Dictionary<string, Persistent>(100);

		/// <summary>
		/// 获取包裹的持久化类
		/// </summary>
		public static Persistent GetPersistent(string packageName)
		{
			if (_persitentDic.ContainsKey(packageName) == false)
				throw new System.Exception("Should never get here !");
			return _persitentDic[packageName];
		}

		/// <summary>
		/// 获取或创建包裹的持久化类
		/// </summary>
		public static Persistent GetOrCreatePersistent(string packageName)
		{
			if (_persitentDic.ContainsKey(packageName) == false)
			{
				Persistent persistent = new Persistent(packageName);
				_persitentDic.Add(packageName, persistent);
			}
			return _persitentDic[packageName];
		}

		/// <summary>
		/// 获取WWW加载本地资源的路径
		/// </summary>
		public static string ConvertToWWWPath(string path)
		{
#if UNITY_EDITOR
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_IPHONE
			return StringUtility.Format("file://{0}", path);
#elif UNITY_ANDROID
			return path;
#elif UNITY_STANDALONE
			return StringUtility.Format("file:///{0}", path);
#elif UNITY_WEBGL
			return path;
#endif
		}
	}
}