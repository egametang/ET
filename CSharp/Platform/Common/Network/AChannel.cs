using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base;
using Common.Helper;

namespace Common.Network
{
	[Flags]
	public enum PacketFlags
	{
		None = 0,
		Reliable = 1 << 0,
		Unsequenced = 1 << 1,
		NoAllocate = 1 << 2
	}

	public abstract class AChannel: Entity<AChannel>, IDisposable
	{
		protected IService service;
		private int requestId;
		protected Action<AChannel> onDispose = channel => { };
		private readonly Dictionary<int, Action<byte[], bool>> requestCallback = new Dictionary<int, Action<byte[], bool>>();

		protected AChannel(IService service)
		{
			this.service = service;
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		public abstract void SendAsync(
				byte[] buffer, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable);

		public abstract void SendAsync(
				List<byte[]> buffers, byte channelID = 0, PacketFlags flags = PacketFlags.Reliable);

		/// <summary>
		/// 接收消息
		/// </summary>
		public abstract Task<byte[]> RecvAsync();

		public abstract Task<bool> DisconnnectAsync();

		public abstract string RemoteAddress { get; }

		public event Action<AChannel> OnDispose
		{
			add
			{
				this.onDispose += value;
			}
			remove
			{
				this.onDispose -= value;
			}
		}

		public abstract void Dispose();

		// 消息回调或者超时回调
		public void RequestCallback(int id, byte[] buffer, bool isOK)
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
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="K"></typeparam>
		/// <param name="type"></param>
		/// <param name="request"></param>
		/// <param name="waitTime"></param>
		/// <returns></returns>
		public Task<T> Request<T, K>(short type, K request, int waitTime = 0)
		{
			++this.requestId;
			byte[] requestBuffer = MongoHelper.ToBson(request);
			byte[] typeBuffer = BitConverter.GetBytes(type);
			byte[] idBuffer = BitConverter.GetBytes(this.requestId);
			this.SendAsync(new List<byte[]> { typeBuffer, idBuffer, requestBuffer });
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
				this.service.Timer.Add(TimeHelper.Now() + waitTime, () => { this.RequestCallback(this.requestId, null, false); });
			}
			return tcs.Task;
		}

		/// <summary>
		/// Rpc响应
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="id"></param>
		/// <param name="response"></param>
		public void Response<T>(short type, int id, T response)
		{
			byte[] responseBuffer = MongoHelper.ToBson(response);
			byte[] typeBuffer = BitConverter.GetBytes(type);
			byte[] idBuffer = BitConverter.GetBytes(id);
			this.SendAsync(new List<byte[]> { typeBuffer, idBuffer, responseBuffer });
		}
	}
}