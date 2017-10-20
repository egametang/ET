using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Model
{
	public sealed class Session : Entity
	{
		private static uint RpcId { get; set; }
		private readonly NetworkComponent network;
		private readonly Dictionary<uint, Action<object>> requestCallback = new Dictionary<uint, Action<object>>();
		private readonly AChannel channel;
		private readonly List<byte[]> byteses = new List<byte[]>() {new byte[0], new byte[0]};

		public Session(NetworkComponent network, AChannel channel)
		{
			this.network = network;
			this.channel = channel;
			this.StartRecv();
		}

		public string RemoteAddress
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

				byte[] messageBytes;
				try
				{
					messageBytes = await channel.Recv();
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

				if (messageBytes.Length < 3)
				{
					continue;
				}

				ushort opcode = BitConverter.ToUInt16(messageBytes, 0);
				try
				{
					this.Run(opcode, messageBytes);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		private void Run(ushort opcode, byte[] messageBytes)
		{
			int offset = 0;
			// opcode最高位表示是否压缩
			bool isCompressed = (opcode & 0x8000) > 0;
			if (isCompressed) // 最高位为1,表示有压缩,需要解压缩
			{
				messageBytes = ZipHelper.Decompress(messageBytes, 2, messageBytes.Length - 2);
				offset = 0;
			}
			else
			{
				offset = 2;
			}
			opcode &= 0x7fff;
			this.RunDecompressedBytes(opcode, messageBytes, offset);
		}

		private void RunDecompressedBytes(ushort opcode, byte[] messageBytes, int offset)
		{
			Type messageType = this.network.Entity.GetComponent<OpcodeTypeComponent>().GetType(opcode);
			object message = this.network.MessagePacker.DeserializeFrom(messageType, messageBytes, offset, messageBytes.Length - offset);

			//Log.Debug($"recv: {JsonHelper.ToJson(message)}");

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

			this.network.MessageDispatcher.Dispatch(this, opcode, offset, messageBytes, (AMessage)message);
		}

		/// <summary>
		/// Rpc调用
		/// </summary>
		public Task<Response> Call<Response>(ARequest request, CancellationToken cancellationToken)
			where Response : AResponse
		{
			request.RpcId = ++RpcId;
			this.SendMessage(request);

			var tcs = new TaskCompletionSource<Response>();

			this.requestCallback[RpcId] = (message) =>
			{
				try
				{
					Response response = (Response)message;
					if (response.Error > 100)
					{
						tcs.SetException(new RpcException(response.Error, response.Message));
						return;
					}
					Log.Debug($"recv: {MongoHelper.ToJson(response)}");
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};

			cancellationToken.Register(() => { this.requestCallback.Remove(RpcId); });

			return tcs.Task;
		}

		/// <summary>
		/// Rpc调用,发送一个消息,等待返回一个消息
		/// </summary>
		public Task<Response> Call<Response>(ARequest request) where Response : AResponse
		{
			request.RpcId = ++RpcId;
			this.SendMessage(request);

			var tcs = new TaskCompletionSource<Response>();
			this.requestCallback[RpcId] = (message) =>
			{
				try
				{
					Response response = (Response)message;
					if (response.Error > 100)
					{
						tcs.SetException(new RpcException(response.Error, response.Message));
						return;
					}
					Log.Debug($"recv: {MongoHelper.ToJson(response)}");
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};

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
			Log.Debug($"send: {MongoHelper.ToJson(message)}");
			ushort opcode = this.network.Entity.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());

			byte[] messageBytes = this.network.MessagePacker.SerializeToByteArray(message);
			if (messageBytes.Length > 100)
			{
				byte[] newMessageBytes = ZipHelper.Compress(messageBytes);
				if (newMessageBytes.Length < messageBytes.Length)
				{
					messageBytes = newMessageBytes;
					opcode |= 0x8000;
				}
			}

			byte[] opcodeBytes = BitConverter.GetBytes(opcode);
			
			this.byteses[0] = opcodeBytes;
			this.byteses[1] = messageBytes;
			channel.Send(this.byteses);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			long id = this.Id;

			base.Dispose();

			this.channel.Dispose();
			this.network.Remove(id);
		}
	}
}