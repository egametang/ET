using UnityEngine;

namespace ETHotfix
{
	public static class Game
	{
		private static EventSystem eventSystem;

		public static EventSystem EventSystem
		{
			get
			{
				return eventSystem ?? (eventSystem = new EventSystem());
			}
		}
		
		private static Scene scene;

		public static Scene Scene
		{
			get
			{
				if (scene != null)
				{
					return scene;
				}
				scene = new Scene() { Name = "ClientHotfix" };
				scene.GameObject.transform.SetParent(scene.GameObject.transform.Find("/Global"));
				return scene;
			}
		}

		private static ObjectPool objectPool;

		public static ObjectPool ObjectPool
		{
			get
			{
				if (objectPool != null)
				{
					return objectPool;
				}
				objectPool = new ObjectPool();
				objectPool.GameObject.transform.SetParent(GameObject.Find("/Global").transform);
				return objectPool;
			}
		}

		public static void Close()
		{
			scene.Dispose();
			scene = null;
			eventSystem = null;
			objectPool.Dispose();
			objectPool = null;
		}
	}
}