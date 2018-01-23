using MongoDB.Driver;

namespace Model
{
	[ObjectSystem]
	public class DbComponentSystem : ObjectSystem<DBComponent>, IAwake
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
			//DBConfig config = Game.Scene.GetComponent<StartConfigComponent>().StartConfig.GetComponent<DBConfig>();
			//string connectionString = config.ConnectionString;
			//mongoClient = new MongoClient(connectionString);
			//this.database = this.mongoClient.GetDatabase(config.DBName);
		}

		public IMongoCollection<Disposer> GetCollection(string name)
		{
			return this.database.GetCollection<Disposer>(name);
		}
	}
}