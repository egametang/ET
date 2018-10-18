using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
		private readonly List<byte[]> byteses = new List<byte[]>() { new byte[1], new byte[2] };

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

			long id = this.Id;

			base.Dispose();
			
			foreach (Action<IResponse> action in this.requestCallback.Values.ToArray())
			{
				action.Invoke(new ResponseMessage { Error = this.Error });
			}

			//int error = this.channel.Error;
			//if (this.channel.Error != 0)
			//{
			//	Log.Trace($"session dispose: {this.Id} ErrorCode: {error}, please see ErrorCode.cs!");
			//}
			
			this.channel.Dispose();
			this.Network.Remove(id);
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
			byte flag = memoryStream.GetBuffer()[Packet.FlagIndex];
			ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
			
#if !SERVER
			if (OpcodeHelper.IsClientHotfixMessage(opcode))
			{
				this.GetComponent<SessionCallbackComponent>().MessageCallback.Invoke(this, flag, opcode, memoryStream);
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
				Log.Error($"opcode: {opcode} {this.Network.Count} {e} ");
				this.Error = ErrorCode.ERR_PacketParserError;
				this.Network.Remove(this.Id);
				return;
			}

			// flag第一位为1表示这是rpc返回消息,否则交由MessageDispatcher分发
			if ((flag & 0x01) == 0)
			{
				this.Network.MessageDispatcher.Dispatch(this, opcode, message);
				return;
			}
				
			IResponse response = message as IResponse;
			if (response == null)
			{
				throw new Exception($"flag is response, but message is not! {opcode}");
			}
			Action<IResponse> action;
			if (!this.requestCallback.TryGetValue(response.RpcId, out action))
			{
				return;
			}
			this.requestCallback.Remove(response.RpcId);

			action(response);
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

					tcs.TrySetResult(response);
				}
				catch (Exception e)
				{
					tcs.TrySetException(new Exception($"Rpc Error: {request.GetType().FullName}", e));
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

			cancellationToken.Register(() => this.requestCallback.Remove(rpcId));

			request.RpcId = rpcId;
			this.Send(0x00, request);
			return tcs.Task;
		}

		public void Send(IMessage message)
		{
			this.Send(0x00, message);
		}

		public void Reply(IResponse message)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}

			this.Send(0x01, message);
		}

		public void Send(byte flag, IMessage message)
		{
			OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			
			Send(flag, opcode, message);
		}
		
		public void Send(byte flag, ushort opcode, object message)
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

			if (stream.Length > ushort.MaxValue)
			{
				Log.Error($"message too large: {stream.Length}, opcode: {opcode}");
				return;
			}
			
			this.byteses[0][0] = flag;
			this.byteses[1].WriteTo(0, opcode);
			int index = 0;
			foreach (var bytes in this.byteses)
			{
				Array.Copy(bytes, 0, stream.GetBuffer(), index, bytes.Length);
				index += bytes.Length;
			}

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