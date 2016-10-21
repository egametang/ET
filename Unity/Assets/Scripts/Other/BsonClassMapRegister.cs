using MongoDB.Bson.Serialization;

namespace Model
{
	public static class BsonClassMapRegister
	{
		private static bool isRegister;

		public static void Register()
		{
			if (isRegister)
			{
				return;
			}
			isRegister = true;

			BsonClassMap.RegisterClassMap<InnerConfig>();
			BsonClassMap.RegisterClassMap<OuterConfig>();
		}
	}
}