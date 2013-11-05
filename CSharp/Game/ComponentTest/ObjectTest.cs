using System;
using Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Object = Component.Object;

namespace ObjectTest
{
	class Player: Object
	{
	}

	[TestClass]
	public class ObjectTest
	{
		[TestMethod]
		public void Serialize()
		{
			var player = new Player();
			player["health"] = 10;

			string json = MongoHelper.ToJson(player);
			Console.WriteLine(json);

			var player2 = MongoHelper.FromJson<Player>(json);
			Console.WriteLine(MongoHelper.ToJson(player2));
		}
	}
}
