using System;

namespace ET.Server
{
    public interface IMActorHandler
    {
        ETTask Handle(Entity entity, object actorMessage, Action<IActorResponse> reply);
        Type GetRequestType();
        Type GetResponseType();
    }
}