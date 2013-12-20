using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace Component
{
	public class GameObjectManager
	{
		private readonly Dictionary<ObjectId, GameObject> gameObjects = 
				new Dictionary<ObjectId, GameObject>();

		private readonly Dictionary<int, Dictionary<ObjectId, GameObject>> typeGameObjects = 
				new Dictionary<int, Dictionary<ObjectId, GameObject>>();

		public void Add(GameObject gameObject)
		{
			this.gameObjects.Add(gameObject.Id, gameObject);
			if (!this.typeGameObjects.ContainsKey(gameObject.Type))
			{
				this.typeGameObjects.Add(gameObject.Type, new Dictionary<ObjectId, GameObject>());
			}
			this.typeGameObjects[gameObject.Type].Add(gameObject.Id, gameObject);
		}

		public GameObject Get(ObjectId id)
		{
			GameObject gameObject = null;
			this.gameObjects.TryGetValue(id, out gameObject);
			return gameObject;
		}

		public GameObject[] GetOneType(int type)
		{
			Dictionary<ObjectId, GameObject> oneTypeGameObjects = null;
			if (!this.typeGameObjects.TryGetValue(type, out oneTypeGameObjects))
			{
				return new GameObject[0];
			}
			return oneTypeGameObjects.Values.ToArray();
		}

		public bool Remove(GameObject gameObject)
		{
			if (gameObject == null)
			{
				throw new ArgumentNullException("gameObject");
			}
			if (!this.gameObjects.Remove(gameObject.Id))
			{
				return false;
			}
			if (!this.typeGameObjects[gameObject.Type].Remove(gameObject.Id))
			{
				return false;
			}
			return true;
		}

		public bool Remove(ObjectId id)
		{
			GameObject gameObject = this.Get(id);
			if (gameObject == null)
			{
				return false;
			}
			return this.Remove(gameObject);
		}
	}
}
