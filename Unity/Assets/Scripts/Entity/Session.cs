using System;
using System.Collections.Generic;
using System.Net;
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

				if (messageBytes.Length < 2)
				{
					Log.Error($"message error length < 2, ip: {this.RemoteAddress}");
					this.network.Remove(this.Id);
					return;
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
			this.RunDecompressedBytes(opcode, messageBytes, 2);
		}

		private void RunDecompressedBytes(ushort opcode, byte[] messageBytes, int offset)
		{
			object message;
			Opcode op;

			try
			{
				op = (Opcode)opcode;
				Type messageType = this.network.Entity.GetComponent<OpcodeTypeComponent>().GetType(op);
				message = this.network.MessagePacker.DeserializeFrom(messageType, messageBytes, offset, messageBytes.Length - offset);
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
		/// Rpc调用
		/// </summary>
		public void CallWithAction(ARequest request, Action<AResponse> action)
		{
			request.RpcId = ++RpcId;

			this.requestCallback[request.RpcId] = (message) =>
			{
				try
				{
					AResponse response = (AResponse)message;
					action(response);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			};
			
			this.SendMessage(request);
		}

		/// <summary>
		/// Rpc调用,发送一个消息,等待返回一个消息
		/// </summary>
		public Task<AResponse> Call(ARequest request, bool isHotfix)
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
		public Task<AResponse> Call(ARequest request, bool isHotfix, CancellationToken cancellationToken)
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

		/// <summary>
		/// Rpc调用,发送一个消息,等待返回一个消息
		/// </summary>
		public Task<Response> Call<Response>(ARequest request) where Response : AResponse
		{
			request.RpcId = ++RpcId;
			
			var tcs = new TaskCompletionSource<Response>();
			this.requestCallback[request.RpcId] = (message) =>
			{
				try
				{
					Response response = (Response)message;
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
					tcs.SetException(new Exception($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};

			this.SendMessage(request);

			return tcs.Task;
		}

		/// <summary>
		/// Rpc调用
		/// </summary>
		public Task<Response> Call<Response>(ARequest request, CancellationToken cancellationToken)
			where Response : AResponse
		{
			request.RpcId = ++RpcId;
			
			var tcs = new TaskCompletionSource<Response>();

			this.requestCallback[request.RpcId] = (message) =>
			{
				try
				{
					Response response = (Response)message;
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
					tcs.SetException(new Exception($"Rpc Error: {typeof(Response).FullName}", e));
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
			Opcode opcode = this.network.GetComponent<OpcodeTypeComponent>().GetOpcode(message.GetType());
			ushort op = (ushort)opcode;
			byte[] messageBytes = this.network.MessagePacker.SerializeToByteArray(message);

#if SERVER
			// 如果是allserver，内部消息不走网络，直接转给session,方便调试时看到整体堆栈
			if (this.network.AppType == AppType.AllServer)
			{
				Session session = this.network.GetComponent<NetInnerComponent>().Get(this.RemoteAddress.ToString());
				session.RunDecompressedBytes(op, messageBytes, 0);
				return;
			}
#endif

			byte[] opcodeBytes = BitConverter.GetBytes(op);
			
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