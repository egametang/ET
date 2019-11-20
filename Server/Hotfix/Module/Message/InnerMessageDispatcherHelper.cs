﻿using ETModel;

 namespace ETHotfix
{
    public static class InnerMessageDispatcherHelper
    {
        public static async ETVoid HandleIActorResponse(Session session, IActorResponse iActorResponse)
		{
			ActorMessageSenderComponent.Instance.RunMessage(iActorResponse);
		}

		public static async ETVoid HandleIActorRequest(Session session, IActorRequest iActorRequest)
		{
			long replyId = IdGenerater.GetProcessId(iActorRequest.ActorId);
			iActorRequest.ActorId = iActorRequest.ActorId & IdGenerater.HeadMask | IdGenerater.Head;

			string address = StartConfigComponent.Instance.GetProcessInnerAddress(replyId);
			Session ss = NetInnerComponent.Instance.Get(address);
			Entity entity = Game.EventSystem.Get(iActorRequest.ActorId);
			if (entity == null)
			{
				Log.Warning($"not found actor: {MongoHelper.ToJson(iActorRequest)}");
				ActorResponse response = new ActorResponse
				{
					Error = ErrorCode.ERR_NotFoundActor,
					RpcId = iActorRequest.RpcId,
				};
				ss.Reply(response);
				return;
			}
	
			MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
			if (mailBoxComponent == null)
			{
				ActorResponse response = new ActorResponse
				{
					Error = ErrorCode.ERR_ActorNoMailBoxComponent,
					RpcId = iActorRequest.RpcId,
				};
				ss.Reply(response);
				Log.Error($"actor not add MailBoxComponent: {entity.GetType().Name} {iActorRequest}");
				return;
			}
			
			await mailBoxComponent.Handle(ss, iActorRequest);
		}

		public static async ETVoid HandleIActorMessage(Session session, IActorMessage iActorMessage)
		{
			long replyId = IdGenerater.GetProcessId(iActorMessage.ActorId);
			iActorMessage.ActorId = iActorMessage.ActorId & IdGenerater.HeadMask | IdGenerater.Head;
			
			Entity entity = Game.EventSystem.Get(iActorMessage.ActorId);
			if (entity == null)
			{
				Log.Error($"not found actor: {MongoHelper.ToJson(iActorMessage)}");
				return;
			}
	
			MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
			if (mailBoxComponent == null)
			{
				Log.Error($"actor not add MailBoxComponent: {entity.GetType().Name} {iActorMessage}");
				return;
			}
			
			Session ss = NetInnerComponent.Instance.Get(replyId);
			await mailBoxComponent.Handle(ss, iActorMessage);
		}
    }
}