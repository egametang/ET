using System;
using System.Collections;
using System.Collections.Generic;

namespace ETModel
{
	public class BTEnv: IEnumerable
	{
		public Dictionary<string, object> Values
		{
			get
			{
				return values;
			}
		}

		private Dictionary<string, object> values = new Dictionary<string, object>();

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
			try
			{
				return (T) value;
			}
			catch (InvalidCastException e)
			{
				throw new Exception($"不能把{value.GetType()}转换为{typeof (T)}", e);
			}
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
	}
}