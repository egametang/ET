using System.Collections.Generic;
using MongoDB.Bson;

namespace Component
{
	public class Object
	{
		public ObjectId Id { get; set; }

		public Dictionary<string, object> Values { get; private set; }

		protected Object()
		{
			this.Id = ObjectId.GenerateNewId();
			this.Values = new Dictionary<string, object>();
		}

		public object this[string key]
		{
			set
			{
				this.Values[key] = value;	
			}
			get
			{
				return this.Values[key];
			}
		}

		public T Get<T>(string key)
		{
			if (!this.Values.ContainsKey(key))
			{
				throw new KeyNotFoundException(string.Format("not found key: {0}", key));
			}
			return (T)this.Values[key];
		}

		public bool Contain(string key)
		{
			return this.Values.ContainsKey(key);
		}
	}
}
