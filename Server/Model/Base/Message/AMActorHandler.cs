﻿using System;
using System.Threading.Tasks;

namespace Model
{
	public abstract class AMActorHandler<E, Message>: IMActorHandler where E: Entity where Message : AMessage
	{
		protected abstract Task Run(E entity, Message message);

		public async Task Handle(Session session, Entity entity, ActorRequest message)
		{
			Message msg = message.AMessage as Message;
			if (msg == null)
			{
				Log.Error($"消息类型转换错误: {message.GetType().FullName} to {typeof (Message).Name}");
				return;
			}
			E e = entity as E;
			if (e == null)
			{
				Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof(E).Name}");
				return;
			}

			await this.Run(e, msg);

			// 等回调回来,session可以已经断开了,所以需要判断session id是否为0
			if (session.Id == 0)
			{
				return;
			}
			ActorResponse response = new ActorResponse
			{
				RpcId = message.RpcId
			};
			session.Reply(response);
		}

		public Type GetMessageType()
		{
			return typeof (Message);
		}
	}

	public abstract class AMActorRpcHandler<E, Request, Response>: IMActorHandler where E: Entity where Request : AActorRequest where Response : AActorResponse
	{
		protected static void ReplyError(Response response, Exception e, Action<Response> reply)
		{
			Log.Error(e.ToString());
			response.Error = ErrorCode.ERR_RpcFail;
			response.Message = e.ToString();
			reply(response);
		}

		protected abstract Task Run(E unit, Request message, Action<Response> reply);

		public async Task Handle(Session session, Entity entity, ActorRequest message)
		{
			try
			{
				Request request = message.AMessage as Request;
				if (request == null)
				{
					Log.Error($"消息类型转换错误: {message.GetType().FullName} to {typeof (Request).Name}");
					return;
				}
				E e = entity as E;
				if (e == null)
				{
					Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof(E).Name}");
					return;
				}
				await this.Run(e, request, response =>
				{
					// 等回调回来,session可以已经断开了,所以需要判断session id是否为0
					if (session.Id == 0)
					{
						return;
					}
					ActorRpcResponse actorResponse = new ActorRpcResponse
					{
						RpcId = message.RpcId,
						AMessage = response
					};
					session.Reply(actorResponse);
				});
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
			}
		}

		public Type GetMessageType()
		{
			return typeof (Request);
		}
	}
}