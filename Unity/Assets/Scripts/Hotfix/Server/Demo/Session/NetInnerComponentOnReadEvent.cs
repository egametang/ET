using System;

namespace ET.Server
{
    [Event(SceneType.Process)]
    public class NetInnerComponentOnReadEvent: AEvent<Scene, NetInnerComponentOnRead>
    {
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args)
        {
            await ETTask.CompletedTask;
            try
            {
                ActorId actorId = args.ActorId;
                object message = args.Message;
                
                if (message is IActorResponse iActorResponse)
                {
                    ActorMessageSenderComponent.Instance.HandleIActorResponse(iActorResponse);
                    return;
                }
                
                
                // 收到actor消息,放入actor队列
                switch (message)
                {
                    case IActorRequest iActorRequest:
                    {
                        //await ActorMessageDispatcherComponent.Instance.HandleIActorRequest(actorId, iActorRequest);
                        break;
                    }
                    case IActorMessage iActorMessage:
                    {
                        //await ActorMessageDispatcherComponent.Instance.HandleIActorMessage(actorId, iActorMessage);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"InnerMessageDispatcher error: {args.Message.GetType().FullName}\n{e}");
            }
        }
    }
}