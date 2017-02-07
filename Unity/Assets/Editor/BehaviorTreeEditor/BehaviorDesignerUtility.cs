using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public static class BehaviorDesignerUtility
	{
		private static readonly string ImagePathName = "Assets/Editor/BehaviorTreeEditor/Pic/";

		private static readonly Dictionary<string, Texture2D> mTextureDict = new Dictionary<string, Texture2D>();

		public static Texture2D GetTexture(string imageName)
		{
			if (!mTextureDict.ContainsKey(imageName))
			{
				Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ImagePathName + imageName + ".png");
				if (tex != null)
				{
					mTextureDict.Add(imageName, tex);
				}
			}
			return mTextureDict[imageName];
		}

		public static void DrawConnection(Vector2 src, Vector2 dst)
		{
			Vector3[] linePoints = { src, dst };
			Handles.DrawAAPolyLine(GetTexture("DarkTaskIdentifyCompact"), 3f, linePoints);
		}
	}
}