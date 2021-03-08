﻿﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public struct ProcessActorId
    {
        public int Process;
        public long ActorId;

        public ProcessActorId(long actorId)
        {
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
            this.Process = instanceIdStruct.Process;
            instanceIdStruct.Process = Game.Options.Process;
            this.ActorId = instanceIdStruct.ToLong();
        }
    }

    public class NetInnerComponent: Entity
    {
        public AService Service;

        public static NetInnerComponent Instance;

        public IMessageDispatcher MessageDispatcher { get; set; }
        
        public Random Random = new Random(Guid.NewGuid().GetHashCode());
    }
}