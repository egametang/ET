using System;
using Common.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDBTest
{
    [TestClass]
    public class MongoDBTest
    {
        [TestMethod]
        public void TestMongoDB()
        {
            const string connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase("test");
            var collection = database.GetCollection<Unit>("Unit");


            World world = World.Instance;

            // 加载配置
            world.AddComponent<ConfigComponent>();
            world.AddComponent<EventComponent<WorldEventAttribute>>();
            world.AddComponent<UnitComponent>();
            world.AddComponent<FactoryComponent<Unit>>();
            world.AddComponent<BehaviorTreeComponent>();
            world.Load();


            Unit player1 = world.GetComponent<FactoryComponent<Unit>>().Create(1);
            player1["hp"] = 10;

            collection.Insert(player1);

            var query = Query<Unit>.EQ(e => e.Id, player1.Id);
            Unit player2 = collection.FindOne(query);
            
            Console.WriteLine(MongoHelper.ToJson(player2));
            Assert.AreEqual(MongoHelper.ToJson(player1), MongoHelper.ToJson(player2));
        }
    }
}
