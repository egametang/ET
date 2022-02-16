using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_TestActorLocationReqeustHandler : AMActorLocationRpcHandler<Unit,C2M_TestActorLocationReqeust,M2C_TestActorLocationResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TestActorLocationReqeust request, M2C_TestActorLocationResponse response, Action reply)
        {
            Log.Debug(response.Content);
            response.Content = "已经接收到了你的消息了,现在给你回复";
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}