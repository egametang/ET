using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
    public struct ProcessActorId
    {
        public int Process;
        public long ActorId;

        public ProcessActorId(long actorId)
        {
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
            this.Process = instanceIdStruct.Process;
            instanceIdStruct.Process = Options.Instance.Process;
            this.ActorId = instanceIdStruct.ToLong();
        }
    }

    
    [ComponentOf(typeof(Scene))]
    public class NetInnerComponent: Entity, IAwake<IPEndPoint, int>, IAwake<int>, IDestroy
    {
        public AService Service;

        [StaticField]
        public static NetInnerComponent Instance;

        public int SessionStreamDispatcherType { get; set; }
    }
}