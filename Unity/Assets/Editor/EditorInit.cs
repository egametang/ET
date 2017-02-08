using System;
using Base;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	[InitializeOnLoad]
	internal class EditorInit
	{
		static EditorInit()
		{
			Game.EntityEventManager.Register("Model", typeof (Game).Assembly);
			Game.EntityEventManager.Register("Editor", typeof (EditorInit).Assembly);

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
				Game.EntityEventManager.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}