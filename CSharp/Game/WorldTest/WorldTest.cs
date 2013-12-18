using System.Threading;
using Component;
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
			world.Logic.Handle(Opcode.LoginWorld, MongoHelper.ToBson(new CLoginWorld()));
			int count = 2;
			while (--count != 0)
			{
				world.Logic.Handle(Opcode.ReloadHandler, "tanghai".ToByteArray());
				Thread.Sleep(1);
				world.Logic.Reload();
			}
		}
	}
}
