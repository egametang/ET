using System;
using System.IO;
using Base;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			Game.EntityEventManager.Register("Model", typeof (Game).Assembly);
			Game.Scene.AddComponent<ILRuntimeComponent>();
		}

		private void Update()
		{
			try
			{
				Game.EntityEventManager.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void OnApplicationQuit()
		{
			Game.CloseScene();
		}
	}
}