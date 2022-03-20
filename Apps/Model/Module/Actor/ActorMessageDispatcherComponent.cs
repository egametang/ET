using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// Actor消息分发组件
    /// </summary>
    public class ActorMessageDispatcherComponent: Entity, IAwake, IDestroy, ILoad
    {
        public static ActorMessageDispatcherComponent Instance;

        public readonly Dictionary<Type, IMActorHandler> ActorMessageHandlers = new Dictionary<Type, IMActorHandler>();
    }
}