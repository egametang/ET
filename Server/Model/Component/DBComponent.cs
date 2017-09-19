using MongoDB.Driver;

namespace Model
{
	[ObjectEvent]
	public class DBComponentEvent : ObjectEvent<DBComponent>, IStart
	{
		public void Start()
		{
			this.Get().Start();
		}
	}

	/// <summary>
	/// 连接mongodb
	/// </summary>
	public class DBComponent : Component
	{
		public MongoClient mongoClient;
		public IMongoDatabase database;

		public void Start()
		{
			//DBConfig config = Game.Scene.GetComponent<StartConfigComponent>().StartConfig.GetComponent<DBConfig>();
			//string connectionString = config.ConnectionString;
			//mongoClient = new MongoClient(connectionString);
			//this.database = this.mongoClient.GetDatabase(config.DBName);
		}

		public IMongoCollection<Entity> GetCollection(string name)
		{
			return this.database.GetCollection<Entity>(name);
		}
	}
}