using Component;
using Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComponentTest
{
	[TestClass]
	public class EntityTest
	{
		[TestMethod]
		public void TestSerialize()
		{
			var entity = new Entity();
			entity["health"] = 10L;
			string entityString = MongoHelper.ToJson(entity);

			var entity2 = MongoHelper.FromJson<Entity>(entityString);
			Assert.AreEqual(10, entity2.Get<long>("health"));
		}
	}
}
