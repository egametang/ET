using UnityEngine;

namespace Model
{
	[DisposerEvent(typeof(GameObjectComponent))]
	public class GameObjectComponent : Component
    {
		public GameObject GameObject { get; private set; }

		public void Awake(GameObject gameObject)
		{
			this.GameObject = gameObject;
		}
    }
}