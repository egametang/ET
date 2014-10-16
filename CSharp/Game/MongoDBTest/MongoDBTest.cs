using System;
using Common.Helper;
using Controller;
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
            var collection = database.GetCollection<GameObject>("GameObjects");

            GameObject player1 = GameObjectFactory.CreatePlayer();
            player1.GetComponent<BuffComponent>().Add(new Buff(1));
            player1["hp"] = 10;

            collection.Insert(player1);

            var query = Query<GameObject>.EQ(e => e.Id, player1.Id);
            GameObject player2 = collection.FindOne(query);

            Console.WriteLine(MongoHelper.ToJson(player2));
            Assert.AreEqual(MongoHelper.ToJson(player1), MongoHelper.ToJson(player2));
        }
    }
}
