using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	public class Env
	{
		[BsonElement, BsonIgnoreIfNull]
		private Dictionary<EnvKey, object> values = new Dictionary<EnvKey, object>();

		public object this[EnvKey key]
		{
			get
			{
				return this.values[key];
			}
			set
			{
				if (this.values == null)
				{
					this.values = new Dictionary<EnvKey, object>();
				}
				this.values[key] = value;
			}
		}

		public T Get<T>(EnvKey key)
		{
			if (this.values == null || !this.values.ContainsKey(key))
			{
				return default(T);
			}
			object value = values[key];
			try
			{
				return (T) value;
			}
			catch (InvalidCastException e)
			{
				throw new Exception($"不能把{value.GetType()}转换为{typeof (T)}", e);
			}
		}

		public void Set(EnvKey key, object obj)
		{
			if (this.values == null)
			{
				this.values = new Dictionary<EnvKey, object>();
			}
			this.values[key] = obj;
		}

		public bool ContainKey(EnvKey key)
		{
			if (this.values == null)
			{
				return false;
			}
			return this.values.ContainsKey(key);
		}

		public void Remove(EnvKey key)
		{
			if (this.values == null)
			{
				return;
			}
			this.values.Remove(key);
			if (this.values.Count == 0)
			{
				this.values = null;
			}
		}

		public void Add(EnvKey key, object value)
		{
			if (this.values == null)
			{
				this.values = new Dictionary<EnvKey, object>();
			}
			this.values[key] = value;
		}

		public IEnumerator GetEnumerator()
		{
			return this.values.GetEnumerator();
		}
	}
}