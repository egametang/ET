using System.Reflection;
using Base;
using UnityEngine;
using Object = Base.Object;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			Object.ObjectManager.Register("Base", typeof(Game).Assembly);
			Object.ObjectManager.Register("Model", typeof(Init).Assembly);
			Object.ObjectManager.Register("Controller", DllHelper.GetController());

			Game.Scene.AddComponent<EventComponent>().Run(EventIdType.InitSceneStart);
		}

		private void Update()
		{
			Base.Object.ObjectManager.Update();
		}

		private void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}

