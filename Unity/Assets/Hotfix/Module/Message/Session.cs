using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class SessionAwakeSystem : AwakeSystem<Session, ETModel.Session>
	{
		public override void Awake(Session self, ETModel.Session session)
		{
			self.session = session;
			SessionCallbackComponent sessionComponent = self.session.AddComponent<SessionCallbackComponent>();
			sessionComponent.MessageCallback = (s, flag, opcode, memoryStream) => { self.Run(s, flag, opcode, memoryStream); };
			sessionComponent.DisposeCallback = s => { self.Dispose(); };
		}
	}

	/// <summary>
	/// 用来收发热更层的消息
	/// </summary>
	public class Session: Entity
	{
		public ETModel.Session session;

		private static int RpcId { get; set; }
		private readonly Dictionary<int, Action<IResponse>> requestCallback = new Dictionary<int, Action<IResponse>>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (Action<IResponse> action in this.requestCallback.Values.ToArray())
			{
				action.Invoke(new ResponseMessage { Error = this.session.Error });
			}

			this.requestCallback.Clear();

			this.session.Dispose();
		}

		public void Run(ETModel.Session s, byte flag, ushort opcode, MemoryStream memoryStream)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			object instance = opcodeTypeComponent.GetInstance(opcode);
			object message = this.session.Network.MessagePacker.DeserializeFrom(instance, memoryStream);

			if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
			{
				Log.Msg(message);
			}

			if ((flag & 0x01) > 0)
			{
				IResponse response = message as IResponse;
				if (response == null)
				{
					throw new Exception($"flag is response, but hotfix message is not! {opcode}");
				}
				
				Action<IResponse> action;
				if (!this.requestCallback.TryGetValue(response.RpcId, out action))
				{
					return;
				}
				this.requestCallback.Remove(response.RpcId);

				action(response);
				return;
			}

			Game.Scene.GetComponent<MessageDispatherComponent>().Handle(session, new MessageInfo(opcode, message));
		}

		public void Send(IMessage message)
		{
			Send(0x00, message);
		}

		public void Send(byte flag, IMessage message)
		{
			ushort opcode = Game.Scene.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
			this.Send(flag, opcode, message);
		}

		public void Send(byte flag, ushort opcode, IMessage message)
		{
			if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
			{
				Log.Msg(message);
			}
			session.Send(flag, opcode, message);
		}

		public ETTask<IResponse> Call(IRequest request)
		{
			int rpcId = ++RpcId;
			var tcs = new ETTaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				try
				{
					if (ErrorCode.IsRpcNeedThrowException(response.Error))
					{
						throw new RpcException(response.Error, response.Message);
					}

					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {request.GetType().FullName}", e));
				}
			};

			request.RpcId = rpcId;
			
			this.Send(0x00, request);
			return tcs.Task;
		}

		public ETTask<IResponse> Call(IRequest request, CancellationToken cancellationToken)
		{
			int rpcId = ++RpcId;
			var tcs = new ETTaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				try
				{
					if (ErrorCode.IsRpcNeedThrowException(response.Error))
					{
						throw new RpcException(response.Error, response.Message);
					}

					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {request.GetType().FullName}", e));
				}
			};

			cancellationToken.Register(() => { this.requestCallback.Remove(rpcId); });

			request.RpcId = rpcId;

			this.Send(0x00, request);
			return tcs.Task;
		}
	}
}
