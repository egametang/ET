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
			world.Enter(3, "tanghai".ToByteArray());
			int count = 2;
			while (--count != 0)
			{
				world.Enter(1, "tanghai".ToByteArray());
				Thread.Sleep(1);
				world.ReloadLogic();
			}
		}
	}
}
