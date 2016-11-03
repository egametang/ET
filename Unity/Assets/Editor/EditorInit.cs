using System;
using Base;
using UnityEditor;
using UnityEngine;
using Model;
using Object = Model.Object;

namespace MyEditor
{
	[InitializeOnLoad]
	internal class EditorInit
	{
		static EditorInit()
		{
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
				Object.ObjectManager.Update();
			}
			catch (Exception e)
			{
				Object.ObjectManager.Dispose();
				Object.ObjectManager = new ObjectManager();
				Log.Error(e.ToString());
			}
		}
	}
}
