using MongoDB.Bson;
using MongoDB.Driver;

namespace Model
{
	[ObjectEvent]
	public class DBComponentEvent : ObjectEvent<DBComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}

	/// <summary>
	/// 连接mongodb
	/// </summary>
	public class DBComponent : Component
	{
		public MongoClient mongoClient;
		public IMongoDatabase database;

		public void Awake()
		{
			DBConfig config = Game.Scene.GetComponent<StartConfigComponent>().StartConfig.GetComponent<DBConfig>();
			string connectionString = config.ConnectionString;
            Log.Debug("connection string " + connectionString);
			mongoClient = new MongoClient(connectionString);
			this.database = this.mongoClient.GetDatabase(config.DBName);
		    //var collection = this.database.GetCollection<BsonDocument>("user");
		}

		public IMongoCollection<Entity> GetCollection(string name)
		{
			return this.database.GetCollection<Entity>(name);
		}
	}
}