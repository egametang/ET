using Base;
using UnityEngine;

namespace Model
{
	public class Init: MonoBehaviour
	{
		private void Start()
		{
			Share.Scene.GetComponent<EventComponent>().Run(EventIdType.InitSceneStart);
		}

		private void Update()
		{
			Base.Object.ObjectManager.Update();
		}
	}
}

