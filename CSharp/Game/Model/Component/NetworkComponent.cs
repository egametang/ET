using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Common.Base;
using Common.Helper;
using Common.Network;
using MongoDB.Bson;
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
				byte[] messageBytes = await channel.RecvAsync();
				Opcode opcode = (Opcode)BitConverter.ToUInt16(messageBytes, 0);

				// rpc异常
				if (opcode == Opcode.RpcException)
				{
					int id = BitConverter.ToInt32(messageBytes, 2);
					this.RpcCallback(channel, id, messageBytes, RpcResponseStatus.Exception);
					continue;
				}

				// 表示消息是rpc响应消息
				if (opcode == Opcode.RpcResponse)
				{
					int id = BitConverter.ToInt32(messageBytes, 2);
					this.RpcCallback(channel, id, messageBytes, RpcResponseStatus.Succee);
					continue;
				}

				// 如果是server message(发给client的消息),说明这是gate server,需要根据unitid查到channel,进行发送
				if (MessageTypeHelper.IsServerMessage(opcode))
				{
					byte[] idBuffer = new byte[12];
					Array.Copy(messageBytes, 2, idBuffer, 0, 12);
					ObjectId unitId = new ObjectId(idBuffer);
					byte[] buffer = new byte[messageBytes.Length - 6];
					Array.Copy(messageBytes, 6, buffer, 0, buffer.Length);
					World.Instance.GetComponent<GateNetworkComponent>().SendAsync(unitId, buffer);
					continue;
				}

				// 处理Rpc请求,并且返回结果
				RpcDo(channel, opcode, messageBytes);
			}
		}

		private async static void RpcDo(AChannel channel, Opcode opcode, byte[] messageBytes)
		{
			byte[] opcodeBuffer;
			int id = BitConverter.ToInt32(messageBytes, 2);
			byte[] idBuffer = BitConverter.GetBytes(id);
			try
			{
				opcodeBuffer = BitConverter.GetBytes((ushort)Opcode.RpcResponse);
				byte[] result = await World.Instance.GetComponent<MessageComponent>().RunAsync(opcode, messageBytes);
				channel.SendAsync(new List<byte[]> { opcodeBuffer, idBuffer, result });
			}
			catch (Exception e)
			{
				opcodeBuffer = BitConverter.GetBytes((ushort)Opcode.RpcException);
				BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));
				using (MemoryStream stream = new MemoryStream())
				{
					formatter.Serialize(stream, e);
					channel.SendAsync(new List<byte[]> { opcodeBuffer, idBuffer, stream.ToArray() });
				}
			}
		}

		// 消息回调或者超时回调
		public void RpcCallback(AChannel channel, int id, byte[] buffer, RpcResponseStatus responseStatus)
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
		public Task<T> RpcCall<T, K>(string address, K request, int waitTime = 0)
		{
			AChannel channel = this.service.GetChannel(address);

			++this.requestId;
			byte[] requestBuffer = MongoHelper.ToBson(request);
			Opcode opcode = (Opcode)Enum.Parse(typeof(Opcode), request.GetType().Name);
			byte[] opcodeBuffer = BitConverter.GetBytes((ushort)opcode);
			byte[] idBuffer = BitConverter.GetBytes(this.requestId);
			channel.SendAsync(new List<byte[]> { opcodeBuffer, idBuffer, requestBuffer });
			var tcs = new TaskCompletionSource<T>();
			this.requestCallback[this.requestId] = (messageBytes, status) =>
			{
				if (status == RpcResponseStatus.Timeout)
				{
					tcs.SetException(new Exception(
						string.Format("rpc timeout {0} {1}", opcode, MongoHelper.ToJson(request))));
					return;
				}
				if (status == RpcResponseStatus.Exception)
				{
					BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));
					Exception exception;
					using (MemoryStream stream = new MemoryStream(messageBytes))
					{
						stream.Seek(6, SeekOrigin.Begin);
						exception = (Exception)formatter.Deserialize(stream);
					}
					tcs.SetException(exception);
					return;
				}

				// RpcResponseStatus.Succee
				T response = MongoHelper.FromBson<T>(messageBytes, 6);
				tcs.SetResult(response);
			};

			if (waitTime > 0)
			{
				this.service.Timer.Add(TimeHelper.Now() + waitTime,
						() => { this.RpcCallback(channel, this.requestId, null, RpcResponseStatus.Timeout); });
			}
			return tcs.Task;
		}
	}
}