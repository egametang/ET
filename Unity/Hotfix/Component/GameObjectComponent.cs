using Model;
using UnityEngine;

namespace Hotfix
{
	[ObjectEvent]
	public class GameObjectComponentEvent : ObjectEvent<GameObjectComponent>, IAwake<GameObject>
	{
		public void Awake(GameObject gameObject)
		{
			this.Get().Awake(gameObject);
		}
	}
	
	public class GameObjectComponent: Component
	{
		public GameObject GameObject { get; private set; }

		public void Awake(GameObject gameObject)
		{
			this.GameObject = gameObject;
		}
	}
}