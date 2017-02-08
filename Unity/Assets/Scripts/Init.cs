using System;
using Base;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			Game.EntityEventManager.Register("Model", typeof (Game).Assembly);
			Game.EntityEventManager.Register("Controller", DllHelper.GetController());

			Game.Scene.AddComponent<EventComponent>().Run(EventIdType.InitSceneStart);
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