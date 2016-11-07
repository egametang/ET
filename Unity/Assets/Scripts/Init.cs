using System;
using Base;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			ObjectManager.Instance.Register("Model", typeof(Game).Assembly);
			ObjectManager.Instance.Register("Controller", DllHelper.GetController());

			Game.Scene.AddComponent<EventComponent>().Run(EventIdType.InitSceneStart);
		}

		private void Update()
		{
			try
			{
				ObjectManager.Instance.Update();
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

