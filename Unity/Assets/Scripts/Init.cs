using System;
using System.Reflection;
using Base;
using MongoDB.Bson.Serialization;
using UnityEngine;
using Object = Base.Object;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			BsonClassMapRegister.Register();
			Object.ObjectManager.Register("Base", typeof(Game).Assembly);
			Object.ObjectManager.Register("Model", typeof(Init).Assembly);
			Object.ObjectManager.Register("Controller", DllHelper.GetController());

			Game.Scene.AddComponent<EventComponent>().Run(EventIdType.InitSceneStart);
		}

		private void Update()
		{
			try
			{
				Base.Object.ObjectManager.Update();
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

