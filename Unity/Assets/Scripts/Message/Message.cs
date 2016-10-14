using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class C2S_Login
	{
		public string Account;
		public string Password;
	}

	[BsonIgnoreExtraElements]
	public class S2C_Login: IErrorMessage
	{
		public ErrorMessage ErrorMessage { get; set; }
	}
}
