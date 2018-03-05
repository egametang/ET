using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	/// <summary>
	/// Key Value组件用于保存一些数据
	/// </summary>
	public class KVComponent: Component
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
			if (!this.kv.TryGetValue(key, out object k))
			{
				return default(T);
			}
			return (T) k;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}