#if UNITY_2019_4_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
	internal class HomePageWindow
	{
		[MenuItem("YooAsset/Home Page", false, 1)]
		public static void OpenWindow()
		{
			Application.OpenURL("https://www.yooasset.com/");
		}
	}
}
#endif