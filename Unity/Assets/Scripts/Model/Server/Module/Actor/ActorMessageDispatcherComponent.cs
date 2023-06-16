using System;
using System.Collections.Generic;

namespace ET.Server
{
    public class ActorMessageDispatcherInfo
    {
        public SceneType SceneType { get; }
        
        public IMActorHandler IMActorHandler { get; }

        public ActorMessageDispatcherInfo(SceneType sceneType, IMActorHandler imActorHandler)
        {
            this.SceneType = sceneType;
            this.IMActorHandler = imActorHandler;
        }
    }
    
    /// <summary>
    /// Actor消息分发组件
    /// </summary>
    [ComponentOf(typeof(RootEntity))]
    public class ActorMessageDispatcherComponent: Entity, IAwake, IDestroy, ILoad
    {
        [ThreadStatic]
        [StaticField]
        public static ActorMessageDispatcherComponent Instance;

        public readonly Dictionary<Type, List<ActorMessageDispatcherInfo>> ActorMessageHandlers = new();
        
        public readonly Dictionary<long, Entity> mailboxEntities = new();
    }
}