using System;
using ETModel;
using UnityEngine;

namespace ETHotfix
{
	public static class ConfigHelper
	{
        /// <summary>
        /// 读取文本
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		public static string GetText(string key)
		{
			try
			{
				GameObject config = (GameObject)ETModel.Game.Scene.GetComponent<ResourcesComponent>().GetAsset("config.unity3d", "Config");
				string configStr = config.Get<TextAsset>(key).text;
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, key: {key}", e);
			}
		}

        /// <summary>
        /// 文本还原成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
		public static T ToObject<T>(string str)
		{
			return JsonHelper.FromJson<T>(str);
		}

        /// <summary>
        /// 读取敏感字库
        /// </summary>
	    public static void LoadMinGanText()
	    {
	        string str = GetText("MinGan");
            IllegalWordDetectionHelper.Init(str);
        }
    }
}