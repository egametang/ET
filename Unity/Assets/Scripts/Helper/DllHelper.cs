using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ETModel
{
	public static class DllHelper
	{
		public static Type[] GetMonoTypes()
		{
			List<Type> types = new List<Type>();
			foreach (Assembly assembly in Game.EventSystem.GetAll())
			{
				types.AddRange(assembly.GetTypes());
			}
			
			return types.ToArray();
		}

		public static Type[] GetAllTypes()
		{
			List<Type> types = new List<Type>();
			foreach (Assembly assembly in Game.EventSystem.GetAll())
			{
				types.AddRange(assembly.GetTypes());
			}

			types.AddRange(Game.Hotfix.GetHotfixTypes());
			return types.ToArray();
		}
	}
}