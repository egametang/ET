using System;
using UnityEngine;

namespace Model
{
	public static class ConfigHelper
	{
        private static GameObject ConfigGameObject = null;
		public static string GetText(string key)
		{
			try
            {
                if (!ConfigGameObject) { ConfigGameObject = (GameObject)ResourceHelper.LoadResource("fromework/config", "Config"); }
                string configStr = ConfigGameObject.Get<TextAsset>(key).text;
                return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, key: {key}", e);
			}
		}
	}
}