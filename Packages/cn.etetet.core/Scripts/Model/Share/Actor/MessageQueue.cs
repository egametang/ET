using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public struct MessageInfo
    {
        public ActorId ActorId;
        public MessageObject MessageObject;
    }
    
    public class MessageQueue: Singleton<MessageQueue>, ISingletonAwake
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<MessageInfo>> messages = new();
        
        public void Awake()
        {
        }

        public bool Send(ActorId actorId, MessageObject messageObject)
        {
            return this.Send(actorId.Address, actorId, messageObject);
        }
        
        public void Reply(ActorId actorId, MessageObject messageObject)
        {
            this.Send(actorId.Address, actorId, messageObject);
        }
        
        public bool Send(Address fromAddress, ActorId actorId, MessageObject messageObject)
        {
            if (!this.messages.TryGetValue(actorId.Address.Fiber, out var queue))
            {
                return false;
            }
            queue.Enqueue(new MessageInfo() {ActorId = new ActorId(fromAddress, actorId.InstanceId), MessageObject = messageObject});
            return true;
        }
        
        public void Fetch(int fiberId, int count, List<MessageInfo> list)
        {
            if (!this.messages.TryGetValue(fiberId, out var queue))
            {
                return;
            }

            for (int i = 0; i < count; ++i)
            {
                if (!queue.TryDequeue(out MessageInfo message))
                {
                    break;
                }
                list.Add(message);
            }
        }

        public void AddQueue(int fiberId)
        {
            var queue = new ConcurrentQueue<MessageInfo>();
            this.messages[fiberId] = queue;
        }
        
        public void RemoveQueue(int fiberId)
        {
            this.messages.TryRemove(fiberId, out _);
        }
    }
}