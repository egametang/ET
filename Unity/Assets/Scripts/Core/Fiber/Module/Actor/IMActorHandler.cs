using System;

namespace ET
{
    public interface IMActorHandler
    {
        ETTask Handle(Entity entity, ActorId actorId, object actorMessage);
        Type GetRequestType();
        Type GetResponseType();
    }
}