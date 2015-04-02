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
	public enum RpcStatus
	{
		OK,
		Timeout,
		Exception,
	}

	public class RpcExcetionInfo
	{
		public int ErrorCode { get; set; }
		public string ErrorInfo { get; set; }
	}

	public class NetworkComponent: Component<World>, IUpdate, IStart
	{
		private IService service;

		private int requestId;

		private readonly Dictionary<int, Action<byte[], RpcStatus>> requestCallback =
				new Dictionary<int, Action<byte[], RpcStatus>>();

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
				int opcode = BitConverter.ToUInt16(message, 0);

				// 表示消息是rpc响应消息
				if (opcode == 0)
				{
					int id = BitConverter.ToInt32(message, 2);
					this.RequestCallback(channel, id, message, RpcStatus.OK);
					continue;
				}

				// 如果是发给client的消息,说明这是gate server,需要根据unitid查到channel,进行发送
				if (MessageTypeHelper.IsServerMessage(opcode))
				{
					World.Instance.GetComponent<EventComponent<EventAttribute>>()
							.Run(EventType.GateRecvServerMessage, env);
					continue;
				}

				// 进行消息分发
				World.Instance.GetComponent<EventComponent<EventAttribute>>()
						.Run(EventType.LogicRecvMessage, env);
			}
		}

		public void SendAsync(string address, byte[] buffer)
		{
			AChannel channel = this.service.GetChannel(address);
			channel.SendAsync(buffer);
		}

		// 消息回调或者超时回调
		public void RequestCallback(AChannel channel, int id, byte[] buffer, RpcStatus status)
		{
			Action<byte[], RpcStatus> action;
			if (!this.requestCallback.TryGetValue(id, out action))
			{
				return;
			}
			action(buffer, status);
			this.requestCallback.Remove(id);
		}

		/// <summary>
		/// Rpc请求
		/// </summary>
		public Task<T> Request<T, K>(AChannel channel, short type, K request, int waitTime = 0)
		{
			++this.requestId;
			byte[] requestBuffer = MongoHelper.ToBson(request);
			byte[] typeBuffer = BitConverter.GetBytes(type);
			byte[] idBuffer = BitConverter.GetBytes(this.requestId);
			channel.SendAsync(new List<byte[]> { typeBuffer, idBuffer, requestBuffer });
			var tcs = new TaskCompletionSource<T>();
			this.requestCallback[this.requestId] = (e, b) =>
			{
				if (b == RpcStatus.Timeout)
				{
					tcs.SetException(new Exception(
						string.Format("rpc timeout {0} {1}", type, MongoHelper.ToJson(request))));
					return;
				}
				if (b == RpcStatus.Exception)
				{
					RpcExcetionInfo errorInfo = MongoHelper.FromBson<RpcExcetionInfo>(e, 8);
					tcs.SetException(new Exception(
						string.Format("rpc exception {0} {1} {2}", type, MongoHelper.ToJson(request), MongoHelper.ToJson(errorInfo))));
					return;
				}

				// RpcStatus.OK
				T response = MongoHelper.FromBson<T>(e, 6);
				tcs.SetResult(response);
			};

			if (waitTime > 0)
			{
				this.service.Timer.Add(TimeHelper.Now() + waitTime,
						() => { this.RequestCallback(channel, this.requestId, null, RpcStatus.Timeout); });
			}
			return tcs.Task;
		}

		/// <summary>
		/// Rpc响应
		/// </summary>
		public void Response<T>(AChannel channel, int id, T response)
		{
			byte[] responseBuffer = MongoHelper.ToBson(response);
			byte[] typeBuffer = BitConverter.GetBytes(0);
			byte[] idBuffer = BitConverter.GetBytes(id);
			channel.SendAsync(new List<byte[]> { typeBuffer, idBuffer, responseBuffer });
		}

		/// <summary>
		/// Rpc响应
		/// </summary>
		public void ResponseException(AChannel channel, int id, int errorCode, string errorInfo)
		{
			byte[] typeBuffer = BitConverter.GetBytes(0);
			byte[] idBuffer = BitConverter.GetBytes(id);
			RpcExcetionInfo info = new RpcExcetionInfo { ErrorCode = errorCode, ErrorInfo = errorInfo };
			byte[] responseBuffer = MongoHelper.ToBson(info);
			channel.SendAsync(new List<byte[]> { typeBuffer, idBuffer, responseBuffer });
		}
	}
}