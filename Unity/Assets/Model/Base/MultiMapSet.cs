﻿using System.Collections.Generic;
using System.Linq;

namespace ET
{
	public class MultiMapSet<T, K>
	{
		private readonly SortedDictionary<T, HashSet<K>> dictionary = new SortedDictionary<T, HashSet<K>>();

		// 重用list
		private static readonly Queue<HashSet<K>> queue = new Queue<HashSet<K>>();
		
		private static HashSet<K> Empty = new HashSet<K>();

		public SortedDictionary<T, HashSet<K>> GetDictionary()
		{
			return this.dictionary;
		}

		public void Add(T t, K k)
		{
			HashSet<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				list = this.FetchList();
				this.dictionary[t] = list;
			}
			list.Add(k);
		}

		public KeyValuePair<T, HashSet<K>> First()
		{
			return this.dictionary.First();
		}

		public T FirstKey()
		{
			return this.dictionary.Keys.First();
		}

		public int Count
		{
			get
			{
				return this.dictionary.Count;
			}
		}

		private HashSet<K> FetchList()
		{
			if (queue.Count > 0)
			{
				HashSet<K> list = queue.Dequeue();
				list.Clear();
				return list;
			}
			return new HashSet<K>();
		}

		private void RecycleList(HashSet<K> list)
		{
			list.Clear();
			queue.Enqueue(list);
		}

		public bool Remove(T t, K k)
		{
			HashSet<K> list;
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
				this.RecycleList(list);
				this.dictionary.Remove(t);
			}
			return true;
		}

		public bool Remove(T t)
		{
			HashSet<K> list = null;
			this.dictionary.TryGetValue(t, out list);
			if (list != null)
			{
				this.RecycleList(list);
			}
			return this.dictionary.Remove(t);
		}

		/// <summary>
		/// 不返回内部的list,copy一份出来
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public K[] GetAll(T t)
		{
			HashSet<K> list;
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
		public HashSet<K> this[T t]
		{
			get
			{
				this.dictionary.TryGetValue(t, out var list);
				return list ?? Empty;
			}
		}

		public K GetOne(T t)
		{
			HashSet<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list != null && list.Count > 0)
			{
				return list.FirstOrDefault();
			}
			return default(K);
		}

		public bool Contains(T t, K k)
		{
			HashSet<K> list;
			this.dictionary.TryGetValue(t, out list);
			if (list == null)
			{
				return false;
			}
			return list.Contains(k);
		}

		public bool ContainsKey(T t)
		{
			return this.dictionary.ContainsKey(t);
		}

		public void Clear()
		{
			foreach (HashSet<K> list in this.dictionary.Values)
			{
				list.Clear();
				queue.Enqueue(list);
			}
			dictionary.Clear();
		}
	}
}