using System;
using UnityEngine;

namespace Model
{
	public static class ConfigHelper
	{
		public static string GetText(string key)
		{
			try
			{
				GameObject config = Game.Scene.GetComponent<ResourcesComponent>().GetAsset<GameObject>("config.unity3d", "Config");
				string configStr = config.Get<TextAsset>(key).text;
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, key: {key}", e);
			}
		}
		
		public static string GetGlobal()
		{
			try
			{
				GameObject config = (GameObject)Resources.Load("KV");
				string configStr = config.Get<TextAsset>("GlobalProto").text;
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load global config file fail", e);
			}
		}
	}
}