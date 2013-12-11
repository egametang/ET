using World;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorldTest
{
	[TestClass]
	public class BuffManagerTest
	{
		private TestContext testContextInstance;

		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		[TestMethod]
		public void TestAdd()
		{
			var buffManager = new BuffManager();
			var buff = new Buff { Type = 1 };
			buffManager.Add(buff);
			var getBuff = buffManager.GetById(buff.Id);
			Assert.AreSame(buff, getBuff);
		}
	}
}
