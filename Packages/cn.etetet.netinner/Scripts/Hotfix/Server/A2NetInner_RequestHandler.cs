using System.Collections.Generic;

namespace ET.Server
{
    [MessageHandler(SceneType.NetInner)]
    public class A2NetInner_RequestHandler: MessageHandler<Scene, A2NetInner_Request, A2NetInner_Response>
    {
        protected override async ETTask Run(Scene root, A2NetInner_Request request, A2NetInner_Response response)
        {
            int rpcId = request.RpcId;
            IResponse res = await root.GetComponent<ProcessOuterSender>().Call(request.ActorId, request.MessageObject, false);
            res.RpcId = rpcId;
            response.MessageObject = res;
        }
    }
}