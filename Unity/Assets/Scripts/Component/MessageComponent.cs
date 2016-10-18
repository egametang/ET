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
		private AChannel channel;
		private MessageDispatherComponent messageDispather;
		
		public void Awake(AChannel aChannel)
		{
			this.messageDispather = Game.Scene.GetComponent<MessageDispatherComponent>();
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
			uint flag = BitConverter.ToUInt32(messageBytes, 2);
			uint rpcFlag = flag & 0x40000000;
			uint rpcId = flag & 0x3fffffff;
			bool isCompressed = (flag & 0x80000000) > 0;
			if (isCompressed) // 最高位为1,表示有压缩,需要解压缩
			{
				messageBytes = ZipHelper.Decompress(messageBytes, 6, messageBytes.Length - 6);
				offset = 0;
			}
			else
			{
				offset = 6;
			}
			
			this.RunDecompressedBytes(opcode, rpcId, rpcFlag, messageBytes, offset);
		}

		private void RunDecompressedBytes(ushort opcode, uint rpcId, uint rpcFlag, byte[] messageBytes, int offset)
		{
			if (rpcId == 0)
			{
				this.messageDispather.Handle(this.Owner, opcode, messageBytes, offset);
			}

			// rpcFlag>0 表示这是一个rpc响应消息
			if (rpcFlag > 0)
			{
				Action<byte[], int, int> action;
				if (this.requestCallback.TryGetValue(rpcId, out action))
				{
					this.requestCallback.Remove(rpcId);
					action(messageBytes, offset, messageBytes.Length - offset);
				}
			}
			else // 这是一个rpc请求消息
			{
				this.messageDispather.HandleRpc(this.Owner, opcode, messageBytes, offset, rpcId);
			}
		}

		/// <summary>
		/// Rpc调用
		/// </summary>
		public Task<Response> Call<Request, Response>(Request request, CancellationToken cancellationToken) 
			where Request : ARequest
			where Response : AResponse
		{
			this.SendMessage(++RpcId, request);

			var tcs = new TaskCompletionSource<Response>();

			this.requestCallback[RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					if (response.ErrorCode != 0)
					{
						tcs.SetException(new RpcException(response.ErrorCode, response.Message));
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
		public Task<Response> Call<Request, Response>(Request request) 
			where Request: ARequest 
			where Response : AResponse
		{
			request.RpcId = ++RpcId;
			this.SendMessage(++RpcId, request);
			var tcs = new TaskCompletionSource<Response>();
			this.requestCallback[RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					if (response.ErrorCode != 0)
					{
						tcs.SetException(new RpcException(response.ErrorCode,  response.Message));
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

		public void Send(object message)
		{
			this.SendMessage(0, message);
		}

		public void Reply<T>(uint rpcId, T message) where T: AResponse
		{
			this.SendMessage(rpcId, message, false);
		}

		private void SendMessage(uint rpcId, object message, bool isCall = true)
		{
			ushort opcode = this.messageDispather.GetOpcode(message.GetType());
			byte[] opcodeBytes = BitConverter.GetBytes(opcode);
			if (rpcId > 0 && !isCall)
			{
				rpcId = rpcId | 0x4fffffff;
			}
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