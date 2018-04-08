namespace ETModel
{
	// 充值流水
	public sealed class RechargeRecord : Entity
	{
		// 充值玩家
		public int PlayerNO { get; set; }

		// 充值数量
		public int CardNumber { get; set; }

		// 充值时间
		public long Time { get; set; }

        public RechargeRecord() : base()
        {
        }

        public RechargeRecord(long id) : base(id)
		{
		}
	}

	// 保存玩家充值记录, 每个玩家只有一条
	public sealed class Recharge : Entity
	{
		public int CardNumber { get; set; }

		public long UpdateTime { get; set; }

        public Recharge() : base()
        {
        }

        public Recharge(long id) : base(id)
		{
		}
	}

	public class HttpResult
	{
		public int code;
		public bool status;
		public string msg = "";
		[MongoDB.Bson.Serialization.Attributes.BsonIgnoreIfNull]
		public object data;
	}

	public static class HttpErrorCode
	{
		public const int Exception = 999;
		public const int Success = 1000;
		public const int RpcFail = 1002;
	}
}