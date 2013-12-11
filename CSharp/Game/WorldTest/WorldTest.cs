using System.Threading;
using Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WorldTest
{
	[TestClass]
	public class WorldTest
	{
		[TestMethod]
		public void TestReload()
		{
			var world = World.World.Instance;
			world.Dispatcher(1, "tanghai".ToByteArray());
			int count = 10;
			while (--count != 0)
			{
				world.Dispatcher(1, "tanghai".ToByteArray());
				Thread.Sleep(10000);
				world.Reload();
			}
		}
	}
}
