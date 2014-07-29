using ENetTest;

namespace ENetExe
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var test = new ENetClientServerTest();
            test.ClientSendToServer();
        }
    }
}