using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	/// <summary>
	/// Key Value组件用于保存一些数据
	/// </summary>
    public class KVComponent<T> : Component<T> where T: Entity<T>
    {
		[BsonElement]
		private readonly Dictionary<string, object> kv = new Dictionary<string, object>();
		
		public void Add(string key, object value)
		{
			this.kv.Add(key, value);
		}
		
		public void Remove(string key)
		{
			this.kv.Remove(key);
		}

		public K Get<K>(string key)
		{
			object k;
			if (!this.kv.TryGetValue(key, out k))
			{
				return default(K);
			}
			return (K)k;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
    }

	public static class KVHelper
	{
		public static void Add<T>(this Entity<T> entity, string key, T value) where T : Entity<T>
		{
			entity.GetComponent<KVComponent<T>>().Add(key, value);
		}

		public static void Remove<T>(this Entity<T> entity, string key) where T : Entity<T>
		{
			entity.GetComponent<KVComponent<T>>().Remove(key);
		}

		public static void Get<T, K>(this Entity<T> entity, string key) where T : Entity<T>
		{
			entity.GetComponent<KVComponent<T>>().Get<K>(key);
		}
	}
}