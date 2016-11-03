using System;
using Base;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			Object.ObjectManager.Register("Model", typeof(Game).Assembly);
			Object.ObjectManager.Register("Controller", DllHelper.GetController());

			Game.Scene.AddComponent<EventComponent>().Run(EventIdType.InitSceneStart);
		}

		private void Update()
		{
			try
			{
				Object.ObjectManager.Update();
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}

