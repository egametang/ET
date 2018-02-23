using System;
using System.IO;

namespace Model
{
	public static class ConfigHelper
	{
		public static string GetText(string key)
		{
			string path = $"../Config/{key}.txt";
			try
			{
				string configStr = File.ReadAllText(path);
				return configStr;
			}
			catch (Exception e)
			{
				throw new Exception($"load config file fail, path: {path} {e}");
			}
		}
	}
}
