using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class C2S_LoginGate
	{
		public string Account;
		
		public string Passwd;
		
		public string Token;
		
		public string Mac;

	}

	[BsonIgnoreExtraElements]
	public class ReconnectBattle
	{
		public long RoomGuid;
		
		public string BattleIp;
		
		public short BattlePort;

	}

	[BsonIgnoreExtraElements]
	public class S2C_LoginGate: IErrorMessage
	{
		public ErrorMessage Errmsg;
		
		public long PlayerGuid;
		
		public string PlayerName;

		//1 登录大厅
		//2 断线重连
		//3 取名字
		public int Type;
		
		public ReconnectBattle Reconnect;

		public ErrorMessage ErrorMessage { get { return this.Errmsg; } }
	}

	[BsonIgnoreExtraElements]
	public class C2S_FetchServerTime
	{
	}

	[BsonIgnoreExtraElements]
	public class S2C_FetchServerTime: IErrorMessage
	{
		public ErrorMessage Errmsg;

		//服务器的真实时间
		public long ServerTime;

		public ErrorMessage ErrorMessage { get { return this.Errmsg; } }
	}

	[BsonIgnoreExtraElements]
	public class C2S_LogoutGate
	{
	}

	[BsonIgnoreExtraElements]
	public class S2C_LogoutGate: IErrorMessage
	{
		public ErrorMessage Errmsg;

		public ErrorMessage ErrorMessage { get { return this.Errmsg; } }
	}
	
	public class S2C_InitBuffInfo
	{
		/// <summary>
		/// buff所有者
		/// </summary>
		public int UnitGuid;

		public int BuffGuid;

		public int BuffId;

		public int Level = 1;

		//叠层数量
		public int StackCount = 1;

		//总时间和剩余持续时间，单位毫秒,如果是0，表示一个永久的BUFF
		public int TotalTime = 0;

		public long ExpiredTick = 0;

		//来源GUID
		public int CasterGuid = 0;

	}
}
