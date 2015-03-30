using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;
using Common.Helper;
using Common.Network;
using MongoDB.Bson;
using TNet;
using UNet;

namespace Model
{
	public class NetworkComponent: Component<World>, IUpdate, IStart
	{
		private IService service;

		private int requestId;

		private readonly Dictionary<int, Action<byte[], bool>> requestCallback = new Dictionary<int, Action<byte[], bool>>();

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

				if (MessageTypeHelper.IsClientMessage(opcode))
				{
					continue;
				}

				// 这个区间表示消息是rpc响应消息
				if (MessageTypeHelper.IsRpcResponseMessage(opcode))
				{
					int id = BitConverter.ToInt32(message, 2);
					this.RequestCallback(channel, id, message, true);
					continue;
				}

				// 进行消息分发
				World.Instance.GetComponent<EventComponent<MessageAttribute>>().RunAsync(opcode, env);
			}
		}

		// 消息回调或者超时回调
		public void RequestCallback(AChannel channel, int id, byte[] buffer, bool isOK)
		{
			Action<byte[], bool> action;
			if (this.requestCallback.TryGetValue(id, out action))
			{
				action(buffer, isOK);
			}
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
				if (b)
				{
					T response = MongoHelper.FromBson<T>(e, 6);
					tcs.SetResult(response);
				}
				else
				{
					tcs.SetException(new Exception(string.Format("rpc timeout {0} {1}", type, MongoHelper.ToJson(request))));
				}
			};

			if (waitTime > 0)
			{
				this.service.Timer.Add(TimeHelper.Now() + waitTime, () =>
				{
					this.RequestCallback(channel, this.requestId, null, false);
				});
			}
			return tcs.Task;
		}

		/// <summary>
		/// Rpc响应
		/// </summary>
		public void Response<T>(AChannel channel, short type, int id, T response)
		{
			byte[] responseBuffer = MongoHelper.ToBson(response);
			byte[] typeBuffer = BitConverter.GetBytes(type);
			byte[] idBuffer = BitConverter.GetBytes(id);
			channel.SendAsync(new List<byte[]> { typeBuffer, idBuffer, responseBuffer });
		}
	}
}