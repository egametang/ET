using System;
using System.Linq;
using UnityEngine;

namespace Base
{
	public static class GameObjectHelper
	{
		public static T Get<T>(this GameObject gameObject, string key) where T : class
		{
			try
			{
				return gameObject.GetComponent<ReferenceCollector>().Get<T>(key);
			}
			catch (Exception e)
			{
				throw new Exception($"获取 {gameObject.name} ReferenceCollector key {key} 失败", e);
			}
		}
	}
}