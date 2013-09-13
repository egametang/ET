using System;
using System.Collections.Generic;
using Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Object = Component.Object;

namespace ObjectTest
{
	class Buff: Object
	{
	}

	class Player: Object
	{
		public Dictionary<ObjectId, Buff> Buffs { get; private set; }

		public Player()
		{
			this.Buffs = new Dictionary<ObjectId, Buff>();
		}
	}

	[TestClass]
	public class ObjectTest
	{
		[TestMethod]
		public void Serialize()
		{
			var player = new Player();
			player["health"] = 10;
			for (int i = 0; i < 1; ++i)
			{
				var buff = new Buff();
				player.Buffs.Add(buff.Id, buff);
			}

			string json = MongoHelper.ToJson(player);
			Console.WriteLine(json);

			var player2 = MongoHelper.FromJson<Player>(json);
			Console.WriteLine(MongoHelper.ToJson(player2));
		}
	}
}
