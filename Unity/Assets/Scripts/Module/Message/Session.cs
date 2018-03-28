using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

		private readonly Dictionary<int, Action<IResponse>> requestCallback = new Dictionary<int, Action<IResponse>>();
		private readonly List<byte[]> byteses = new List<byte[]>() { new byte[1], new byte[0], new byte[0]};

		public NetworkComponent Network
		{
			get
			{
				return this.GetParent<NetworkComponent>();
			}
		}

		public void Awake(NetworkComponent net, AChannel c)
		{
			this.channel = c;
			this.requestCallback.Clear();
		}

		public void Start()
		{
			this.StartRecv();
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
				action.Invoke(new ResponseMessage { Error = ErrorCode.ERR_SocketDisconnected });
			}

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

		private async void StartRecv()
		{
			while (true)
			{
				if (this.IsDisposed)
				{
					return;
				}

				Packet packet;
				try
				{
					packet = await this.channel.Recv();
					
					if (this.IsDisposed)
					{
						return;
					}
				}
				catch (Exception e)
				{
					Log.Error(e);
					continue;
				}
				
				try
				{
					this.Run(packet);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		private void Run(Packet packet)
		{
			if (packet.Length < Packet.MinSize)
			{
				Log.Error($"message error length < {Packet.MinSize}, ip: {this.RemoteAddress}");
				this.Network.Remove(this.Id);
				return;
			}

			byte flag = packet.Flag();
			ushort opcode = packet.Opcode();

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
			
			OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
			Type responseType = opcodeTypeComponent.GetType(opcode);
			object message = this.Network.MessagePacker.DeserializeFrom(responseType, packet.Bytes, Packet.Index, packet.Length - Packet.Index);
			//Log.Debug($"recv: {JsonHelper.ToJson(message)}");

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
				this.pkt.Length = 0;
				ushort index = 0;
				foreach (var byts in byteses)
				{
					Array.Copy(byts, 0, this.pkt.Bytes, index, byts.Length);
					index += (ushort)byts.Length;
				}

				this.pkt.Length = index;
				session.Run(this.pkt);
				return;
			}
#endif

			channel.Send(this.byteses);
		}

#if SERVER
		private Packet pkt = new Packet(ushort.MaxValue);
#endif
	}
}