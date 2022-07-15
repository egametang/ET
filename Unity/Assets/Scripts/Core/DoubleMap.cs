using System;
using System.Collections.Generic;

namespace ET
{
	public class DoubleMap<K, V>
	{
		private readonly Dictionary<K, V> kv = new Dictionary<K, V>();
		private readonly Dictionary<V, K> vk = new Dictionary<V, K>();

		public DoubleMap()
		{
		}

		public DoubleMap(int capacity)
		{
			kv = new Dictionary<K, V>(capacity);
			vk = new Dictionary<V, K>(capacity);
		}

		public void ForEach(Action<K, V> action)
		{
			if (action == null)
			{
				return;
			}
			Dictionary<K, V>.KeyCollection keys = kv.Keys;
			foreach (K key in keys)
			{
				action(key, kv[key]);
			}
		}

		public List<K> Keys
		{
			get
			{
				return new List<K>(kv.Keys);
			}
		}

		public List<V> Values
		{
			get
			{
				return new List<V>(vk.Keys);
			}
		}

		public void Add(K key, V value)
		{
			if (key == null || value == null || kv.ContainsKey(key) || vk.ContainsKey(value))
			{
				return;
			}
			kv.Add(key, value);
			vk.Add(value, key);
		}

		public V GetValueByKey(K key)
		{
			if (key != null && kv.ContainsKey(key))
			{
				return kv[key];
			}
			return default(V);
		}

		public K GetKeyByValue(V value)
		{
			if (value != null && vk.ContainsKey(value))
			{
				return vk[value];
			}
			return default(K);
		}

		public void RemoveByKey(K key)
		{
			if (key == null)
			{
				return;
			}
			V value;
			if (!kv.TryGetValue(key, out value))
			{
				return;
			}

			kv.Remove(key);
			vk.Remove(value);
		}

		public void RemoveByValue(V value)
		{
			if (value == null)
			{
				return;
			}

			K key;
			if (!vk.TryGetValue(value, out key))
			{
				return;
			}

			kv.Remove(key);
			vk.Remove(value);
		}

		public void Clear()
		{
			kv.Clear();
			vk.Clear();
		}

		public bool ContainsKey(K key)
		{
			if (key == null)
			{
				return false;
			}
			return kv.ContainsKey(key);
		}

		public bool ContainsValue(V value)
		{
			if (value == null)
			{
				return false;
			}
			return vk.ContainsKey(value);
		}

		public bool Contains(K key, V value)
		{
			if (key == null || value == null)
			{
				return false;
			}
			return kv.ContainsKey(key) && vk.ContainsKey(value);
		}
	}
}