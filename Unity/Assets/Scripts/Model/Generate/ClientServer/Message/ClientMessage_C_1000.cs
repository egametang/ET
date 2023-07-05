using ET;
using MemoryPack;
using System.Collections.Generic;
namespace ET
{
// using
	[ResponseType(nameof(NetClient2Main_Login))]
	[Message(ClientMessage.Main2NetClient_Login)]
	[MemoryPackable]
	public partial class Main2NetClient_Login: MessageObject, IActorRequest
	{
		public static Main2NetClient_Login Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new Main2NetClient_Login() : ObjectPool.Instance.Fetch(typeof(Main2NetClient_Login)) as Main2NetClient_Login; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public string Account { get; set; }

		[MemoryPackOrder(2)]
		public string Password { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Account = default;
			this.Password = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	[Message(ClientMessage.NetClient2Main_Login)]
	[MemoryPackable]
	public partial class NetClient2Main_Login: MessageObject, IActorResponse
	{
		public static NetClient2Main_Login Create(bool isFromPool = true) 
		{ 
			return !isFromPool? new NetClient2Main_Login() : ObjectPool.Instance.Fetch(typeof(NetClient2Main_Login)) as NetClient2Main_Login; 
		}

		[MemoryPackOrder(0)]
		public int RpcId { get; set; }

		[MemoryPackOrder(1)]
		public int Error { get; set; }

		[MemoryPackOrder(2)]
		public string Message { get; set; }

		[MemoryPackOrder(3)]
		public long PlayerId { get; set; }

		public override void Dispose() 
		{
			if (!this.IsFromPool) return;
			this.RpcId = default;
			this.Error = default;
			this.Message = default;
			this.PlayerId = default;
			
			ObjectPool.Instance.Recycle(this); 
		}

	}

	public static class ClientMessage
	{
		 public const ushort Main2NetClient_Login = 1001;
		 public const ushort NetClient2Main_Login = 1002;
	}
}
