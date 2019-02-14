using System;
using System.IO;
using ETModel;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Example3_5
{
    class Program
    {
        static void Main(string[] args)
        {
            Player player1 = new Player() { Id = 1 };
            string json = player1.ToJson();
            Console.WriteLine($"player1 to json: {json}");

            byte[] bson = player1.ToBson();
            Console.WriteLine($"player1 to bson: {bson.ToHex()}");
            // // player1 to json: { "_id" : NumberLong(1), "C" : [], "Account" : null, "UnitId" : NumberLong(0) }

            // 反序列化json
            Player player11 = BsonSerializer.Deserialize<Player>(json);
            Console.WriteLine($"player11 to json: {player11.ToJson()}");
            // 反序列化bson
            using (MemoryStream memoryStream = new MemoryStream(bson))
            {
                Player player12 = (Player) BsonSerializer.Deserialize(memoryStream, typeof (Player));
                Console.WriteLine($"player12 to json: {player12.ToJson()}");
            }
            
            
            // 使用标准json
            Console.WriteLine($"player to Strict json: {player1.ToJson(new JsonWriterSettings() {OutputMode = JsonOutputMode.Strict})}");
            // player1 to Strict json: { "_id" : 1, "C" : [], "Account" : null, "UnitId" : 0 }
        }
    }
}