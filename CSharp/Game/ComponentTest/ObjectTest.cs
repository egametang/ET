using System;
using Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.IO;
using Object = Component.Object;

namespace ObjectTest
{
	class Buff: Object
	{
	}

	[TestClass]
	public class ObjectTest
	{
		[TestMethod]
		public void TestSerialize()
		{
			var buff = new Buff();
			buff.Values["health"] = 10L;
			string json = MongoHelper.ToJson(buff);

			var buff2 = MongoHelper.FromJson<Buff>(json);

			Assert.AreEqual(10L, buff2.Values["health"]);
		}
	}
}
