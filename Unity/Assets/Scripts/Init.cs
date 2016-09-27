using System.Reflection;
using UnityEngine;

namespace Base
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			Share.Scene.GetComponent<EventComponent>().Run(EventIdType.InitSceneStart);
		}

		private void Update()
		{
			Object.ObjectManager.Update();
		}
	}
}

