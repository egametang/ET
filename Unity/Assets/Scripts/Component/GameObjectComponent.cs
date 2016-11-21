using Base;
using UnityEngine;
using Component = UnityEngine.Component;

namespace Model
{
	[DisposerEvent]
	public class GameObjectComponentEvent : DisposerEvent<GameObjectComponent>, IAwake<GameObject>
	{
		public void Awake(GameObject gameObject)
		{
			this.GetValue().Awake(gameObject);
		}
	}

	public class GameObjectComponent : Component
    {
		public GameObject GameObject { get; private set; }

		public void Awake(GameObject gameObject)
		{
			this.GameObject = gameObject;
		}
    }
}