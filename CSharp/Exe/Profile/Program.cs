using Common.Logger;
using TNetTest;

namespace Profile
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			TServiceTest test = new TServiceTest();
			Log.Debug("Profile start");
			test.ClientSendToServer();
			Log.Debug("Profile stop");
		}
	}
}