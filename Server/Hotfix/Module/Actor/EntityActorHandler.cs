using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    /// gate session收到的消息直接转发给客户端
    /// </summary>
    public class GateSessionEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, ActorRequest actorRequest)
        {
			ActorResponse actorResponse = new ActorResponse();
			try
	        {
		        OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
		        Type type = opcodeTypeComponent.GetType(actorRequest.Op);
		        IMessage message = (IMessage)session.Network.MessagePacker.DeserializeFrom(type, actorRequest.AMessage);

				// 发送给客户端
				Session clientSession = entity as Session;
		        clientSession.Send(actorResponse.Flag, message);

				actorResponse.RpcId = actorRequest.RpcId;
		        session.Reply(actorResponse);
		        await Task.CompletedTask;
	        }
	        catch (Exception e)
	        {
		        actorResponse.Error = ErrorCode.ERR_SessionActorError;
		        actorResponse.Message = $"session actor error {e}";
		        actorResponse.RpcId = actorRequest.RpcId;
				session.Reply(actorResponse);
				throw;
	        }
        }
    }

    public class CommonEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, ActorRequest actorRequest)
        {
	        OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
	        Type type = opcodeTypeComponent.GetType(actorRequest.Op);
	        IMessage message = (IMessage)session.Network.MessagePacker.DeserializeFrom(type, actorRequest.AMessage);
			await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, actorRequest, message);
        }
    }

    /// <summary>
    /// 玩家收到帧同步消息交给帧同步组件处理
    /// </summary>
    public class MapUnitEntityActorHandler : IEntityActorHandler
    {
        public async Task Handle(Session session, Entity entity, ActorRequest actorRequest)
        {
	        OpcodeTypeComponent opcodeTypeComponent = session.Network.Entity.GetComponent<OpcodeTypeComponent>();
	        Type type = opcodeTypeComponent.GetType(actorRequest.Op);
	        IMessage message = (IMessage)session.Network.MessagePacker.DeserializeFrom(type, actorRequest.AMessage);

			if (message is OneFrameMessage aFrameMessage)
            {
				Game.Scene.GetComponent<ServerFrameComponent>().Add(aFrameMessage);

	            ActorResponse actorResponse = new ActorResponse();
	            actorResponse.RpcId = actorRequest.RpcId;
	            session.Reply(actorResponse);
				return;
            }
            await Game.Scene.GetComponent<ActorMessageDispatherComponent>().Handle(session, entity, actorRequest, message);
        }
    }
}