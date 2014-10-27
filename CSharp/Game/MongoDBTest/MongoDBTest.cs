using System;
using System.IO;
using System.Reflection;
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
            ConfigComponent configComponent = world.AddComponent<ConfigComponent>();
            configComponent.Load(typeof(World).Assembly);

            Assembly assembly = Assembly.Load(File.ReadAllBytes(@"./Controller.dll"));

            // 构造工厂
            FactoryComponent<Unit> factoryComponent = world.AddComponent<FactoryComponent<Unit>>();
            factoryComponent.Load(assembly);

            // 构造行为树
            BehaviorTreeComponent behaviorTreeComponent = world.AddComponent<BehaviorTreeComponent>();
            behaviorTreeComponent.Load(assembly);


            Unit player1 = factoryComponent.Create(1);
            Buff buff = new Buff(1);
            player1.GetComponent<BuffComponent>().Add(buff);
            player1["hp"] = 10;

            collection.Insert(player1);

            var query = Query<Unit>.EQ(e => e.Id, player1.Id);
            Unit player2 = collection.FindOne(query);
            
            Console.WriteLine(MongoHelper.ToJson(player2));
            Assert.AreEqual(MongoHelper.ToJson(player1), MongoHelper.ToJson(player2));
        }
    }
}
