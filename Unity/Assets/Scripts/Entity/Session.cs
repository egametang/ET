using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Model
{
	[ObjectSystem]
	public class SessionSystem : ObjectSystem<Session>, IAwake<NetworkComponent, AChannel>, IStart
	{
		public void Awake(NetworkComponent network, AChannel channel)
		{
			this.Get().Awake(network, channel);
		}

		public void Start()
		{
			this.Get().Start();
		}
	}

	public sealed class Session : Entity
	{
		private static uint RpcId { get; set; }
		private NetworkComponent network;
		private AChannel channel;

		private readonly Dictionary<uint, Action<object>> requestCallback = new Dictionary<uint, Action<object>>();
		private readonly List<byte[]> byteses = new List<byte[]>() {new byte[0], new byte[0]};
		
		public void Awake(NetworkComponent net, AChannel c)
		{
			this.network = net;
			this.channel = c;
			this.requestCallback.Clear();
		}

		public void Start()
		{
			this.StartRecv();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			long id = this.Id;

			base.Dispose();

			foreach (Action<object> action in this.requestCallback.Values.ToArray())
			{
				action.Invoke(new ErrorResponse() { Error = ErrorCode.ERR_SocketDisconnected });
			}

			this.channel.Dispose();
			this.network.Remove(id);
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
				if (this.Id == 0)
				{
					return;
				}

				Packet packet;
				try
				{
					packet = await this.channel.Recv();
					if (this.Id == 0)
					{
						return;
					}
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
					continue;
				}

				if (packet.Length < 2)
				{
					Log.Error($"message error length < 2, ip: {this.RemoteAddress}");
					this.network.Remove(this.Id);
					return;
				}

				ushort opcode = BitConverter.ToUInt16(packet.Bytes, 0);
				try
				{
					this.RunDecompressedBytes(opcode, packet.Bytes, 2, packet.Length);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		private void RunDecompressedBytes(ushort opcode, byte[] messageBytes, int offset, int count)
		{
			object message;
			Opcode op;

			try
			{
				op = (Opcode)opcode;
				Type messageType = this.network.Parent.GetComponent<OpcodeTypeComponent>().GetType(op);
				message = this.network.MessagePacker.DeserializeFrom(messageType, messageBytes, offset, count - offset);
			}
			catch (Exception e)
			{
				Log.Error($"message deserialize error, ip: {this.RemoteAddress} {opcode} {e}");
				this.network.Remove(this.Id);
				return;
			}

			//Log.Debug($"recv: {MongoHelper.ToJson(message)}");

			AResponse response = message as AResponse;
			if (response != null)
			{
				// rpcFlag>0 表示这是一个rpc响应消息
				// Rpc回调有找不着的可能，因为client可能取消Rpc调用
				Action<object> action;
				if (!this.requestCallback.TryGetValue(response.RpcId, out action))
				{
					return;
				}
				this.requestCallback.Remove(response.RpcId);
				action(message);
				return;
			}

			this.network.MessageDispatcher.Dispatch(this, op, offset, messageBytes, (AMessage)message);
		}

		/// <summary>
		/// Rpc调用,发送一个消息,等待返回一个消息
		/// </summary>
		public Task<AResponse> Call(ARequest request)
		{
			request.RpcId = ++RpcId;

			var tcs = new TaskCompletionSource<AResponse>();
			this.requestCallback[request.RpcId] = (message) =>
			{
				try
				{
					AResponse response = (AResponse)message;
					if (response.Error > 100)
					{
						tcs.SetException(new RpcException(response.Error, response.Message));
						return;
					}
					//Log.Debug($"recv: {MongoHelper.ToJson(response)}");
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {message.GetType().FullName}", e));
				}
			};

			this.SendMessage(request);
			return tcs.Task;
		}

		/// <summary>
		/// Rpc调用
		/// </summary>
		public Task<AResponse> Call(ARequest request, CancellationToken cancellationToken)
		{
			request.RpcId = ++RpcId;
			
			var tcs = new TaskCompletionSource<AResponse>();

			this.requestCallback[request.RpcId] = (message) =>
			{
				try
				{
					AResponse response = (AResponse)message;
					if (response.Error > 100)
					{
						tcs.SetException(new RpcException(response.Error, response.Message));
						return;
					}
					//Log.Debug($"recv: {MongoHelper.ToJson(response)}");
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {message.GetType().FullName}", e));
				}
			};

			cancellationToken.Register(() => { this.requestCallback.Remove(request.RpcId); });

			this.SendMessage(request);

			return tcs.Task;
		}

		public void Send(AMessage message)
		{
			if (this.Id == 0)
			{
				throw new Exception("session已经被Dispose了");
			}
			this.SendMessage(message);
		}

		public void Reply<Response>(Response message) where Response : AResponse
		{
			if (this.Id == 0)
			{
				throw new Exception("session已经被Dispose了");
			}
			this.SendMessage(message);
		}

		private void SendMessage(object message)
		{
			//Log.Debug($"send: {MongoHelper.ToJson(message)}");
			Opcode opcode = this.network.Parent.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
			ushort op = (ushort)opcode;
			byte[] messageBytes = this.network.MessagePacker.SerializeToByteArray(message);

#if SERVER
			// 如果是allserver，内部消息不走网络，直接转给session,方便调试时看到整体堆栈
			if (this.network.AppType == AppType.AllServer)
			{
				Session session = this.network.Parent.GetComponent<NetInnerComponent>().Get(this.RemoteAddress);
				session.RunDecompressedBytes(op, messageBytes, 0, messageBytes.Length);
				return;
			}
#endif

			byte[] opcodeBytes = BitConverter.GetBytes(op);
			
			this.byteses[0] = opcodeBytes;
			this.byteses[1] = messageBytes;

			channel.Send(this.byteses);
		}
	}
}