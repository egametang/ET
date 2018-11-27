using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ETModel
{
	public static class ResourcesHelper
	{
		public static UnityEngine.Object Load(string path)
		{
			return Resources.Load(path);
		}
	}
}
