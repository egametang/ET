using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class SessionAwakeSystem : AwakeSystem<Session, AChannel>
	{
		public override void Awake(Session self, AChannel b)
		{
			self.Awake(b);
		}
	}

	public sealed class Session : Entity
	{
		private static int RpcId { get; set; }
		private AChannel channel;

		private readonly Dictionary<int, Action<IResponse>> requestCallback = new Dictionary<int, Action<IResponse>>();
		private readonly byte[] opcodeBytes = new byte[2];

		public NetworkComponent Network
		{
			get
			{
				return this.GetParent<NetworkComponent>();
			}
		}

		public int Error
		{
			get
			{
				return this.channel.Error;
			}
			set
			{
				this.channel.Error = value;
			}
		}

		public void Awake(AChannel aChannel)
		{
			this.channel = aChannel;
			this.requestCallback.Clear();
			long id = this.Id;
			channel.ErrorCallback += (c, e) =>
			{
				this.Network.Remove(id); 
			};
			channel.ReadCallback += this.OnRead;
		}
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			this.Network.Remove(this.Id);

			base.Dispose();
			
			foreach (Action<IResponse> action in this.requestCallback.Values.ToArray())
			{
				action.Invoke(new ErrorResponse { Error = this.Error });
			}

			//int error = this.channel.Error;
			//if (this.channel.Error != 0)
			//{
			//	Log.Trace($"session dispose: {this.Id} ErrorCode: {error}, please see ErrorCode.cs!");
			//}
			
			this.channel.Dispose();
			
			this.requestCallback.Clear();
		}

		public void Start()
		{
			this.channel.Start();
		}

		public IPEndPoint RemoteAddress
		{
			get
			{
				return this.channel.RemoteAddress;
			}
		}

		public ChannelType ChannelType
		{
			get
			{
				return this.channel.ChannelType;
			}
		}

		public MemoryStream Stream
		{
			get
			{
				return this.channel.Stream;
			}
		}

		public void OnRead(MemoryStream memoryStream)
		{
			try
			{
				this.Run(memoryStream);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private void Run(MemoryStream memoryStream)
		{
			memoryStream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
			ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
			
#if !SERVER
			if (OpcodeHelper.IsClientHotfixMessage(opcode))
			{
				this.GetComponent<SessionCallbackComponent>().MessageCallback.Invoke(this, opcode, memoryStream);
				return;
			}
#endif
			
			object message;
			try
			{
				OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
				object instance = opcodeTypeComponent.GetInstance(opcode);
				message = this.Network.MessagePacker.DeserializeFrom(instance, memoryStream);
				
				if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
				{
					Log.Msg(message);
				}
			}
			catch (Exception e)
			{
				// 出现任何消息解析异常都要断开Session，防止客户端伪造消息
				Log.Error($"opcode: {opcode} {this.Network.Count} {e}, ip: {this.RemoteAddress}");
				this.Error = ErrorCode.ERR_PacketParserError;
				this.Network.Remove(this.Id);
				return;
			}

			if (!(message is IResponse response))
			{
				this.Network.MessageDispatcher.Dispatch(this, opcode, message);
				return;
			}
			
			Action<IResponse> action;
			if (!this.requestCallback.TryGetValue(response.RpcId, out action))
			{
				throw new Exception($"not found rpc, response message: {StringHelper.MessageToStr(response)}");
			}
			this.requestCallback.Remove(response.RpcId);

			action(response);
		}
		
		public ETTask<IResponse> CallWithoutException(IRequest request)
		{
			int rpcId = ++RpcId;
			var tcs = new ETTaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				if (response is ErrorResponse errorResponse)
				{
					tcs.SetException(new Exception($"session close, errorcode: {errorResponse.Error} {errorResponse.Message}"));
					return;
				}
				tcs.SetResult(response);
			};

			request.RpcId = rpcId;
			this.Send(request);
			return tcs.Task;
		}
		
		public ETTask<IResponse> CallWithoutException(IRequest request, CancellationToken cancellationToken)
		{
			int rpcId = ++RpcId;
			var tcs = new ETTaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				if (response is ErrorResponse errorResponse)
				{
					tcs.SetException(new Exception($"session close, errorcode: {errorResponse.Error} {errorResponse.Message}"));
					return;
				}
				tcs.SetResult(response);
			};

			cancellationToken.Register(() => this.requestCallback.Remove(rpcId));

			request.RpcId = rpcId;
			this.Send(request);
			return tcs.Task;
		}

		public ETTask<IResponse> Call(IRequest request)
		{
			int rpcId = ++RpcId;
			var tcs = new ETTaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				if (ErrorCode.IsRpcNeedThrowException(response.Error))
				{
					tcs.SetException(new Exception($"Rpc Error: {request.GetType().FullName} {response.Error}"));
					return;
				}

				tcs.SetResult(response);
			};

			request.RpcId = rpcId;
			this.Send(request);
			return tcs.Task;
		}

		public ETTask<IResponse> Call(IRequest request, CancellationToken cancellationToken)
		{
			int rpcId = ++RpcId;
			var tcs = new ETTaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				if (ErrorCode.IsRpcNeedThrowException(response.Error))
				{
					tcs.SetException(new Exception($"Rpc Error: {request.GetType().FullName} {response.Error}"));
				}

				tcs.SetResult(response);
			};

			cancellationToken.Register(() => this.requestCallback.Remove(rpcId));

			request.RpcId = rpcId;
			this.Send(request);
			return tcs.Task;
		}

		public void Reply(IResponse message)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}

			this.Send(message);
		}

		public void Send(IMessage message)
		{
			OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			
			Send(opcode, message);
		}
		
		public void Send(ushort opcode, object message)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}
			
			if (OpcodeHelper.IsNeedDebugLogMessage(opcode) )
			{
#if !SERVER
				if (OpcodeHelper.IsClientHotfixMessage(opcode))
				{
				}
				else
#endif
				{
					Log.Msg(message);
				}
			}

			MemoryStream stream = this.Stream;
			
			stream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
			stream.SetLength(Packet.MessageIndex);
			this.Network.MessagePacker.SerializeTo(message, stream);
			stream.Seek(0, SeekOrigin.Begin);
			
			opcodeBytes.WriteTo(0, opcode);
			Array.Copy(opcodeBytes, 0, stream.GetBuffer(), 0, opcodeBytes.Length);

#if SERVER
			// 如果是allserver，内部消息不走网络，直接转给session,方便调试时看到整体堆栈
			if (this.Network.AppType == AppType.AllServer)
			{
				Session session = this.Network.Entity.GetComponent<NetInnerComponent>().Get(this.RemoteAddress);
				session.Run(stream);
				return;
			}
#endif

			this.Send(stream);
		}

		public void Send(MemoryStream stream)
		{
			channel.Send(stream);
		}
	}
}