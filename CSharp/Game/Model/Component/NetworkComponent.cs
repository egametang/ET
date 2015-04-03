using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;
using Common.Helper;
using Common.Network;
using TNet;
using UNet;

namespace Model
{
	public enum RpcResponseStatus
	{
		Succee,
		Timeout,
		Exception,
	}

	public class RpcExcetionInfo
	{
		public int ErrorCode { get; private set; }
		public string ErrorInfo { get; private set; }

		public RpcExcetionInfo(int errorCode, string errorInfo)
		{
			this.ErrorCode = errorCode;
			this.ErrorInfo = errorInfo;
		}
	}

	public class NetworkComponent: Component<World>, IUpdate, IStart
	{
		private IService service;

		private int requestId;

		private readonly Dictionary<int, Action<byte[], RpcResponseStatus>> requestCallback =
				new Dictionary<int, Action<byte[], RpcResponseStatus>>();

		private void Accept(string host, int port, NetworkProtocol protocol = NetworkProtocol.TCP)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.service = new TService(host, port);
					break;
				case NetworkProtocol.UDP:
					this.service = new UService(host, port);
					break;
				default:
					throw new ArgumentOutOfRangeException("protocol");
			}

			this.AcceptChannel();
		}

		public void Start()
		{
			this.Accept(World.Instance.Options.Host, World.Instance.Options.Port,
					World.Instance.Options.Protocol);
		}

		public void Update()
		{
			this.service.Update();
		}

		/// <summary>
		/// 接收连接
		/// </summary>
		private async void AcceptChannel()
		{
			while (true)
			{
				AChannel channel = await this.service.GetChannel();
				this.ProcessChannel(channel);
			}
		}

		/// <summary>
		/// 接收分发封包
		/// </summary>
		/// <param name="channel"></param>
		private async void ProcessChannel(AChannel channel)
		{
			while (true)
			{
				byte[] message = await channel.RecvAsync();
				Env env = new Env();
				env[EnvKey.Channel] = channel;
				env[EnvKey.Message] = message;
				ushort opcode = BitConverter.ToUInt16(message, 0);
				env[EnvKey.Opcode] = opcode;

				// 表示消息是rpc响应消息
				if (opcode == Opcode.RpcResponse)
				{
					int id = BitConverter.ToInt32(message, 2);
					this.RequestCallback(channel, id, message, RpcResponseStatus.Succee);
					continue;
				}

				// rpc异常
				if (opcode == Opcode.RpcException)
				{
					int id = BitConverter.ToInt32(message, 2);
					this.RequestCallback(channel, id, message, RpcResponseStatus.Exception);
					continue;
				}

				// 如果是server message(发给client的消息),说明这是gate server,需要根据unitid查到channel,进行发送
				if (MessageTypeHelper.IsServerMessage(opcode))
				{
					World.Instance.GetComponent<EventComponent<EventAttribute>>()
							.RunAsync(EventType.GateRecvServerMessage, env);
					continue;
				}

				// 进行消息分发
				if (MessageTypeHelper.IsClientMessage(opcode))
				{
					World.Instance.GetComponent<EventComponent<EventAttribute>>()
							.RunAsync(EventType.LogicRecvClientMessage, env);
					continue;
				}

				if (MessageTypeHelper.IsRpcRequestMessage(opcode))
				{
					World.Instance.GetComponent<EventComponent<EventAttribute>>()
							.RunAsync(EventType.LogicRecvRpcMessage, env);
				}
			}
		}

		public void SendAsync(string address, byte[] buffer)
		{
			AChannel channel = this.service.GetChannel(address);
			channel.SendAsync(buffer);
		}

		public void SendAsync(string address, List<byte[]> buffers)
		{
			AChannel channel = this.service.GetChannel(address);
			channel.SendAsync(buffers);
		}

		// 消息回调或者超时回调
		public void RequestCallback(AChannel channel, int id, byte[] buffer, RpcResponseStatus responseStatus)
		{
			Action<byte[], RpcResponseStatus> action;
			if (!this.requestCallback.TryGetValue(id, out action))
			{
				return;
			}
			this.requestCallback.Remove(id);
			action(buffer, responseStatus);
		}

		/// <summary>
		/// Rpc请求
		/// </summary>
		public Task<T> RpcRequest<T, K>(string address, short type, K request, int waitTime = 0)
		{
			AChannel channel = this.service.GetChannel(address);

			++this.requestId;
			byte[] requestBuffer = MongoHelper.ToBson(request);
			byte[] typeBuffer = BitConverter.GetBytes(type);
			byte[] idBuffer = BitConverter.GetBytes(this.requestId);
			channel.SendAsync(new List<byte[]> { typeBuffer, idBuffer, requestBuffer });
			var tcs = new TaskCompletionSource<T>();
			this.requestCallback[this.requestId] = (e, b) =>
			{
				if (b == RpcResponseStatus.Timeout)
				{
					tcs.SetException(new Exception(
						string.Format("rpc timeout {0} {1}", type, MongoHelper.ToJson(request))));
					return;
				}
				if (b == RpcResponseStatus.Exception)
				{
					RpcExcetionInfo errorInfo = MongoHelper.FromBson<RpcExcetionInfo>(e, 8);
					tcs.SetException(new Exception(
						string.Format("rpc exception {0} {1} {2}", type, MongoHelper.ToJson(request), MongoHelper.ToJson(errorInfo))));
					return;
				}

				// RpcResponseStatus.Succee
				T response = MongoHelper.FromBson<T>(e, 6);
				tcs.SetResult(response);
			};

			if (waitTime > 0)
			{
				this.service.Timer.Add(TimeHelper.Now() + waitTime,
						() => { this.RequestCallback(channel, this.requestId, null, RpcResponseStatus.Timeout); });
			}
			return tcs.Task;
		}

		/// <summary>
		/// Rpc响应
		/// </summary>
		public void RpcResponse<T>(AChannel channel, int id, T response)
		{
			byte[] responseBuffer = MongoHelper.ToBson(response);
			byte[] typeBuffer = BitConverter.GetBytes(Opcode.RpcResponse);
			byte[] idBuffer = BitConverter.GetBytes(id);
			channel.SendAsync(new List<byte[]> { typeBuffer, idBuffer, responseBuffer });
		}

		/// <summary>
		/// Rpc响应
		/// </summary>
		public void RpcException(AChannel channel, int id, int errorCode, string errorInfo)
		{
			byte[] typeBuffer = BitConverter.GetBytes(Opcode.RpcException);
			byte[] idBuffer = BitConverter.GetBytes(id);
			RpcExcetionInfo info = new RpcExcetionInfo(errorCode, errorInfo);
			byte[] responseBuffer = MongoHelper.ToBson(info);
			channel.SendAsync(new List<byte[]> { typeBuffer, idBuffer, responseBuffer });
		}
	}
}