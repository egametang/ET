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

			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.Get<TextAsset>("Controller.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Controller.dll.mdb").bytes;
			Assembly assembly = Assembly.Load(assBytes, mdbBytes);
			Object.ObjectManager.Register("Controller", assembly);

			Game.Scene.AddComponent<EventComponent>().Run(EventIdType.InitSceneStart);
		}

		private void Update()
		{
			Base.Object.ObjectManager.Update();
		}
	}
}

