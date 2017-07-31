using System.Collections.Generic;

namespace Model
{
	public class QueueDictionary<T, K>
	{
		private readonly List<T> list = new List<T>();
		private readonly Dictionary<T, K> dictionary = new Dictionary<T, K>();

		public void Add(T t, K k)
		{
			this.list.Add(t);
			this.dictionary.Add(t, k);
		}

		public bool Remove(T t)
		{
			this.list.Remove(t);
			this.dictionary.Remove(t);
			return true;
		}

		public int Count
		{
			get
			{
				return this.list.Count;
			}
		}

		public T FirstKey
		{
			get
			{
				return this.list[0];
			}
		}

		public K this[T t]
		{
			get
			{
				return this.dictionary[t];
			}
		}
	}
}