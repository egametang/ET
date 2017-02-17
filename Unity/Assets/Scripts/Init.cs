using System;
using System.IO;
using Base;
using ILRuntime.CLR.Method;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private readonly object[] param0 = new object[0];
		private IMethod start;

		private void Start()
		{
			Game.EntityEventManager.RegisterILRuntime();
			Game.EntityEventManager.RegisterILAdapter();
			Game.EntityEventManager.Register("Model", typeof (Game).Assembly);
			ILRuntime.Runtime.Enviorment.AppDomain appDomain = Game.EntityEventManager.AppDomain;
			Game.Scene.AddComponent<ResourcesComponent>();
			Game.Scene.AddComponent<UIComponent>();
			Game.Scene.AddComponent<UnitComponent>();

			EventHelper.Run(EventIdType.InitSceneStart);

			//this.start = appDomain.LoadedTypes["Hotfix.HotfixEntry"].GetMethod("Start", 0);
			//appDomain.Invoke(this.start, null, param0);
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