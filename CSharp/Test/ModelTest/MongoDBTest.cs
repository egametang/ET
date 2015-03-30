using System;
using Common.Helper;
using Model;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace MongoDBTest
{
	[TestFixture]
	public class MongoDBTest
	{
		[Test]
		public async void TestMongoDB()
		{
			const string connectionString = "mongodb://localhost";
			MongoClient client = new MongoClient(connectionString);
			IMongoDatabase database = client.GetDatabase("test");
			IMongoCollection<Unit> collection = database.GetCollection<Unit>("Unit");

			World world = World.Instance;

			// 加载配置
			world.AddComponent<ConfigComponent>();
			world.AddComponent<EventComponent<EventAttribute>>();
			world.AddComponent<TimerComponent>();
			world.AddComponent<UnitComponent>();
			world.AddComponent<FactoryComponent<Unit>>();
			world.AddComponent<BehaviorTreeComponent>();
			world.AddComponent<NetworkComponent>();
			world.Load();

			Unit player1 = world.GetComponent<FactoryComponent<Unit>>().Create(UnitType.GatePlayer, 1);
			player1["hp"] = 10;

			await collection.InsertOneAsync(player1);

			Unit player3 = player1.Clone();

			Assert.AreEqual(MongoHelper.ToJson(player1), MongoHelper.ToJson(player3));

			//Thread.Sleep(20 * 1000);
			//world.Load();
			//
			//Assert.AreEqual(MongoHelper.ToJson(player1), MongoHelper.ToJson(player2));
		}

		[Test]
		public void Test()
		{
			ObjectId id = ObjectId.GenerateNewId();
			byte[] bytes = id.ToByteArray();
			Console.WriteLine(bytes.Length);
		}
	}
}