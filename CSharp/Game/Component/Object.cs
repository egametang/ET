using System.Collections.Generic;
using MongoDB.Bson;

namespace Component
{
	public class Object
	{
		public ObjectId Id { get; set; }

		public Dictionary<string, object> Dict { get; private set; }

		protected Object()
		{
			this.Id = ObjectId.GenerateNewId();
			this.Dict = new Dictionary<string, object>();
		}

		public object this[string key]
		{
			set
			{
				this.Dict[key] = value;	
			}
			get
			{
				return this.Dict[key];
			}
		}

		public T Get<T>(string key)
		{
			if (!this.Dict.ContainsKey(key))
			{
				throw new KeyNotFoundException(string.Format("not found key: {0}", key));
			}
			return (T)this.Dict[key];
		}

		public bool Contain(string key)
		{
			return this.Dict.ContainsKey(key);
		}
	}
}
