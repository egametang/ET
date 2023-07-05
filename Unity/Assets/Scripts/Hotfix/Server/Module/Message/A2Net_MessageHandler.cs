using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.NetInner)]
    public class A2Net_MessageHandler: ActorMessageHandler<Scene, A2NetInner_Message>
    {
        protected override async ETTask Run(Scene root, A2NetInner_Message innerMessage)
        {
            int process = innerMessage.ActorId.Process;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.NetInners[process];
            Session session = root.GetComponent<NetInnerComponent>().Get(startSceneConfig.Id);
            ActorId actorId = innerMessage.ActorId;
            actorId.Address = innerMessage.FromAddress;
            session.Send(actorId, innerMessage.MessageObject);
            await ETTask.CompletedTask;
        }
    }
}