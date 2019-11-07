namespace ETModel
{
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