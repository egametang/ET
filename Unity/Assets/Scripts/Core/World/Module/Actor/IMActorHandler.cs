using System;

namespace ET
{
    public interface IMActorHandler
    {
        ETTask Handle(Entity entity, Address fromAddress, MessageObject actorMessage);
        Type GetRequestType();
        Type GetResponseType();
    }
}