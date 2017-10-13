using System.Collections;
using System.Collections.Generic;

namespace Model
{
	public class EQueue<T>: IEnumerable
	{
		private readonly Queue<T> queue = new Queue<T>();

		public void Enqueue(T t)
		{
			this.queue.Enqueue(t);
		}

		public T Dequeue()
		{
			return this.queue.Dequeue();
		}
		
		public int Count
		{
			get
			{
				return this.queue.Count;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return this.queue.GetEnumerator();
		}

		public void Clear()
		{
			this.queue.Clear();
		}
	}
}