using System;
using Common.Helper;
using Model;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;

namespace MongoDBTest
{
	[TestFixture]
	public class MongoDBTest
	{
		[Test]
		public void TestMongoDB()
		{
			const string connectionString = "mongodb://localhost";
			MongoClient client = new MongoClient(connectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase("test");
			MongoCollection<Unit> collection = database.GetCollection<Unit>("Unit");

			World world = World.Instance;

			// 加载配置
			world.AddComponent<ConfigComponent>();
			world.AddComponent<EventComponent<ActionAttribute>>();
			world.AddComponent<EventComponent<EventAttribute>>();
			world.AddComponent<TimerComponent>();
			world.AddComponent<UnitComponent>();
			world.AddComponent<FactoryComponent<Unit>>();
			world.AddComponent<BehaviorTreeComponent>();
			world.Load();

			Unit player1 = world.GetComponent<FactoryComponent<Unit>>().Create(UnitType.GatePlayer, 1);
			player1["hp"] = 10;

			collection.Insert(player1);

			IMongoQuery query = Query<Unit>.EQ(e => e.Id, player1.Id);
			Unit player2 = collection.FindOne(query);

			Console.WriteLine(MongoHelper.ToJson(player2));
			Assert.AreEqual(MongoHelper.ToJson(player1), MongoHelper.ToJson(player2));

			Unit player3 = player1.Clone();

			Assert.AreEqual(MongoHelper.ToJson(player1), MongoHelper.ToJson(player3));

			//Thread.Sleep(20 * 1000);
			//world.Load();
			//
			//Assert.AreEqual(MongoHelper.ToJson(player1), MongoHelper.ToJson(player2));
		}
	}
}