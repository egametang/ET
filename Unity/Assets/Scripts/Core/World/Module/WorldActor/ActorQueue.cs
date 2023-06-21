using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public struct ActorMessageInfo
    {
        public ActorId ActorId;
        public MessageObject MessageObject;
    }
    
    public class ActorQueue: Singleton<ActorQueue>, ISingletonAwake
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<ActorMessageInfo>> messages = new();
        
        public void Awake()
        {
        }

        public void Send(ActorId actorId, MessageObject messageObject)
        {
            this.Send(actorId.Address, actorId, messageObject);
        }
        
        public void Send(Address fromAddress, ActorId actorId, MessageObject messageObject)
        {
            if (!this.messages.TryGetValue(actorId.Address.VProcess, out var queue))
            {
                return;
            }
            queue.Enqueue(new ActorMessageInfo() {ActorId = new ActorId(fromAddress, actorId.InstanceId), MessageObject = messageObject});
        }
        
        public void Fetch(int processId, int count, List<ActorMessageInfo> list)
        {
            if (!this.messages.TryGetValue(processId, out var queue))
            {
                return;
            }

            for (int i = 0; i < count; ++i)
            {
                if (!queue.TryDequeue(out ActorMessageInfo message))
                {
                    break;
                }
                list.Add(message);
            }
        }

        public void AddQueue(int processId)
        {
            var queue = new ConcurrentQueue<ActorMessageInfo>();
            this.messages[processId] = queue;
        }
        
        public void RemoveQueue(int processId)
        {
            this.messages.TryRemove(processId, out _);
        }
    }
}