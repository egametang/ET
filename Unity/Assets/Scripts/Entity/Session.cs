using System;
using System.Collections.Generic;
using System.IO;
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
		private bool isRpc;

		private readonly IMessagePacker messagePacker;

		public Session(NetworkComponent network, AChannel channel, IMessagePacker messagePacker)
		{
			this.network = network;
			this.channel = channel;
			this.messagePacker = messagePacker;
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
			TimerComponent timerComponent = Game.Scene.GetComponent<TimerComponent>();
			while (true)
			{
				if (this.Id == 0)
				{
					return;
				}

				byte[] messageBytes;
				try
				{
					if (this.isRpc)
					{
						this.isRpc = false;
						await timerComponent.WaitAsync(0);
					}
					messageBytes = await channel.Recv();
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
			byte flag = messageBytes[2];

			bool isCompressed = (flag & 0x80) > 0;
			const int opcodeAndFlagLength = 3;
			if (isCompressed) // 最高位为1,表示有压缩,需要解压缩
			{
				messageBytes = ZipHelper.Decompress(messageBytes, opcodeAndFlagLength, messageBytes.Length - opcodeAndFlagLength);
				offset = 0;
			}
			else
			{
				offset = opcodeAndFlagLength;
			}

			this.RunDecompressedBytes(opcode, messageBytes, offset);
		}

		private void RunDecompressedBytes(ushort opcode, byte[] messageBytes, int offset)
		{
			Type messageType = this.network.Owner.GetComponent<OpcodeTypeComponent>().GetType(opcode);
			object message = messagePacker.DeserializeFrom(messageType, messageBytes, offset, messageBytes.Length - offset);
			

			// 普通消息或者是Rpc请求消息
			if (message is AMessage || message is ARequest)
			{
				MessageInfo messageInfo = new MessageInfo(opcode, message);
				Game.Scene.GetComponent<CrossComponent>().Run(CrossIdType.MessageDeserializeFinish, messageInfo);
				return;
			}

			AResponse response = message as AResponse;
			Log.Debug($"aaaaaaaaaaaaaaaaaaaaaaaaaaa {JsonHelper.ToJson(response)}");
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

			throw new Exception($"message type error: {message.GetType().FullName}");
		}

		/// <summary>
		/// Rpc调用
		/// </summary>
		public Task<Response> Call<Request, Response>(Request request, CancellationToken cancellationToken) where Request : ARequest
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
					if (response.Error != 0)
					{
						tcs.SetException(new RpcException(response.Error, response.Message));
						return;
					}
					this.isRpc = true;
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
		public Task<Response> Call<Request, Response>(Request request) where Request : ARequest where Response : AResponse
		{
			request.RpcId = ++RpcId;
			this.SendMessage(request);

			var tcs = new TaskCompletionSource<Response>();
			this.requestCallback[RpcId] = (message) =>
			{
				try
				{
					Response response = (Response)message;
					if (response.Error != 0)
					{
						tcs.SetException(new RpcException(response.Error, response.Message));
						return;
					}
					this.isRpc = true;
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};

			return tcs.Task;
		}

		public void Send<Message>(Message message) where Message : AMessage
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
			ushort opcode = this.network.Owner.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
			byte[] opcodeBytes = BitConverter.GetBytes(opcode);

			byte[] messageBytes = messagePacker.SerializeToByteArray(message);
			byte flag = 0;
			if (messageBytes.Length > 100)
			{
				byte[] newMessageBytes = ZipHelper.Compress(messageBytes);
				if (newMessageBytes.Length < messageBytes.Length)
				{
					messageBytes = newMessageBytes;
					flag |= 0x80;
				}
			}

			byte[] flagBytes = { flag };

			channel.Send(new List<byte[]> { opcodeBytes, flagBytes, messageBytes });
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