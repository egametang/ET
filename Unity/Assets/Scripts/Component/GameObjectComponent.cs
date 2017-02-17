using UnityEngine;

namespace Model
{
	[EntityEvent(EntityEventId.GameObjectComponent)]
	public class GameObjectComponent: Component
	{
		public GameObject GameObject { get; private set; }

		private void Awake(GameObject gameObject)
		{
			this.GameObject = gameObject;
		}
	}
}