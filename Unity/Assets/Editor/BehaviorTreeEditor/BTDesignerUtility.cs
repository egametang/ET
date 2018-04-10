using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
	public static class BTDesignerUtility
	{
		private static readonly string ImagePathName = "Assets/Editor/BehaviorTreeEditor/Pic/";

		private static readonly Dictionary<string, Texture2D> mTextureDict = new Dictionary<string, Texture2D>();

		public static Texture2D GetTexture(string imageName)
		{
			try
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
			catch (Exception e)
			{
				throw new Exception($"无法找到资源: {imageName}", e);
			}
		}

		public static void DrawConnection(Vector2 src, Vector2 dst)
		{
			Vector3[] linePoints = { src, dst };
			Handles.DrawAAPolyLine(GetTexture("default"), 3f, linePoints);
		}
	}
}