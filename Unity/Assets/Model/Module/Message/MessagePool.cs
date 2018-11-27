using System;
#if !SERVER
using System.Collections.Generic;		
#endif

namespace ETModel
{
	// 客户端为了0GC需要消息池，服务端消息需要跨协程不需要消息池
	public class MessagePool
	{
		public static MessagePool Instance { get; } = new MessagePool();

#if !SERVER
		private readonly Dictionary<Type, Queue<object>> dictionary = new Dictionary<Type, Queue<object>>();
#endif

		public object Fetch(Type type)
		{
#if !SERVER
			Queue<object> queue;
			if (!this.dictionary.TryGetValue(type, out queue))
			{
				queue = new Queue<object>();
				this.dictionary.Add(type, queue);
			}

			object obj;
			if (queue.Count > 0)
			{
				obj = queue.Dequeue();
			}
			else
			{
				obj = Activator.CreateInstance(type);
			}

			return obj;
#else
			return Activator.CreateInstance(type);
#endif
		}

		public T Fetch<T>() where T : class
		{
			T t = (T) this.Fetch(typeof (T));
			return t;
		}

		public void Recycle(object obj)
		{
#if !SERVER
			Type type = obj.GetType();
			Queue<object> queue;
			if (!this.dictionary.TryGetValue(type, out queue))
			{
				queue = new Queue<object>();
				this.dictionary.Add(type, queue);
			}

			queue.Enqueue(obj);
#endif
		}
	}
}