using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public class WorldActor: Singleton<WorldActor>, ISingletonAwake
    {
        private readonly Dictionary<Type, List<IProcessActorHandler>> handlers = new();

        public void Awake()
        {
            var types = EventSystem.Instance.GetTypes(typeof (ProcessActorHandlerAttribute));
            foreach (Type type in types)
            {
                IProcessActorHandler processActorHandler = Activator.CreateInstance(type) as IProcessActorHandler;
                if (processActorHandler == null)
                {
                    Log.Error($"Process Actor Handler {type.Name} 需要继承 IProcessActorHandler");
                    continue;
                }

                Type messageType = processActorHandler.GetMessageType();
                if (!this.handlers.TryGetValue(messageType, out var list))
                {
                    list = new List<IProcessActorHandler>();
                    this.handlers.Add(messageType, list);
                }
                list.Add(processActorHandler);
            }
        }

        public void Load()
        {
            World.Instance.AddSingleton<WorldActor>();
        }

        public void Handle(ActorId actorId, MessageObject messageObject)
        {
            if (!this.handlers.TryGetValue(messageObject.GetType(), out var list))
            {
                throw new Exception($"not found process actor handler: {messageObject.GetType().FullName}");
            }

            foreach (IProcessActorHandler processActorHandler in list)
            {
                processActorHandler.Handle(actorId, messageObject);
            }
        }
    }
}