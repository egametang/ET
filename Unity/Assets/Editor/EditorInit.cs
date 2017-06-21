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
			ObjectEvents.Instance.Register("Model", typeof (Game).Assembly);
			ObjectEvents.Instance.Register("Editor", typeof (EditorInit).Assembly);

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
				ObjectEvents.Instance.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}