
namespace ENetExe
{
	static class Program
	{
		static void Main(string[] args)
		{
			var test = new ENetTest.ENetClientServerTest();
			test.ClientSendToServer();
		}
	}
}
