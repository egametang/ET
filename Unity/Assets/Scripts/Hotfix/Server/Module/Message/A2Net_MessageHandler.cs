using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Net)]
    public class A2Net_MessageHandler: ActorMessageHandler<Scene, A2Net_Message>
    {
        protected override async ETTask Run(Scene root, A2Net_Message message)
        {
            int process = message.ActorId.Process;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Nets[process];
            Session session = root.GetComponent<NetInnerComponent>().Get(startSceneConfig.Id);
            ActorId actorId = message.ActorId;
            actorId.Address = message.FromAddress;
            session.Send(actorId, message.MessageObject);
            await ETTask.CompletedTask;
        }
    }
}