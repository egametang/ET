using System.Collections;
using System.Collections.Generic;

namespace Model
{
	public class EQueue<T>: IEnumerable
	{
		private readonly Queue<T> list = new Queue<T>();

		public void Enqueue(T t)
		{
			this.list.Enqueue(t);
		}

		public T Dequeue()
		{
			T t = this.list.Dequeue();
			return t;
		}
		
		public int Count
		{
			get
			{
				return this.list.Count;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		public void Clear()
		{
			this.list.Clear();
		}
	}
}