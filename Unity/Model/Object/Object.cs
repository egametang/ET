using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class Object: ISupportInitialize, IDisposable, IEnumerable
	{
		public static ObjectManager ObjectManager = new ObjectManager();
		[BsonId]
		public long Id { get; private set; }

		public Dictionary<string, object> Values
		{
			get
			{
				return values;
			}
		}

		[BsonElement, BsonIgnoreIfNull]
		private Dictionary<string, object> values = new Dictionary<string, object>();

		protected Object()
		{
			Id = IdGenerater.GenerateId();
		}

		protected Object(long id)
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
            if (this.values == null || !this.values.ContainsKey(key))
            {
                return default(T);
            }
            object value = values[key];
            return (T)value;
		}

		public void Set(string key, object obj)
		{
			if (this.values == null)
			{
				this.values = new Dictionary<string, object>();
			}
			this.values[key] = obj;
		}

		public bool ContainKey(string key)
		{
			if (this.values == null)
			{
				return false;
			}
			return this.values.ContainsKey(key);
		}

		public void Remove(string key)
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

		public void Add(string key, object value)
		{
			if (this.values == null)
			{
				this.values = new Dictionary<string, object>();
			}
			this.values[key] = value;
		}

		public IEnumerator GetEnumerator()
		{
			return this.values.GetEnumerator();
		}

		public virtual void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			this.Id = 0;
		}
	}
}