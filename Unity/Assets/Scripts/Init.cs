using System;
using Base;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			DisposerManager.Instance.Register("Model", typeof(Game).Assembly);
			DisposerManager.Instance.Register("Controller", DllHelper.GetController());

			Game.Scene.AddComponent<EventComponent>().Run(EventIdType.InitSceneStart);
		}

		private void Update()
		{
			try
			{
				DisposerManager.Instance.Update();
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

