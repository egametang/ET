using System;

namespace ET
{
    public interface IMHandler
    {
        ETTask Handle(Entity entity, Address fromAddress, MessageObject actorMessage);
        Type GetRequestType();
        Type GetResponseType();
    }
}