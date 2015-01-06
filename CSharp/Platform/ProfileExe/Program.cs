using UNetTest;

namespace ENetExe
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			var test = new UServiceTest();
			test.ClientSendToServer();
		}
	}
}