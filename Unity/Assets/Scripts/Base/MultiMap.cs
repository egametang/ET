using System.Collections.Generic;

namespace Model
{
	public class MultiMap<T, K>
	{
		private readonly SortedDictionary<T, List<K>> dictionary = new SortedDictionary<T, List<K>>();

		public SortedDictionary<T, List<K>>.KeyCollection Keys
		{
			get
			{
				return this.dictionary.Keys;
			}
		}

		public void Add(T t, K k)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				list = new List<K>();
			}
			list.Add(k);
			this.dictionary[t] = list;
		}

		public bool Remove(T t, K k)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				return false;
			}
			if (!list.Remove(k))
			{
				return false;
			}
			if (list.Count == 0)
			{
				this.dictionary.Remove(t);
			}
			return true;
		}

		public bool Remove(T t)
		{
			return this.dictionary.Remove(t);
		}

		/// <summary>
		/// 不返回内部的list,copy一份出来
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public K[] GetAll(T t)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				return new K[0];
			}
			return list.ToArray();
		}

		/// <summary>
		/// 返回内部的list
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public List<K> this[T t]
		{
			get
			{
				List<K> list;
				this.dictionary.TryGetValue(t, out list);
				return list;
			}
		}

		public K GetOne(T t)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if ((list != null) && (list.Count > 0))
			{
				return list[0];
			}
			return default(K);
		}

		public bool Contains(T t, K k)
		{
			List<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				return false;
			}
			return list.Contains(k);
		}
	}
}