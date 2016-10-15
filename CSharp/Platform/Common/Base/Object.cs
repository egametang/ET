using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
	public abstract class Object: ISupportInitialize
	{
		[BsonId]
		public ObjectId Id { get; protected set; }

		[BsonElement, BsonIgnoreIfNull]
		private Dictionary<string, object> values;

		protected Object()
		{
			this.Id = ObjectId.GenerateNewId();
		}

		protected Object(ObjectId id)
		{
			this.Id = id;
		}

		public virtual void BeginInit()
		{
			if (this.values == null)
			{
				this.values = new Dictionary<string, object>();
			}
		}

		public virtual void EndInit()
		{
			if (this.values.Count == 0)
			{
				this.values = null;
			}
		}

		public object this[string key]
		{
			get
			{
				return this.values[key];
			}
			set
			{
				if (this.values == null)
				{
					this.values = new Dictionary<string, object>();
				}
				this.values[key] = value;
			}
		}

		public T Get<T>(string key)
		{
			if (!this.values.ContainsKey(key))
			{
				return default(T);
			}
			return (T) this.values[key];
		}

		public T Get<T>()
		{
			return this.Get<T>(typeof (T).Name);
		}

		public void Set(string key, object obj)
		{
			if (this.values == null)
			{
				this.values = new Dictionary<string, object>();
			}
			this.values[key] = obj;
		}

		public void Set<T>(T obj)
		{
			if (this.values == null)
			{
				this.values = new Dictionary<string, object>();
			}
			this.values[typeof (T).Name] = obj;
		}

		public bool ContainKey(string key)
		{
			return this.values.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			bool ret = this.values.Remove(key);
			if (this.values.Count == 0)
			{
				this.values = null;
			}
			return ret;
		}

		public void Add(string key, object value)
		{
			if (this.values == null)
			{
				this.values = new Dictionary<string, object>();
			}
			this.values.Add(key, value);
		}

		public IEnumerator GetEnumerator()
		{
			return this.values.GetEnumerator();
		}
	}
}