using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ETModel
{
	[ObjectSystem]
	public class SessionAwakeSystem : AwakeSystem<Session, NetworkComponent, AChannel>
	{
		public override void Awake(Session self, NetworkComponent a, AChannel b)
		{
			self.Awake(a, b);
		}
	}

	[ObjectSystem]
	public class SessionStartSystem : StartSystem<Session>
	{
		public override void Start(Session self)
		{
			self.Start();
		}
	}

	public sealed class Session : Entity
	{
		private static int RpcId { get; set; }
		private AChannel channel;
		public int Error;

		private readonly Dictionary<int, Action<IResponse>> requestCallback = new Dictionary<int, Action<IResponse>>();
		private readonly List<byte[]> byteses = new List<byte[]>() { new byte[1], new byte[0], new byte[0]};

		public NetworkComponent Network
		{
			get
			{
				return this.GetParent<NetworkComponent>();
			}
		}

		public void Awake(NetworkComponent net, AChannel aChannel)
		{
			this.Error = 0;
			this.channel = aChannel;
			this.requestCallback.Clear();
			
			channel.ErrorCallback += (c, e) =>
			{
				this.Error = e;
				this.Network.Remove(this.Id); 
			};
			channel.ReadCallback += this.OnRead;
		}

		public void Start()
		{
			this.channel?.Start();
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
				action.Invoke(new ResponseMessage { Error = ErrorCode.ERR_SessionDispose });
			}

			this.Error = 0;
			this.channel.Dispose();
			this.Network.Remove(id);
			this.requestCallback.Clear();
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

		public void OnRead(Packet packet)
		{
			try
			{
				this.Run(packet);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private void Run(Packet packet)
		{
			byte flag = packet.Flag;
			ushort opcode = packet.Opcode;
			
#if !SERVER
			if (OpcodeHelper.IsClientHotfixMessage(opcode))
			{
				this.Network.MessageDispatcher.Dispatch(this, packet);
				return;
			}
#endif

			// flag第一位为1表示这是rpc返回消息,否则交由MessageDispatcher分发
			if ((flag & 0x01) == 0)
			{
				this.Network.MessageDispatcher.Dispatch(this, packet);
				return;
			}

			object message;
			try
			{
				OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
				Type responseType = opcodeTypeComponent.GetType(opcode);
				message = this.Network.MessagePacker.DeserializeFrom(responseType, packet.Bytes, packet.Offset, packet.Length);
				//Log.Debug($"recv: {JsonHelper.ToJson(message)}");
			}
			catch (Exception e)
			{
				// 出现任何消息解析异常都要断开Session，防止客户端伪造消息
				Log.Error(e);
				this.Error = ErrorCode.ERR_PacketParserError;
				this.Network.Remove(this.Id);
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

		public Task<IResponse> Call(IRequest request)
		{
			int rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				try
				{
					if (response.Error > ErrorCode.ERR_Exception)
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

		public Task<IResponse> Call(IRequest request, CancellationToken cancellationToken)
		{
			int rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				try
				{
					if (response.Error > ErrorCode.ERR_Exception)
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
			byte[] bytes = this.Network.MessagePacker.SerializeToByteArray(message);

			Send(flag, opcode, bytes);
		}

		public void Send(byte flag, ushort opcode, byte[] bytes)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}
			this.byteses[0][0] = flag;
			this.byteses[1] = BitConverter.GetBytes(opcode);
			this.byteses[2] = bytes;

#if SERVER
			// 如果是allserver，内部消息不走网络，直接转给session,方便调试时看到整体堆栈
			if (this.Network.AppType == AppType.AllServer)
			{
				Session session = this.Network.Entity.GetComponent<NetInnerComponent>().Get(this.RemoteAddress);

				Packet packet = ((TChannel)this.channel).parser.packet;

				Array.Copy(bytes, 0, packet.Bytes, 0, bytes.Length);

				packet.Offset = 0;
				packet.Length = (ushort)bytes.Length;
				packet.Flag = flag;
				packet.Opcode = opcode;
				session.Run(packet);
				return;
			}
#endif

			channel.Send(this.byteses);
		}
	}
}