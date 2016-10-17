using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Base;

namespace Model
{
	[ObjectEvent]
	public class MessageComponentEvent : ObjectEvent<MessageComponent>, IAwake<AChannel>
	{
		public void Awake(AChannel aChannel)
		{
			this.GetValue().Awake(aChannel);
		}
	}
	
	/// <summary>
	/// 消息收发
	/// </summary>
	public class MessageComponent: Component
	{
		private static uint RpcId { get; set; }
		private readonly Dictionary<uint, Action<byte[], int, int>> requestCallback = new Dictionary<uint, Action<byte[], int, int>>();
		private readonly Dictionary<ushort, Action<byte[], int, int>> waitCallback = new Dictionary<ushort, Action<byte[], int, int>>();
		private AChannel channel;
		private MessageHandlerComponent messageHandler;
		
		public void Awake(AChannel aChannel)
		{
			this.messageHandler = Game.Scene.GetComponent<MessageHandlerComponent>();
			this.channel = aChannel;
			this.StartRecv();
		}

		public string RemoteAddress
		{
			get
			{
				return this.channel.RemoteAddress;
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
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
					continue;
				}

				if (messageBytes.Length < 6)
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
			uint flagUInt = BitConverter.ToUInt32(messageBytes, 2);
			bool isCompressed = (flagUInt & 0x80000000) > 0;
			if (isCompressed) // 最高位为1,表示有压缩,需要解压缩
			{
				messageBytes = ZipHelper.Decompress(messageBytes, 6, messageBytes.Length - 6);
				offset = 0;
			}
			else
			{
				offset = 6;
			}
			uint rpcId = flagUInt & 0x7fffffff;
			this.RunDecompressedBytes(opcode, rpcId, messageBytes, offset);
		}

		private void RunDecompressedBytes(ushort opcode, uint rpcId, byte[] messageBytes, int offset)
		{
			Action<byte[], int, int> action;
			if (this.requestCallback.TryGetValue(rpcId, out action))
			{
				this.requestCallback.Remove(rpcId);
				action(messageBytes, offset, messageBytes.Length - offset);
				return;
			}

			if (this.waitCallback.TryGetValue(opcode, out action))
			{
				this.waitCallback.Remove(opcode);
				action(messageBytes, offset, messageBytes.Length - offset);
				return;
			}

			this.messageHandler.Handle(this.Owner, opcode, messageBytes, offset);
		}


		public Task<Response> CallAsync<Response>(object request, CancellationToken cancellationToken) where Response : IErrorMessage
		{
			this.Send(++RpcId, request);

			var tcs = new TaskCompletionSource<Response>();

			this.requestCallback[RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					if (response.ErrorMessage.Errno != 0)
					{
						tcs.SetException(new RpcException(response.ErrorMessage.Errno, response.ErrorMessage.Message));
						return;
					}
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
		/// <typeparam name="Response"></typeparam>
		/// <param name="request"></param>
		/// <returns></returns>
		public Task<Response> CallAsync<Response>(object request) where Response : IErrorMessage
		{
			this.Send(++RpcId, request);
			var tcs = new TaskCompletionSource<Response>();
			this.requestCallback[RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					if (response.ErrorMessage.Errno != 0)
					{
						tcs.SetException(new RpcException(response.ErrorMessage.Errno,  response.ErrorMessage.Message));
						return;
					}
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};
			
			return tcs.Task;
		}

		/// <summary>
		/// 不发送消息,直接等待返回一个消息
		/// </summary>
		/// <typeparam name="Response"></typeparam>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<Response> WaitAsync<Response>(CancellationToken cancellationToken) where Response : class
		{
			var tcs = new TaskCompletionSource<Response>();
			ushort opcode = this.messageHandler.messageOpcode[typeof(Response)];
			this.waitCallback[opcode] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Wait Error: {typeof(Response).FullName}", e));
				}
			};

			cancellationToken.Register(() => { this.waitCallback.Remove(opcode); });

			return tcs.Task;
		}

		/// <summary>
		/// 不发送消息,直接等待返回一个消息
		/// </summary>
		/// <typeparam name="Response"></typeparam>
		/// <returns></returns>
		public Task<Response> WaitAsync<Response>() where Response : class
		{
			var tcs = new TaskCompletionSource<Response>();
			ushort opcode = this.messageHandler.messageOpcode[typeof(Response)];
			this.waitCallback[opcode] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Wait Error: {typeof(Response).FullName}", e));
				}
			};

			return tcs.Task;
		}

		public void Send(object message)
		{
			this.Send(0, message);
		}

		public void Send(uint rpcId, object message)
		{
			ushort opcode = this.messageHandler.GetOpcode(message.GetType());
			byte[] opcodeBytes = BitConverter.GetBytes(opcode);
			byte[] seqBytes = BitConverter.GetBytes(rpcId);
			byte[] messageBytes = MongoHelper.ToBson(message);
			
			if (channel == null)
			{
				throw new Exception("game channel not found!");
			}

			channel.Send(new List<byte[]> { opcodeBytes, seqBytes, messageBytes });
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			channel.Dispose();
		}
	}
}