using System;
using Base;
using UnityEditor;
using UnityEngine;
using Model;

namespace MyEditor
{
	[InitializeOnLoad]
	internal class EditorInit
	{
		static EditorInit()
		{
			ObjectManager.Instance.Register("Editor", typeof(EditorInit).Assembly);
			EditorApplication.update += Update;
		}

		private static void Update()
		{
			if (Application.isPlaying)
			{
				return;
			}

			try
			{
				ObjectManager.Instance.Update();
			}
			catch (Exception e)
			{
				ObjectManager.Reset();
				Log.Error(e.ToString());
			}
		}
	}
}
