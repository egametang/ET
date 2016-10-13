using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Base
{
	public enum NetChannelType
	{
		Login,
		Gate,
		Battle,
	}

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
		private uint RpcId { get; set; }
		private readonly Dictionary<uint, Action<byte[], int, int>> requestCallback = new Dictionary<uint, Action<byte[], int, int>>();
		private readonly Dictionary<Opcode, Action<byte[], int, int>> waitCallback = new Dictionary<Opcode, Action<byte[], int, int>>();
		private AChannel channel;
		
		public void Awake(AChannel aChannel)
		{
			this.channel = aChannel;
			this.UpdateChannel();
		}
		
		private async void UpdateChannel()
		{
			while (true)
			{
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

				Opcode opcode = (Opcode)BitConverter.ToUInt16(messageBytes, 0);
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

		private void Run(Opcode opcode, byte[] messageBytes)
		{
			int offset = 0;
			uint flagUInt = BitConverter.ToUInt32(messageBytes, 2);
			bool isCompressed = (byte)(flagUInt >> 24) == 1;
			if (isCompressed) // 表示有压缩,需要解压缩
			{
				messageBytes = ZipHelper.Decompress(messageBytes, 6, messageBytes.Length - 6);
				offset = 0;
			}
			else
			{
				offset = 6;
			}
			uint rpcId = flagUInt & 0x0fff;
			this.RunDecompressedBytes(opcode, rpcId, messageBytes, offset);
		}

		private void RunDecompressedBytes(Opcode opcode, uint rpcId, byte[] messageBytes, int offset)
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

			Server.Scene.GetComponent<MessageHandlerComponent>().Handle(this.Owner, opcode, messageBytes, offset);
		}


		public Task<Response> CallAsync<Response>(object request, CancellationToken cancellationToken) where Response : IErrorMessage
		{
			this.Send(request, ++this.RpcId);

			var tcs = new TaskCompletionSource<Response>();

			this.requestCallback[this.RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					Opcode opcode = EnumHelper.FromString<Opcode>(response.GetType().Name);
					if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
					{
						Log.Debug(MongoHelper.ToJson(response));
					}
					if (response.ErrorMessage.errno != (int)ErrorCode.ERR_Success)
					{
						tcs.SetException(new RpcException((ErrorCode)response.ErrorMessage.errno, response.ErrorMessage.msg.Utf8ToStr()));
						return;
					}
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new GameException($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};

			cancellationToken.Register(() => { this.requestCallback.Remove(this.RpcId); });

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
			this.Send(request, ++this.RpcId);

			var tcs = new TaskCompletionSource<Response>();
			this.requestCallback[this.RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					Opcode opcode = EnumHelper.FromString<Opcode>(response.GetType().Name);
					if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
					{
						Log.Debug(MongoHelper.ToJson(response));
					}
					if (response.ErrorMessage.errno != (int) ErrorCode.ERR_Success)
					{
						tcs.SetException(new RpcException((ErrorCode)response.ErrorMessage.errno,  response.ErrorMessage.msg.Utf8ToStr()));
						return;
					}
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new GameException($"Rpc Error: {typeof(Response).FullName}", e));
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
			Opcode opcode = EnumHelper.FromString<Opcode>(typeof(Response).Name);

			this.waitCallback[opcode] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					Opcode op = EnumHelper.FromString<Opcode>(response.GetType().Name);
					if (OpcodeHelper.IsNeedDebugLogMessage(op))
					{
						Log.Debug(MongoHelper.ToJson(response));
					}
					
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new GameException($"Wait Error: {typeof(Response).FullName}", e));
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
			Opcode opcode = EnumHelper.FromString<Opcode>(typeof(Response).Name);
			this.waitCallback[opcode] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					Opcode op = EnumHelper.FromString<Opcode>(response.GetType().Name);
					if (OpcodeHelper.IsNeedDebugLogMessage(op))
					{
						Log.Debug(MongoHelper.ToJson(response));
					}
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new GameException($"Wait Error: {typeof(Response).FullName}", e));
				}
			};

			return tcs.Task;
		}

		public void Send(object message)
		{
			this.Send(message, 0);
		}

		private void Send(object message, uint rpcId)
		{
			Opcode opcode = EnumHelper.FromString<Opcode>(message.GetType().Name);
			byte[] opcodeBytes = BitConverter.GetBytes((ushort)opcode);
			byte[] seqBytes = BitConverter.GetBytes(rpcId);
			byte[] messageBytes = MongoHelper.ToBson(message);
			
			if (channel == null)
			{
				throw new GameException("game channel not found!");
			}

			channel.Send(new List<byte[]> { opcodeBytes, seqBytes, messageBytes });

			if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
			{
				Log.Debug(MongoHelper.ToJson(message));
			}
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