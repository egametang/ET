using System;
using System.Collections.Generic;

namespace ET
{
	public class UnOrderMultiMap<T, K>: Dictionary<T, List<K>>
	{
		private readonly List<K> Empty = new List<K>();
		
		public void Add(T t, K k)
		{
			List<K> list;
			this.TryGetValue(t, out list);
			if (list == null)
			{
				list = MonoPool.Instance.Fetch(typeof (List<K>)) as List<K>;
				list.Clear();
				base[t] = list;
			}
			list.Add(k);
		}

		public bool Remove(T t, K k)
		{
			List<K> list;
			this.TryGetValue(t, out list);
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
				base.Remove(t);
				MonoPool.Instance.Recycle(list);
			}
			return true;
		}
		
		public new bool Remove(T t)
		{
			List<K> list;
			this.TryGetValue(t, out list);
			if (list == null)
			{
				return false;
			}

			base.Remove(t);
            
			list.Clear();
			MonoPool.Instance.Recycle(list);
			return true;
		}

		/// <summary>
		/// 不返回内部的list,copy一份出来
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public K[] GetAll(T t)
		{
			List<K> list;
			this.TryGetValue(t, out list);
			if (list == null)
			{
				return Array.Empty<K>();
			}
			return list.ToArray();
		}

		/// <summary>
		/// 返回内部的list
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public new List<K> this[T t]
		{
			get
			{
				List<K> list;
				if (this.TryGetValue(t, out list))
				{
					return list;
				}
				return this.Empty;
			}
		}
		
		public K GetOne(T t)
		{
			List<K> list;
			this.TryGetValue(t, out list);
			if (list != null && list.Count > 0)
			{
				return list[0];
			}
			return default(K);
		}

		public bool Contains(T t, K k)
		{
			List<K> list;
			this.TryGetValue(t, out list);
			if (list == null)
			{
				return false;
			}
			return list.Contains(k);
		}
	}
}
