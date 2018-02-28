using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Model
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
		private static uint RpcId { get; set; }
		private AChannel channel;

		private readonly Dictionary<uint, Action<PacketInfo>> requestCallback = new Dictionary<uint, Action<PacketInfo>>();

		private readonly byte[] flagBytes = new byte[1];
		private readonly List<byte[]> byteses = new List<byte[]>() {new byte[0], new byte[0], new byte[0]};
		private readonly List<byte[]> rpcByteses = new List<byte[]>() { new byte[0], new byte[0], new byte[0], new byte[0] };

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

			foreach (Action<PacketInfo> action in this.requestCallback.Values.ToArray())
			{
				action.Invoke(new PacketInfo());
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
					Log.Error(e.ToString());
					continue;
				}
				
				try
				{
					this.Run(packet);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
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
			PacketInfo packetInfo = new PacketInfo
			{
				Opcode = opcode,
				Bytes = packet.Bytes
			};
			
			if ((flag & 0xC0) > 0)
			{
				uint rpcId = packet.RpcId();
				packetInfo.RpcId = rpcId;
				packetInfo.Index = Packet.RpcIdIndex + 4;
				packetInfo.Length = (ushort)(packet.Length - packetInfo.Index);
				
				// flag第2位表示这是rpc返回消息
				if ((flag & 0x40) > 0)
				{
					Action<PacketInfo> action;
					if (!this.requestCallback.TryGetValue(rpcId, out action))
					{
						return;
					}
					this.requestCallback.Remove(rpcId);

					action(packetInfo);
					return;
				}
			}
			else
			{
				packetInfo.RpcId = 0;
				packetInfo.Index = Packet.RpcIdIndex;
				packetInfo.Length = (ushort)(packet.Length - packetInfo.Index);
			}
			
			this.Network.MessageDispatcher.Dispatch(this, packetInfo);
		}

		public Task<IResponse> Call(IRequest request)
		{
			uint rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<IResponse>();

			OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(request.GetType());
			byte[] bytes = this.Network.MessagePacker.SerializeToByteArray(request);

			this.requestCallback[rpcId] = (packetInfo) =>
			{
				try
				{
					Type responseType = opcodeTypeComponent.GetType(packetInfo.Opcode);
					object message = this.Network.MessagePacker.DeserializeFrom(responseType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
					IResponse response = (IResponse)message;
					if (response.Error > ErrorCode.ERR_Exception)
					{
						throw new RpcException(response.Error, response.Message);
					}

					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {packetInfo.Opcode}", e));
				}
			};

			const byte flag = 0x80;
			this.SendMessage(flag, opcode, rpcId, bytes);
			return tcs.Task;
		}

		public Task<IResponse> Call(IRequest request, CancellationToken cancellationToken)
		{
			uint rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<IResponse>();

			OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(request.GetType());
			byte[] bytes = this.Network.MessagePacker.SerializeToByteArray(request);

			this.requestCallback[rpcId] = (packetInfo) =>
			{
				try
				{
					Type responseType = opcodeTypeComponent.GetType(packetInfo.Opcode);
					object message = this.Network.MessagePacker.DeserializeFrom(responseType, packetInfo.Bytes, packetInfo.Index, packetInfo.Length);
					IResponse response = (IResponse)message;
					if (response.Error > ErrorCode.ERR_Exception)
					{
						throw new RpcException(response.Error, response.Message);
					}

					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {packetInfo.Opcode}", e));
				}
			};

			cancellationToken.Register(()=>this.requestCallback.Remove(rpcId));

			const byte flag = 0x80;
			this.SendMessage(flag, opcode, rpcId, bytes);
			return tcs.Task;
		}

		public Task<PacketInfo> Call(ushort opcode, byte[] bytes)
		{
			uint rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<PacketInfo>();
			this.requestCallback[rpcId] = (packetInfo) =>
			{
				try
				{
					// 抛到外层不能再使用之前的byte[],因为那是Packet所有的,为了减少gc一直传到这个位置
					byte[] newBytes = new byte[packetInfo.Length + packetInfo.Index];
					Array.Copy(packetInfo.Bytes, 0, newBytes, 0, newBytes.Length);
					packetInfo.Bytes = newBytes;
					tcs.SetResult(packetInfo);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {opcode}", e));
				}
			};

			const byte flag = 0x80;
			this.SendMessage(flag, opcode, rpcId, bytes);
			return tcs.Task;
		}

		public Task<PacketInfo> Call(ushort opcode, byte[] bytes, CancellationToken cancellationToken)
		{
			uint rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<PacketInfo>();
			this.requestCallback[rpcId] = (packetInfo) =>
			{
				try
				{
					// 抛到外层不能再使用之前的byte[],因为那是Packet所有的,为了减少gc一直传到这个位置
					byte[] newBytes = new byte[packetInfo.Length + packetInfo.Index];
					Array.Copy(packetInfo.Bytes, 0, newBytes, 0, newBytes.Length);
					packetInfo.Bytes = newBytes;
					tcs.SetResult(packetInfo);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {opcode}", e));
				}
			};

			cancellationToken.Register(() => { this.requestCallback.Remove(rpcId); });

			const byte flag = 0x80;
			this.SendMessage(flag, opcode, rpcId, bytes);
			return tcs.Task;
		}

		public void Send(IMessage message)
		{
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			byte[] bytes = this.Network.MessagePacker.SerializeToByteArray(message);
			this.Send(opcode, bytes);
		}

		public void Reply(uint rpcId, IResponse message)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}
			OpcodeTypeComponent opcodeTypeComponent = Game.Scene.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			byte[] bytes = this.Network.MessagePacker.SerializeToByteArray(message);
			const byte flag = 0x40;
			this.SendMessage(flag, opcode, rpcId, bytes);
		}

		public void Send(ushort opcode, byte[] bytes)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}
			const byte flag = 0x00;
			this.SendMessage(flag, opcode, 0, bytes);
		}

		private void SendMessage(byte flag, ushort opcode, uint rpcId, byte[] bytes)
		{
			this.flagBytes[0] = flag;

			List<byte[]> bb;
			if (rpcId == 0)
			{
				bb = this.byteses;
				bb[0] = flagBytes;
				bb[1] = BitConverter.GetBytes(opcode);
				bb[2] = bytes;
			}
			else
			{
				bb = this.rpcByteses;
				bb[0] = flagBytes;
				bb[1] = BitConverter.GetBytes(opcode);
				bb[2] = BitConverter.GetBytes(rpcId);
				bb[3] = bytes;
			}
			
#if SERVER
			// 如果是allserver，内部消息不走网络，直接转给session,方便调试时看到整体堆栈
			if (this.Network.AppType == AppType.AllServer)
			{
				Session session = this.Network.Entity.GetComponent<NetInnerComponent>().Get(this.RemoteAddress);
				this.pkt.Length = 0;
				ushort index = 0;
				foreach (var byts in bb)
				{
					Array.Copy(byts, 0, this.pkt.Bytes, index, byts.Length);
					index += (ushort)byts.Length;
				}

				this.pkt.Length = index;
				session.Run(this.pkt);
				return;
			}
#endif

			channel.Send(bb);
		}

#if SERVER
		private Packet pkt = new Packet(ushort.MaxValue);
#endif
	}
}