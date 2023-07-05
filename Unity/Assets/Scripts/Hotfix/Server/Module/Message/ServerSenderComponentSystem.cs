using System;

namespace ET.Server
{

    public static class ServerSenderComponentSystem
    {
        public static void Send(this ServerSenderComponent self, ActorId actorId, IActorMessage message)
        {
            Fiber fiber = self.Fiber();
            if (actorId.Process == fiber.Process)
            {
                self.ActorInnerComponent.Send(actorId, message);
                return;
            }

            // 发给NetInner纤程
            A2NetInner_Message a2NetInnerMessage = A2NetInner_Message.Create();
            a2NetInnerMessage.FromAddress = fiber.Address;
            a2NetInnerMessage.ActorId = actorId;
            a2NetInnerMessage.MessageObject = message;

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.NetInners[fiber.Process];
            self.ActorInnerComponent.Send(startSceneConfig.ActorId, a2NetInnerMessage);
        }


        public static async ETTask<IActorResponse> Call(this ServerSenderComponent self, ActorId actorId, IActorRequest request, bool needException = true)
        {
            Fiber fiber = self.Fiber();
            if (actorId.Process == fiber.Process)
            {
                return await self.ActorInnerComponent.Call(actorId, request, needException);
            }

            // 发给NetInner纤程
            A2NetInner_Request a2NetInner_Request = A2NetInner_Request.Create();
            a2NetInner_Request.ActorId = actorId;
            a2NetInner_Request.MessageObject = request;
            a2NetInner_Request.FromAddress = fiber.Address;

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.NetInners[fiber.Process];
            A2NetInner_Response response = await self.ActorInnerComponent.Call(startSceneConfig.ActorId, a2NetInner_Request, needException: needException) as A2NetInner_Response;
            return response.MessageObject;
        }
    }
}

