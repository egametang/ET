using System;
using Base;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	[InitializeOnLoad]
	internal class EditorInit
	{
		static EditorInit()
		{
			Game.DisposerEventManager.Register("Editor", typeof(EditorInit).Assembly);
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
				Game.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}
