using System;
using System.Collections.Generic;

namespace ET
{
    public class MessagePool
    {
        private readonly Dictionary<Type, Queue<MessageObject>> pool = new();
        
        public T Fetch<T>() where T: MessageObject
        {
            return this.Fetch(typeof (T)) as T;
        }

        // 只有客户端才用消息池，服务端不使用
        public MessageObject Fetch(Type type)
        {
            lock (this.pool)
            {
                MessageObject messageObject;
                Queue<MessageObject> queue = null;
                if (!pool.TryGetValue(type, out queue))
                {
                    messageObject = Activator.CreateInstance(type) as MessageObject;
                }
                else if (queue.Count == 0)
                {
                    messageObject = Activator.CreateInstance(type) as MessageObject;
                }
                else
                {
                    messageObject = queue.Dequeue();
                }
                
                messageObject.IsFromPool = true;
                return messageObject;
            }
        }

        public void Recycle(MessageObject obj)
        {
            if (!obj.IsFromPool)
            {
                return;
            }
            Type type = obj.GetType();
            
            lock (this.pool)
            {
                Queue<MessageObject> queue = null;
                if (!pool.TryGetValue(type, out queue))
                {
                    queue = new Queue<MessageObject>();
                    pool.Add(type, queue);
                }

                // 一种对象最大为100个
                if (queue.Count > 100)
                {
                    return;
                }
                queue.Enqueue(obj);
            }
        }
    }
}