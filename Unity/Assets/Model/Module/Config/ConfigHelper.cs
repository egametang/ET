using System;
using UnityEngine;

namespace ET
{
	public static class ConfigHelper
	{
		public static string GetText(string key)
		{
			try
			{
				string configStr = ((TextAsset)Game.Scene.GetComponent<ResourcesComponent>().GetAsset("config.unity3d", key)).text;
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, key: {key}", e);
			}
		}

		public static T ToObject<T>(string str)
		{
			return JsonHelper.FromJson<T>(str);
		}
	}
}