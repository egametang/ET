using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	/// <summary>
	/// Key Value组件用于保存一些数据
	/// </summary>
    public class KVComponent : Component
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

		public T Get<T>(string key)
		{
			object k;
			if (!this.kv.TryGetValue(key, out k))
			{
				return default(T);
			}
			return (T)k;
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
		public static void KVAdd(this Entity entity, string key, object value)
		{
			entity.GetComponent<KVComponent>().Add(key, value);
		}

		public static void KVRemove(this Entity entity, string key)
		{
			entity.GetComponent<KVComponent>().Remove(key);
		}

		public static void KVGet<T>(this Entity entity, string key)
		{
			entity.GetComponent<KVComponent>().Get<T>(key);
		}
	}
}