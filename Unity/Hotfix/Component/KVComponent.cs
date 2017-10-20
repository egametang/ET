using System.Collections.Generic;

namespace Hotfix
{
	/// <summary>
	/// Key Value组件用于保存一些数据
	/// </summary>
	public class KVComponent: Component
	{
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
			return (T) k;
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
}